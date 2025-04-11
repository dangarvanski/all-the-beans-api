using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using System;

namespace all_the_beans_application.Services
{
    public class BeansService : IBeansService
    {
        private readonly IBeansDbRepository _appDbContext;

        public BeansService(IBeansDbRepository appDbContext)
        {
            _appDbContext = appDbContext;
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
