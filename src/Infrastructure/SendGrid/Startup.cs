using UNIFIEDDASHBOARD.WebApi.Infrastructure.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

namespace UNIFIEDDASHBOARD.WebApi.Infrastructure.SendGrid;

internal static class Startup
{
    internal static IServiceCollection AddMailDeliveryServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSendGrid(options =>
        {
            options.ApiKey = config.GetSection("SevenFourSevenAPIs:Sendgrid:ApiKey").Value!;
        });

        return services;
    }
}