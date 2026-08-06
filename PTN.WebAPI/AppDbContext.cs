using Microsoft.EntityFrameworkCore;
using PTN.WebAPI.Entities;

namespace PTN.WebAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RequestLogEntity> RequestLogs { get; set; }
        public DbSet<ApiSettings> ApiSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EntityConfigurations klasöründeki TÜM konfigürasyon sınıflarını otomatik bulur ve uygular!
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}