namespace Aruba.Infrastructure;

using System.Collections.Concurrent;
using System.Reflection;

using Aruba.Domain.Entities;
using Aruba.Domain.Interfaces;
using Aruba.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            var dictionary = new ConcurrentDictionary<BookId, BookEntity>();
            services.AddSingleton<IBookRepository>(new BookRepository(dictionary));

            return services;
        }
    }
}
