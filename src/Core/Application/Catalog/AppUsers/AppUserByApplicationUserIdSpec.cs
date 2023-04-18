namespace RAFFLE.WebApi.Application.Catalog.AppUsers;
public class AppUserByApplicationUserIdSpec : Specification<AppUser, AppUserDto>
{
    public AppUserByApplicationUserIdSpec(string applicationUserId) =>
        Query
        .Where(au => applicationUserId.Equals(au.ApplicationUserId), applicationUserId != default!);
}