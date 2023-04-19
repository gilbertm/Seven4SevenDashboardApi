using RAFFLE.WebApi.Domain.Common.Events;

namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class CreateAppUserRequest : IRequest<Guid>
{
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

public class CreateAppUserRequestHandler : IRequestHandler<CreateAppUserRequest, Guid>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;

    public CreateAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository) => _repository = repository;

    public async Task<Guid> Handle(CreateAppUserRequest request, CancellationToken cancellationToken)
    {
        AppUser? appUser = new AppUser(applicationUserId: request.ApplicationUserId, homeAddress: request.HomeAddress ?? default, homeCity: request.HomeCity ?? default, homeRegion: request.HomeRegion ?? default, homeCountry: request.HomeCountry ?? default, roleId: request.RoleId ?? default, roleName: request.RoleName ?? default, raffleUserId: request.RaffleUserId ?? default, raffleUserId747: request.RaffleUserId747 ?? default, raffleUsername747: request.RaffleUsername747 ?? default);

        await _repository.AddAsync(appUser, cancellationToken);

        // Add Domain Events to be raised after the commit
        appUser.DomainEvents.Add(EntityCreatedEvent.WithEntity(appUser));

        return appUser.Id;
    }
}