namespace Aruba.Application.FunctionalTests;

using Aruba.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public abstract class TestBase
{
    protected readonly ServiceProvider serviceProvider;

    protected TestBase()
    {
        var collection = new Dictionary<string, string?>
        {
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(collection)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        this.serviceProvider = services.BuildServiceProvider();
    }
}
