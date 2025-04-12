using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Polly;

namespace all_the_beans_application.Services
{
    public class BeansService : BackgroundService, IBeansService
    {
        private readonly IBeansDbRepository _appDbContext;
        private readonly TimeSpan _startTime = TimeSpan.FromHours(0); // Midnight

        public BeansService(IBeansDbRepository appDbContext)
        {
            _appDbContext = appDbContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1).Add(_startTime); // Tomorrow at midnight
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                await retryPolicy.ExecuteAsync(SetBeanOfTheDay);
            }
        }

        private async Task SetBeanOfTheDay()
        {
            var beansIndexes = await _appDbContext.GetAllIndexesForRecordsAsync();
            var currentBOTD = await _appDbContext.GetBeanOfTheDayRecordAsync();

            int newBOTDIndex;
            do
            {
                var randomNumber = GenerateNumWithinRange(beansIndexes.Count());
                newBOTDIndex = beansIndexes[randomNumber];
            } while (newBOTDIndex == currentBOTD!.index);

            await _appDbContext.SetBeanOfTheDayAsync(currentBOTD!.index, newBOTDIndex);
        }

        private int GenerateNumWithinRange(int upperLimit)
        {
            Random rnd = new Random();
            return rnd.Next(0, upperLimit);
        }
    }
}
