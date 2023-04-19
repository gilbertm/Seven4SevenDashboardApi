using RAFFLE.WebApi.Domain.Common.Events;

namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class UpdateAppUserRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; } = default!;
    public string? HomeAddress { get; set; }
    public string? HomeCity { get; set; }
    public string? HomeRegion { get; set; }
    public string? HomeCountry { get; set; }
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? RaffleUserId { get; set; }
    public string? RaffleUserId747 { get; set; }
    public string? RaffleUsername747 { get; set; }
}

public class UpdateAppUserRequestHandler : IRequestHandler<UpdateAppUserRequest, Guid>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly IStringLocalizer _t;

    public UpdateAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository, IStringLocalizer<UpdateAppUserRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<Guid> Handle(UpdateAppUserRequest request, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _repository.GetByIdAsync(request.Id, cancellationToken);
        _ = appUser ?? throw new NotFoundException(_t["Application user {0} details does not exist. Nothing to update.", request.Id]);

        appUser.Update(applicationUserId: request.ApplicationUserId, homeAddress: request.HomeAddress ?? default, homeCity: request.HomeCity ?? default, homeRegion: request.HomeRegion ?? default, homeCountry: request.HomeCountry ?? default, roleId: request.RoleId ?? default, roleName: request.RoleName ?? default, raffleUserId: request.RaffleUserId ?? default, raffleUserId747: request.RaffleUserId747 ?? default, raffleUsername747: request.RaffleUsername747 ?? default);

        await _repository.UpdateAsync(appUser, cancellationToken);

        // Add Domain Events to be raised after the commit
        appUser.DomainEvents.Add(EntityUpdatedEvent.WithEntity(appUser));

        return appUser.Id;
    }
}