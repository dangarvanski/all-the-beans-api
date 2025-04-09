using Microsoft.EntityFrameworkCore;

namespace CreateSQLiteDatabase
{
    public class AppDbContext : DbContext
    {
        public DbSet<BeanDbRecord> Beans { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=beans.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BeanDbRecord>()
                .HasKey(b => b._id);
        }
    }
}
