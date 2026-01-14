using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using EnterpriseCore.Infrastructure.Caching;
using EnterpriseCore.Infrastructure.Data;
using EnterpriseCore.Infrastructure.Repositories;
using EnterpriseCore.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register DbContext as base type for services that need it
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Redis (optional - gracefully handle connection failures)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var redisConfigured = false;

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 5000; // 5 second timeout

                var redis = ConnectionMultiplexer.Connect(redisOptions);

                // Check if actually connected
                if (redis.IsConnected)
                {
                    services.AddSingleton<IConnectionMultiplexer>(redis);

                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnectionString;
                        options.InstanceName = "EnterpriseCore:";
                    });

                    services.AddScoped<ICacheService, RedisCacheService>();
                    redisConfigured = true;
                }
            }
            catch
            {
                // Redis connection failed - will use NullCacheService
            }
        }

        // Fallback to NullCacheService when Redis is not available
        if (!redisConfigured)
        {
            services.AddSingleton<ICacheService, NullCacheService>();
        }

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // File Storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
