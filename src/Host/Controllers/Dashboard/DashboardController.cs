using UNIFIEDDASHBOARD.WebApi.Application.Dashboard;

namespace UNIFIEDDASHBOARD.WebApi.Host.Controllers.Dashboard;

public class DashboardController : VersionedApiController
{
    [HttpGet]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Dashboard)]
    [OpenApiOperation("Get statistics for the dashboard.", "")]
    public Task<StatsDto> GetAsync()
    {
        return Mediator.Send(new GetStatsRequest());
    }
}