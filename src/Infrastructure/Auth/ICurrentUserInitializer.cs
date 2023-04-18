using System.Security.Claims;

namespace RAFFLE.WebApi.Infrastructure.Auth;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);

    void SetCurrentUserId(string userId);
}