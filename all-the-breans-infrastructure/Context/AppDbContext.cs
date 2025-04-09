using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Context
{
    public class AppDbContext : DbContext , IAppDbContext
    {
        public DbSet<BeanDbRecord> Beans { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public async Task<BeanDbRecord?> GetRecordByIndexAsync(int index)
        {
            var thing = Beans.FirstOrDefault();
            return await Beans.FindAsync(index);
        }
    }
}
