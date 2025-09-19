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

            // Seed data
            //modelBuilder.Entity<Indicator>().HasData(
            //    new Indicator { Id = Guid.NewGuid(), Name = "CPI (All Urban Consumers)", SeriesId = "CPIAUCSL", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Real GDP", SeriesId = "GDPC1", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Unemployment Rate", SeriesId = "UNRATE", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Federal Funds Rate", SeriesId = "FEDFUNDS", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Nonfarm Payrolls", SeriesId = "PAYEMS", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Industrial Production", SeriesId = "INDPRO", LastDate = null, LastValue = string.Empty },
            //    new Indicator { Id = Guid.NewGuid(), Name = "Retail Sales", SeriesId = "RSXFS", LastDate = null, LastValue = string.Empty }
            //);

            // Seed data
            modelBuilder.Entity<Indicator>().HasData(
                new Indicator { Id = Guid.NewGuid(), Name = "CPI (All Urban Consumers)", SeriesId = "CPIAUCSL", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Core CPI (Ex Food & Energy)", SeriesId = "CPILFESL", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "PCE Price Index", SeriesId = "PCEPI", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Core PCE Price Index", SeriesId = "PCEPILFE", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Producer Price Index (PPI)", SeriesId = "PPIACO", LastDate = null, LastValue = string.Empty },

                new Indicator { Id = Guid.NewGuid(), Name = "Real GDP", SeriesId = "GDPC1", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Real GDP per Capita", SeriesId = "A939RX0Q048SBEA", LastDate = null, LastValue = string.Empty },

                new Indicator { Id = Guid.NewGuid(), Name = "Unemployment Rate", SeriesId = "UNRATE", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Nonfarm Payrolls", SeriesId = "PAYEMS", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Labor Force Participation Rate", SeriesId = "CIVPART", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Initial Jobless Claims", SeriesId = "ICSA", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "JOLTS Job Openings", SeriesId = "JTSJOL", LastDate = null, LastValue = string.Empty },

                new Indicator { Id = Guid.NewGuid(), Name = "Federal Funds Rate", SeriesId = "FEDFUNDS", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Federal Funds Target Range Upper Limit", SeriesId = "DFEDTARU", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Federal Funds Target Range Lower Limit", SeriesId = "DFEDTARL", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Federal Funds Target Rate, midpoint", SeriesId = "DFEDTAR", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Fed Balance Sheet (Total Assets)", SeriesId = "WALCL", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "10-Year Treasury Yield", SeriesId = "DGS10", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "2-Year Treasury Yield", SeriesId = "DGS2", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Yield Curve (10Y - 2Y)", SeriesId = "T10Y2Y", LastDate = null, LastValue = string.Empty },

                new Indicator { Id = Guid.NewGuid(), Name = "Industrial Production", SeriesId = "INDPRO", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Capacity Utilization", SeriesId = "TCU", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "Retail Sales", SeriesId = "RSXFS", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Durable Goods Orders", SeriesId = "DGORDER", LastDate = null, LastValue = string.Empty },

                //new Indicator { Id = Guid.NewGuid(), Name = "Housing Starts", SeriesId = "HOUST", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Building Permits", SeriesId = "PERMIT", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Case-Shiller Home Price Index (National)", SeriesId = "CSUSHPINSA", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Existing Home Sales", SeriesId = "EXHOSLUSM495S", LastDate = null, LastValue = string.Empty },

                //new Indicator { Id = Guid.NewGuid(), Name = "Trade Balance", SeriesId = "NETEXP", LastDate = null, LastValue = string.Empty },
                //new Indicator { Id = Guid.NewGuid(), Name = "Current Account Balance", SeriesId = "NETFI", LastDate = null, LastValue = string.Empty },
                new Indicator { Id = Guid.NewGuid(), Name = "US Dollar Index (Broad)", SeriesId = "DTWEXBGS", LastDate = null, LastValue = string.Empty }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
