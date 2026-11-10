namespace Aruba.AppHost;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        _ = builder.AddProject<Projects.Aruba_WebApi>("webapi")
            .WithHttpHealthCheck("/health")
            ;

        var app = builder.Build();

        await app.RunAsync();

        return Environment.ExitCode;
    }
}
