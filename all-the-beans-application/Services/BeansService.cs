using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1).Add(_startTime); // Tomorrow at midnight
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                SetBeanOfTheDay();
            }
        }

        public async void SetBeanOfTheDay()
        {
            var beansIndexes = (await _appDbContext.GetAllRecordsAsync()).Select(x => x.index).ToList();
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
