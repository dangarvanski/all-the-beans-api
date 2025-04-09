using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Context
{
    public class AppDbContext : IAppDbContext
    {
        public DbSet<BeanDbRecord> Beans { get; set; }

        public async Task<BeanDbRecord?> GetRecordByIdAsync(int id)
        {
            // Get Data from DB
            return new BeanDbRecord();
        }
    }
}
