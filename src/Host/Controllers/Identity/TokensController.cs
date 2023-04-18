using RAFFLE.WebApi.Application.Identity.Tokens;

namespace RAFFLE.WebApi.Host.Controllers.Identity;

public sealed class TokensController : VersionNeutralApiController
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public TokensController(IConfiguration config, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _config = config;

    }

    [HttpPost]
    [AllowAnonymous]
    [TenantIdHeader]
    [OpenApiOperation("Request an access token using credentials.", "")]
    public Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
    {
        return _tokenService.GetTokenAsync(request, GetIpAddress(), cancellationToken);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [TenantIdHeader]
    [OpenApiOperation("Request an access token using a refresh token.", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Search))]
    public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
    {
        return _tokenService.RefreshTokenAsync(request, GetIpAddress());
    }

    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
}