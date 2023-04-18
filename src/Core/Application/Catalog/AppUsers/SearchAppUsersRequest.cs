namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class SearchAppUsersRequest : PaginationFilter, IRequest<PaginationResponse<AppUserDto>>
{
    public string? ApplicationUserId { get; set; }
}

public class SearchAppUsersRequestHandler : IRequestHandler<SearchAppUsersRequest, PaginationResponse<AppUserDto>>
{
    private readonly IReadRepository<AppUser> _repository;

    public SearchAppUsersRequestHandler(IReadRepository<AppUser> repository) => _repository = repository;

    public async Task<PaginationResponse<AppUserDto>> Handle(SearchAppUsersRequest request, CancellationToken cancellationToken)
    {
        var spec = new AppUserByApplicationUserIdCollectionSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
    }
}