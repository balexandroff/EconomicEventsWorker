using EconomicEventsWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace EconomicEventsWorker.Database
{
    public class EconomicContext : DbContext
    {
        public EconomicContext(DbContextOptions<EconomicContext> options) : base(options) { }

        public DbSet<EconomicEvent> EconomicEvents { get; set; }
        public DbSet<Indicator> Indicators { get; set; }
        public DbSet<Observation> Observations { get; set; }
        public DbSet<WeeklyEvent> WeeklyEvents { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=economic.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EconomicEvent>().HasKey(i => i.Id);
            modelBuilder.Entity<Indicator>().HasKey(i => i.Id);
            modelBuilder.Entity<Observation>().HasKey(o => o.Id);
            modelBuilder.Entity<WeeklyEvent>().HasKey(w => w.Id);

            modelBuilder.Entity<Observation>()
                .HasOne(o => o.Indicator)
                .WithMany()
                .HasForeignKey(o => o.IndicatorId);
        }
    }
}
