using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Interfaces
{
    public interface IBeansDbRepository
    {
        Task<List<BeanDbRecord>> GetAllRecordsAsync();
        Task<BeanDbRecord?> GetRecordByIndexAsync(int id);
        Task<BeanDbRecord?> GetBeanOfTheDayRecordAsync();
        Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record, CancellationToken cancellationToken);
        Task<bool> UpdateBeanRecordAsync(BeanDbRecord originalRecord, BeanDbRecord updatedRecord, CancellationToken cancellationToken);
        Task<bool> DeleteRecordByIndexAsync(BeanDbRecord record, CancellationToken cancellationToken);
        Task<bool> SetBeanOfTheDayAsync(int currentBOTDIndex, int newBOTDIndex);
        Task<BeanOfTheDayDbRecord> InsertRecordAsync(BeanOfTheDayDbRecord record);
    }
}