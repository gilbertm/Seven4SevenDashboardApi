using RAFFLE.WebApi.Application;
using RAFFLE.WebApi.Host.Configurations;
using RAFFLE.WebApi.Host.Controllers;
using RAFFLE.WebApi.Infrastructure;
using RAFFLE.WebApi.Infrastructure.Common;
using RAFFLE.WebApi.Infrastructure.Logging.Serilog;
using Serilog;
using Serilog.Formatting.Compact;

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