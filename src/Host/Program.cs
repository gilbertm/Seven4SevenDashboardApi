using Microsoft.Extensions.Options;
using UNIFIEDDASHBOARD.WebApi.Application;
using UNIFIEDDASHBOARD.WebApi.Host.Configurations;
using UNIFIEDDASHBOARD.WebApi.Host.Controllers;
using UNIFIEDDASHBOARD.WebApi.Infrastructure;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Common;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Logging.Serilog;
using SendGrid.Extensions.DependencyInjection;
using Serilog;

[assembly: ApiConventionType(typeof(RAFFLEApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations().RegisterSerilog();
    builder.Services.AddControllers();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    var app = builder.Build();

    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}