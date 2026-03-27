using ApplicationCore.Interfaces.Repositories;
using Hangfire;
using HangfireJobs.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        string dnd5eBaseUrl = hostContext.Configuration["ExternalApis:Dnd5e:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: ExternalApis:Dnd5e:BaseUrl");

        // 1) Register DbContext
        _ = services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // 2) Add Hangfire + HangfireServer
        _ = services.AddHangfire(config =>
        {
            // Use SQL Server. ConnectionString must match your DB
            _ = config.UseSqlServerStorage(
                hostContext.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Optionally specify server options:
        _ = services.AddHangfireServer(options =>
        {
            options.ServerName = "DnDSpellsWorker";
            options.Queues = ["default"];
            options.WorkerCount = 10;
        });

        // 3) Register dependencies
        _ = services.AddHttpClient<SpellUpsertJob>(client =>
        {
            client.BaseAddress = new Uri(dnd5eBaseUrl);
        });
        _ = services.AddScoped<ISpellRepository, SpellRepository>();
        _ = services.AddScoped<SpellUpsertJob>();
    });

IHost app = builder.Build();

// 4) Use a scope to configure jobs after the Host is built
using (IServiceScope scope = app.Services.CreateScope())
{
    IHostEnvironment env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

    if (env.IsDevelopment())
    {
        // DEVELOPMENT ENVIRONMENT: run the job once immediately
        // Use IBackgroundJobClient (instead of static BackgroundJob.Enqueue)
        IBackgroundJobClient backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        _ = backgroundJobs.Enqueue<SpellUpsertJob>(job => job.ExecuteAsync(CancellationToken.None));
    }
    else
    {
        // PRODUCTION (or other env): schedule recurring job
        IRecurringJobManager recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<SpellUpsertJob>(
            "spell-upsert-job",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            }
        );
    }
}

// 5) Run the WorkerService
await app.RunAsync();
