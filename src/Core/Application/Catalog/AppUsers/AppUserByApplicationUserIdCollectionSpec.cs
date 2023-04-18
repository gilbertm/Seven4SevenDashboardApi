namespace RAFFLE.WebApi.Application.Catalog.AppUsers;
public class AppUserByApplicationUserIdCollectionSpec : EntitiesByPaginationFilterSpec<AppUser, AppUserDto>
{
    public AppUserByApplicationUserIdCollectionSpec(SearchAppUsersRequest request)
        : base(request) =>
        Query
            .OrderBy(a => a.ApplicationUserId, !request.HasOrderBy())
            .Where(a => request.ApplicationUserId == a.ApplicationUserId, request.ApplicationUserId is not null);
}