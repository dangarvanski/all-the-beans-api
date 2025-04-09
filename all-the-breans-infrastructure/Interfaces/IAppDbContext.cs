using all_the_breans_sharedKernal.Entities;
using Microsoft.EntityFrameworkCore;

namespace all_the_breans_infrastructure.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<BeanDbRecord> Beans { get; set; }
        Task<BeanDbRecord?> GetRecordByIdAsync(int id);
    }
}