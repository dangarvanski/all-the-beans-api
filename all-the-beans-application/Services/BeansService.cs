using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace all_the_beans_application.Services
{
    public class BeansService : BackgroundService, IBeansService
    {
        private readonly IBeansDbRepository _appDbContext;
        private readonly ILogger<BeansService> _logger;
        private readonly TimeSpan _startTime = TimeSpan.FromHours(0); // Midnight

        public BeansService(IBeansDbRepository appDbContext, ILogger<BeansService> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for SetBeanOfTheDay due to {exception.Message}. Retrying in {timeSpan.TotalSeconds} seconds.");
                    });

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = CalculateNextRunDelay(DateTime.Now);
                    _logger.LogInformation($"Scheduling next Bean of the Day update in {delay.TotalHours:F2} hours at {DateTime.Now + delay:yyyy-MM-dd HH:mm:ss}.");
                    await Task.Delay(delay, stoppingToken);

                    await retryPolicy.ExecuteAsync(async () => await SetBeanOfTheDay());
                    _logger.LogInformation("Bean of the Day updated successfully.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("BeansService execution canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in BeansService. Will retry on next cycle.");
                }
            }
        }

        public virtual TimeSpan CalculateNextRunDelay(DateTime currentTime)
        {
            var nextRun = DateTime.Today.AddDays(1).Add(_startTime); // Tomorrow at midnight
            if (currentTime > nextRun)
            {
                nextRun = nextRun.AddDays(1); // If we've passed midnight today, schedule for tomorrow
            }

            return nextRun - currentTime;
        }

        public async Task SetBeanOfTheDay()
        {
            var beansIndexes = await _appDbContext.GetAllIndexesForRecordsAsync();
            if (beansIndexes == null || !beansIndexes.Any())
            {
                _logger.LogWarning("No beans available to set as Bean of the Day.");
                return;
            }

            var currentBOTD = await _appDbContext.GetBeanOfTheDayRecordAsync();
            int newBOTDIndex;

            if (currentBOTD == null)
            {
                // If no BOTD exists, pick any bean
                var randomNumber = GenerateNumWithinRange(beansIndexes.Count());
                newBOTDIndex = beansIndexes[randomNumber];
                _logger.LogInformation($"No current BOTD. Selected new BOTD with index {newBOTDIndex}.");
            }
            else
            {
                // Pick a different bean than the current BOTD
                do
                {
                    var randomNumber = GenerateNumWithinRange(beansIndexes.Count());
                    newBOTDIndex = beansIndexes[randomNumber];
                } while (newBOTDIndex == currentBOTD.index);
                _logger.LogInformation($"Current BOTD index: {currentBOTD.index}. Selected new BOTD with index {newBOTDIndex}.");
            }

            var previousIndex = currentBOTD?.index ?? -1;
            await _appDbContext.SetBeanOfTheDayAsync(previousIndex, newBOTDIndex);
        }

        private int GenerateNumWithinRange(int upperLimit)
        {
            Random rnd = new Random();
            return rnd.Next(0, upperLimit);
        }
    }
}
