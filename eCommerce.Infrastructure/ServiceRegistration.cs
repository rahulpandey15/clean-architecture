using eCommerce.Application.Contracts;
using eCommerce.Application.Jobs;
using eCommerce.Domain.Contracts;
using eCommerce.Infrastructure.Persistence.Entities;
using eCommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using eCommerce.Infrastructure.Background;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;

namespace eCommerce.Infrastructure
{
    public static class ServiceRegistration
    {

        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString,
            IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Background job queue for processing orders
            services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();


            services.AddHangfireInfrastructure(config);

            return services;
        }
        
        public static IServiceCollection AddHangfireInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("HangfireConnection")
                                   ?? config.GetConnectionString("DatabaseConnection");

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                    PrepareSchemaIfNecessary = true   // creates Hangfire tables automatically
                }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Math.Min(Environment.ProcessorCount * 2, 10);
                options.Queues = new[] { "default" };
            });

            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
           services.AddScoped<OrderJobs>(); // job classes are resolved from DI

            return services;
        }


    }
}
