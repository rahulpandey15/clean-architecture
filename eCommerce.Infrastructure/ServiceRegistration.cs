using eCommerce.Domain.Contracts;
using eCommerce.Infrastructure.Persistence.Entities;
using eCommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using eCommerce.Infrastructure.Background;

namespace eCommerce.Infrastructure
{
    public static class ServiceRegistration
    {

        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString)
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

            return services;
        }


    }
}
