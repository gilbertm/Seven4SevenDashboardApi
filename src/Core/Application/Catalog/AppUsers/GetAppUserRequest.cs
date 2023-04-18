namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class GetAppUserRequest : IRequest<AppUserDto>
{
   public Guid Id { get; set; }

   public GetAppUserRequest(Guid id) => Id = id;
}

public class GetAppUserRequestHandler : IRequestHandler<GetAppUserRequest, AppUserDto>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IStringLocalizer _t;

    public GetAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository, ICurrentUser currentUser, IStringLocalizer<GetAppUserRequestHandler> localizer) =>
        (_repository, _currentUser, _t) = (repository, currentUser, localizer);

    public async Task<AppUserDto> Handle(GetAppUserRequest request, CancellationToken cancellationToken)
    {
        return await _repository.GetBySpecAsync(
           (ISpecification<AppUser, AppUserDto>)new AppUserByIdSpec(request.Id), cancellationToken) ?? throw new NotFoundException(_t["AppUser Id: {0} Not Found.", request.Id]);
    }
}