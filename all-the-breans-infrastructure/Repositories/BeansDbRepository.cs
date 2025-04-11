using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace all_the_breans_infrastructure.Repositories
{
    public class BeansDbRepository : DbContext , IBeansDbRepository
    {
        public DbSet<BeanDbRecord> Beans { get; set; }
        public DbSet<BeanOfTheDayDbRecord> BeanOfTheDay { get; set; }

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

        public async Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record, CancellationToken cancellationToken)
        {
            Beans.Add(record);
            await SaveChangesAsync(cancellationToken);
            return record;
        }

        public async Task<bool> UpdateBeanRecordAsync(BeanDbRecord originalRecord, BeanDbRecord updatedRecord, CancellationToken cancellationToken)
        {
            Entry(originalRecord).CurrentValues.SetValues(updatedRecord);
            await SaveChangesAsync(cancellationToken);
            return true;

        }

        public async Task<bool> DeleteRecordByIndexAsync(BeanDbRecord record , CancellationToken cancellationToken)
        {
            Beans.Remove(record);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<BeanOfTheDayDbRecord> InsertRecordAsync(BeanOfTheDayDbRecord record)
        {
            BeanOfTheDay.Add(record);          // Add the record to the DbSet
            await SaveChangesAsync();   // Save changes to the database
            return record;              // Return the inserted record
        }
    }
}
