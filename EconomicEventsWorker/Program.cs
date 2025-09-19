using DotNetEnv;
using EconomicEventsWorker;
using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using EconomicEventsWorker.Notifiers;
using EconomicEventsWorker.Services;
using Microsoft.EntityFrameworkCore;

Env.Load();

var host = Host.CreateDefaultBuilder(args);

Env.Load();

host.ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Default");
        services.AddDbContext<EconomicContext>(options =>
            options.UseSqlite(connectionString));

        services.AddLogging();
        services.Configure<AppSettings>(context.Configuration);
        services.AddTransient<CalendarEventsScraper>();
        services.AddTransient<FREDEventsScraper>();
        services.AddTransient<InvestingComScraper>();
        services.AddTransient<WeeklyNotifier>();
        services.AddTransient<DiscordNotifier>();
        services.AddHostedService<Worker>();
        services.AddHttpClient();
    });

await host.Build().RunAsync();