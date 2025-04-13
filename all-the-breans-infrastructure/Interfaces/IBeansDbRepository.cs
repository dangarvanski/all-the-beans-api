using all_the_breans_sharedKernal.Entities;

namespace all_the_breans_infrastructure.Interfaces
{
    public interface IBeansDbRepository
    {
        Task<int> GetRecordCount(CancellationToken cancellationToken);
        Task UpdateCountAsync(CancellationToken cancellationToken);
        Task<List<BeanDbRecord>> GetAllRecordsAsync(int page, int pageSize);
        Task<List<int>> GetAllIndexesForRecordsAsync();
        Task<BeanDbRecord?> GetRecordByIndexAsync(int id);
        Task<BeanDbRecord?> GetBeanOfTheDayRecordAsync();
        Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record, CancellationToken cancellationToken);
        Task<bool> UpdateBeanRecordAsync(BeanDbRecord updatedRecord, CancellationToken cancellationToken);
        Task<bool> DeleteRecordByIndexAsync(BeanDbRecord record, CancellationToken cancellationToken);
        Task SetBeanOfTheDayAsync(int currentBOTDIndex, int newBOTDIndex);
    }
}