namespace UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;

public class GetAppUserByUserApplicationRequest : IRequest<AppUser>
{
   public string ApplicationUserId { get; set; }

   public GetAppUserByUserApplicationRequest(string applicationUserId) => ApplicationUserId = applicationUserId;
}

public class GetAppUserByUserApplicationRequestHandler : IRequestHandler<GetAppUserByUserApplicationRequest, AppUser>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly IStringLocalizer _t;

    public GetAppUserByUserApplicationRequestHandler(IRepositoryWithEvents<AppUser> repository, IStringLocalizer<GetAppUserByUserApplicationRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<AppUser> Handle(GetAppUserByUserApplicationRequest request, CancellationToken cancellationToken)
    {
        return await _repository.FirstOrDefaultAsync(new AppUserByApplicationUserIdSpec(request.ApplicationUserId), cancellationToken) ?? default!;
    }
}