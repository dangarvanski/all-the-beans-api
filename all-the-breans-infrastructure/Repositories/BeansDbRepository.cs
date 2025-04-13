using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Repositories
{
    public class BeansDbRepository(DbContextOptions<BeansDbRepository> options) : DbContext(options), IBeansDbRepository
    {
        private DbSet<BeanDbRecord> Beans { get; set; }
        private int RecordCount = 0;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BeanDbRecord>()
                .HasKey(b => b._id);
            modelBuilder.Entity<BeanDbRecord>()
                .HasIndex(b => b.index)
                .IsUnique();
        }

        public async Task<int> GetRecordCount(CancellationToken cancellationToken)
        {
            await UpdateCountAsync(cancellationToken);
            return RecordCount;
        }

        public async Task UpdateCountAsync(CancellationToken cancellationToken = default)
        {
            RecordCount = await Beans.CountAsync(cancellationToken);
        }

        public async Task<List<BeanDbRecord>> GetAllRecordsAsync(int page, int pageSize)
        {
            return await Beans
                .AsNoTracking()
                .OrderByDescending(x => x.index)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<int>> GetAllIndexesForRecordsAsync()
        {
            return await Beans
                .AsNoTracking()
                .Select(b => b.index)
                .ToListAsync();
        }

        public async Task<BeanDbRecord?> GetRecordByIndexAsync(int index)
        {
            return await Beans
                .FirstOrDefaultAsync(x => x.index == index); ;
        }

        public async Task<BeanDbRecord?> GetBeanOfTheDayRecordAsync()
        {
            return await Beans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsBOTD == true);
        }

        public async Task<BeanDbRecord> InsertNewBeanRecordAsync(BeanDbRecord record, CancellationToken cancellationToken)
        {
            try
            {
                Beans.Add(record);
                await SaveChangesAsync(cancellationToken);
                await UpdateCountAsync(cancellationToken);
                return record;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to create new record", ex);
            }
        }

        public async Task<bool> UpdateBeanRecordAsync(BeanDbRecord updatedRecord, CancellationToken cancellationToken)
        {
            if (!await Beans.AnyAsync(b => b._id == updatedRecord._id, cancellationToken))
            {
                return false;
            }

            var entry = Entry(updatedRecord);
            if (entry.State == EntityState.Detached)
            {
                Beans.Update(updatedRecord);
                entry = Entry(updatedRecord);
            }

            try
            {
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to update record", ex);
            }
        }

        public async Task<bool> DeleteRecordByIndexAsync(BeanDbRecord record, CancellationToken cancellationToken)
        {
            try
            {
                Beans.Remove(record);
                await SaveChangesAsync(cancellationToken);
                await UpdateCountAsync(cancellationToken);
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
                var currentBOTD = await Beans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.index == currentBOTDIndex);
                var newBOTD = await Beans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.index == newBOTDIndex);

                currentBOTD!.IsBOTD = false;
                newBOTD!.IsBOTD = true;
                await SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to set new bean of the day", ex);
            }
        }
    }
}
