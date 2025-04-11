using all_the_breans_sharedKernal.Entities;

namespace all_the_beans_application.Interfaces
{
    public interface IBeansService
    {
        Task<BeanDbRecord?> GetRecordByIndexAsync(int id);
        Task<BeanOfTheDayDbRecord> AddBeanOfTheDay();
    }
}