namespace Aruba.Infrastructure;

using System.Collections.Concurrent;
using System.Reflection;

using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("DefaultConnection");
            });
            
            var dictionary = new ConcurrentDictionary<BookId, BookEntity>();
            services.AddSingleton<IBookRepository>(new BookRepository(dictionary));

            services.AddFusionCache()
                .WithOptions(options => options.CacheKeyPrefix = "aruba:v1:")
                .WithDefaultEntryOptions(options =>
                {
                    options.Duration = TimeSpan.FromMinutes(5);
                    options.IsFailSafeEnabled = true;
                    options.FailSafeMaxDuration = TimeSpan.FromHours(2);
                })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .WithRegisteredDistributedCache()
                .AsHybridCache();

            return services;
        }
    }
}
