using System.Reflection;
using System.Runtime.CompilerServices;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Auth;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.BackgroundJobs;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Caching;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Common;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Cors;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.FileStorage;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Localization;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Mailing;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Mapping;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Middleware;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Multitenancy;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Notifications;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.OpenApi;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Persistence;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Persistence.Initialization;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.SecurityHeaders;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Validations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.SendGrid;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("Infrastructure.Test")]

namespace UNIFIEDDASHBOARD.WebApi.Infrastructure;

public static class Startup
{
    private const string RaffleAPIByPassName = "Raffle.ByPass.API";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var applicationAssembly = typeof(UNIFIEDDASHBOARD.WebApi.Application.Startup).GetTypeInfo().Assembly;

        MapsterSettings.Configure();
        return services
            .AddHttpClient(RaffleAPIByPassName)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                handler.ServerCertificateCustomValidationCallback = ValidateServerCertification;

                return handler;
            })
            .Services
            .AddApiVersioning()
            .AddAuth(config)
            .AddBackgroundJobs(config)
            .AddCaching(config)
            .AddCorsPolicy(config)
            .AddExceptionMiddleware()
            .AddBehaviours(applicationAssembly)
            .AddHealthCheck()
            .AddPOLocalization(config)
            .AddMailing(config)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddMultitenancy()
            .AddNotifications(config)
            .AddOpenApiDocumentation(config)
            .AddPersistence()
            .AddRequestLogging(config)
            .AddMailDeliveryServices(config)
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices();
    }

    private static bool ValidateServerCertification(HttpRequestMessage message, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        Console.WriteLine($"Requested URI: {message.RequestUri}");
        Console.WriteLine($"Effective date: {certificate?.GetEffectiveDateString()}");
        Console.WriteLine($"Expiry date: {certificate?.GetExpirationDateString()}");
        Console.WriteLine($"Issuer: {certificate?.Issuer}");
        Console.WriteLine($"Subject: {certificate?.Subject}");
        return true;
    }

    private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

    private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
        services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
        builder
            .UseRequestLocalization()
            .UseStaticFiles()
            .UseSecurityHeaders(config)
            .UseFileStorage()
            .UseExceptionMiddleware()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseCurrentUser()
            .UseMultiTenancy()
            .UseAuthorization()
            .UseRequestLogging(config)
            .UseHangfireDashboard(config)
            .UseOpenApiDocumentation(config);

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        builder.MapNotifications();
        return builder;
    }

    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/api/health");
}