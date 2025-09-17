using EconomicEventsWorker;
using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using EconomicEventsWorker.Notifiers;
using EconomicEventsWorker.Services;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Default");
        services.AddDbContext<EconomicContext>(options =>
            options.UseSqlite(connectionString));

        services.Configure<AppSettings>(context.Configuration);
        services.AddTransient<CalendarEventsScraper>();
        services.AddTransient<WeeklyNotifier>();
        services.AddTransient<DiscordNotifier>();
        services.AddHostedService<Worker>();
        services.AddHttpClient();
    })
    .Build();

await host.RunAsync();