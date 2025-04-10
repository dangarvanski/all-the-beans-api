using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record)
        {
            Beans.Add(record);
            await SaveChangesAsync();
            return record;
        }

        public async Task<BeanOfTheDayDbRecord> InsertRecordAsync(BeanOfTheDayDbRecord record)
        {
            BeanOfTheDay.Add(record);          // Add the record to the DbSet
            await SaveChangesAsync();   // Save changes to the database
            return record;              // Return the inserted record
        }
    }
}
