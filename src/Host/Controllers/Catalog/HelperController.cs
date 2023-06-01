using UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;

namespace UNIFIEDDASHBOARD.WebApi.Host.Controllers.Catalog;

public class HelperController : VersionedApiController
{
    [HttpGet("getip")]
    [AllowAnonymous]
    public string GetIPAddress()
    {
        var remoteIpAddress = this.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress != null)
            return remoteIpAddress.ToString();

        return string.Empty;

    }
}