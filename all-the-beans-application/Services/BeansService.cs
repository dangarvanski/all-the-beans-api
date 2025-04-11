using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;

namespace all_the_beans_application.Services
{
    public class BeansService : IBeansService
    {
        private readonly IBeansDbRepository _appDbContext;

        public BeansService(IBeansDbRepository appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<BeanDbRecord?> GetRecordByIndexAsync(int id)
        {
            return await _appDbContext.GetRecordByIndexAsync(id);
        }

        public async Task<BeanOfTheDayDbRecord> AddBeanOfTheDay()
        {
            var beanOfTheDay = _appDbContext.GetAllRecordsAsync().Result.FirstOrDefault(x => x.IsBOTD == true);
            var beanOfTheDayRecord = new BeanOfTheDayDbRecord
            {
                _id = Guid.NewGuid(),
                BeanIndex = beanOfTheDay.index,
                Date = DateTime.Now
            };

            return await _appDbContext.InsertRecordAsync(beanOfTheDayRecord);
        }
    }
}
