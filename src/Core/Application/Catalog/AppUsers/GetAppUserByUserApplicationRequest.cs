namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class GetAppUserByUserApplicationRequest : IRequest<AppUserDto>
{
   public string ApplicationUserId { get; set; }

   public GetAppUserByUserApplicationRequest(string applicationUserId) => ApplicationUserId = applicationUserId;
}

public class GetAppUserByUserApplicationRequestHandler : IRequestHandler<GetAppUserByUserApplicationRequest, AppUserDto>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IStringLocalizer _t;

    public GetAppUserByUserApplicationRequestHandler(IRepositoryWithEvents<AppUser> repository, ICurrentUser currentUser, IStringLocalizer<GetAppUserByUserApplicationRequestHandler> localizer) =>
        (_repository, _currentUser, _t) = (repository, currentUser, localizer);

    public async Task<AppUserDto> Handle(GetAppUserByUserApplicationRequest request, CancellationToken cancellationToken)
    {
        var appUserDto = await _repository.GetBySpecAsync(
           (ISpecification<AppUser, AppUserDto>)new AppUserByApplicationUserIdSpec(request.ApplicationUserId), cancellationToken);

        if (appUserDto != default)
            return appUserDto;

        throw new NotFoundException(_t["AppUser Application User Id: {0} Not Found.", request.ApplicationUserId]);
    }
}