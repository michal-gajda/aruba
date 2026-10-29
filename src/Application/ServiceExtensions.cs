namespace Aruba.Application;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            return services;
        }
    }
}
