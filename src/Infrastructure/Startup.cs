using System.Reflection;
using System.Runtime.CompilerServices;
using RAFFLE.WebApi.Infrastructure.Auth;
using RAFFLE.WebApi.Infrastructure.BackgroundJobs;
using RAFFLE.WebApi.Infrastructure.Caching;
using RAFFLE.WebApi.Infrastructure.Common;
using RAFFLE.WebApi.Infrastructure.Cors;
using RAFFLE.WebApi.Infrastructure.FileStorage;
using RAFFLE.WebApi.Infrastructure.Localization;
using RAFFLE.WebApi.Infrastructure.Mailing;
using RAFFLE.WebApi.Infrastructure.Mapping;
using RAFFLE.WebApi.Infrastructure.Middleware;
using RAFFLE.WebApi.Infrastructure.Multitenancy;
using RAFFLE.WebApi.Infrastructure.Notifications;
using RAFFLE.WebApi.Infrastructure.OpenApi;
using RAFFLE.WebApi.Infrastructure.Persistence;
using RAFFLE.WebApi.Infrastructure.Persistence.Initialization;
using RAFFLE.WebApi.Infrastructure.SecurityHeaders;
using RAFFLE.WebApi.Infrastructure.Validations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;
using RAFFLE.WebApi.Infrastructure.SendGrid;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("Infrastructure.Test")]

namespace RAFFLE.WebApi.Infrastructure;

public static class Startup
{
    private const string RaffleAPIByPassName = "Raffle.ByPass.API";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var applicationAssembly = typeof(RAFFLE.WebApi.Application.Startup).GetTypeInfo().Assembly;

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