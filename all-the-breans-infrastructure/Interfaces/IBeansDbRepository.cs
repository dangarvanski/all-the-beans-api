using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Interfaces
{
    public interface IBeansDbRepository
    {
        DbSet<BeanDbRecord> Beans { get; set; }
        Task<List<BeanDbRecord>> GetAllRecordsAsync();
        Task<BeanDbRecord?> GetRecordByIndexAsync(int id);
        Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record);
        Task<BeanOfTheDayDbRecord> InsertRecordAsync(BeanOfTheDayDbRecord record);
    }
}