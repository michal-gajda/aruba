namespace Aruba.WebApi;

using Aruba.Application;
using Aruba.Infrastructure;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWindowsService(options => options.ServiceName = "Aruba");

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();

        return Environment.ExitCode;
    }
}
