using ApplicationCore.Interfaces.Repositories;
using Hangfire;
using HangfireJobs.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // 1) Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // 2) Add Hangfire + HangfireServer
        services.AddHangfire(config =>
        {
            // Use SQL Server. ConnectionString must match your DB
            config.UseSqlServerStorage(
                hostContext.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Optionally specify server options:
        services.AddHangfireServer(options =>
        {
            options.ServerName = "DnDSpellsWorker";
            options.Queues = ["default"];
            options.WorkerCount = 10;
        });

        // 3) Register dependencies
        services.AddHttpClient<SpellUpsertJob>();
        services.AddScoped<ISpellRepository, SpellRepository>();
        services.AddScoped<SpellUpsertJob>();
    });

var app = builder.Build();

// 4) Use a scope to configure jobs after the Host is built
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

    if (env.IsDevelopment())
    {
        // DEVELOPMENT ENVIRONMENT: run the job once immediately
        // Use IBackgroundJobClient (instead of static BackgroundJob.Enqueue)
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobs.Enqueue<SpellUpsertJob>(job => job.ExecuteAsync(CancellationToken.None));
    }
    else
    {
        // PRODUCTION (or other env): schedule recurring job
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
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