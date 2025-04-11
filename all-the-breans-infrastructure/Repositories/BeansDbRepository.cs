using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Repositories
{
    public class BeansDbRepository : DbContext, IBeansDbRepository
    {
        private DbSet<BeanDbRecord> Beans { get; set; }
        private DbSet<BeanOfTheDayDbRecord> BeanOfTheDay { get; set; }

        public BeansDbRepository(DbContextOptions<BeansDbRepository> options) : base(options)
        {
        }

        public async Task<List<BeanDbRecord>> GetAllRecordsAsync()
        {
            return await Beans.OrderByDescending(x => x.index).ToListAsync();
        }

        public async Task<BeanDbRecord?> GetRecordByIndexAsync(int index)
        {
            return await Beans.FirstOrDefaultAsync(x => x.index == index);
        }

        public async Task<BeanDbRecord?> GetBeanOfTheDayRecordAsync()
        {
            return await Beans.FirstOrDefaultAsync(x => x.IsBOTD == true);
        }

        public async Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record, CancellationToken cancellationToken)
        {
            try
            {
                Beans.Add(record);
                await SaveChangesAsync(cancellationToken);
                return record;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to create new record", ex);
            }
        }

        public async Task<bool> UpdateBeanRecordAsync(BeanDbRecord originalRecord, BeanDbRecord updatedRecord, CancellationToken cancellationToken)
        {
            try
            {
                Entry(originalRecord).CurrentValues.SetValues(updatedRecord);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to update record", ex);
            }
        }

        public async Task<bool> DeleteRecordByIndexAsync(BeanDbRecord record , CancellationToken cancellationToken)
        {
            try
            {
                Beans.Remove(record);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to delete record", ex);
            }
        }

        public async Task SetBeanOfTheDayAsync(int currentBOTDIndex, int newBOTDIndex)
        {
            try
            {
                var currentBOTD = await Beans.FirstOrDefaultAsync(x => x.index == currentBOTDIndex);
                var newBOTD = await Beans.FirstOrDefaultAsync(x => x.index == newBOTDIndex);

                currentBOTD!.IsBOTD = false;
                newBOTD!.IsBOTD = true;
                await SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to set new bean of the day", ex);
            }
        }

        public async Task<BeanOfTheDayDbRecord> InsertRecordAsync(BeanOfTheDayDbRecord record)
        {
            BeanOfTheDay.Add(record);          // Add the record to the DbSet
            await SaveChangesAsync();   // Save changes to the database
            return record;              // Return the inserted record
        }
    }
}
