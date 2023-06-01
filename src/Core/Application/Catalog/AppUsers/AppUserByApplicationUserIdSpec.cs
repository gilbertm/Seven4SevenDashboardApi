namespace UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;
public class AppUserByApplicationUserIdSpec : Specification<AppUser>
{
    public AppUserByApplicationUserIdSpec(string applicationUserId) =>
        Query
        .Where(au => applicationUserId.Equals(au.ApplicationUserId), applicationUserId != default!);
}