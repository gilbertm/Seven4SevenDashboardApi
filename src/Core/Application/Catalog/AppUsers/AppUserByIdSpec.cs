namespace RAFFLE.WebApi.Application.Catalog.AppUsers;
public class AppUserByIdSpec : Specification<AppUser, AppUserDto>, ISingleResultSpecification
{
    public AppUserByIdSpec(Guid appUserId) =>
        Query
        .Where(au => au.Id == appUserId);
}