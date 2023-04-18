using RAFFLE.WebApi.Domain.Common.Events;

namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class DeleteAppUserRequest : IRequest<Guid>
{
    public Guid Id = default!;
    public DeleteAppUserRequest(Guid id) => Id = id;
}

public class DeleteAppUserRequestHandler : IRequestHandler<DeleteAppUserRequest, Guid>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly IStringLocalizer _t;

    public DeleteAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository, IStringLocalizer<DeleteAppUserRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<Guid> Handle(DeleteAppUserRequest request, CancellationToken cancellationToken)
    {
        var appUser = await _repository.GetByIdAsync(request.Id, cancellationToken);
        _ = appUser ?? throw new NotFoundException(_t["Application user {0} details does not exist. Nothing to update.", request.Id]);

        // Add Domain Events to be raised after the commit
        appUser.DomainEvents.Add(EntityDeletedEvent.WithEntity(appUser));

        await _repository.DeleteAsync(appUser, cancellationToken);

        return appUser.Id;
    }
}