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
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
    public bool? IsVerified { get; set; }
    public VerificationStatus? AddressStatus { get; set; }
    public VerificationStatus? DocumentsStatus { get; set; }
    public VerificationStatus? RolePackageStatus { get; set; }

    public string? RoleId { get; set; }
    public string? RoleName { get; set; }

    // manual propagation
    // this is from the application user
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateAppUserRequestHandler : IRequestHandler<UpdateAppUserRequest, Guid>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;
    private readonly IStringLocalizer _t;

    public UpdateAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository, IStringLocalizer<UpdateAppUserRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<Guid> Handle(UpdateAppUserRequest request, CancellationToken cancellationToken)
    {
        var appUser = await _repository.GetByIdAsync(request.Id, cancellationToken);
        _ = appUser ?? throw new NotFoundException(_t["Application user {0} details does not exist. Nothing to update.", request.Id]);

        appUser.Update(applicationUserId: request.ApplicationUserId, homeAddress: request.HomeAddress ?? default, homeCity: request.HomeCity ?? default, homeRegion: request.HomeRegion ?? default, homeCountry: request.HomeCountry ?? default, longitude: request.Longitude ?? default, latitude: request.Latitude ?? default, isVerified: request.IsVerified, addressStatus: request.AddressStatus, roleId: request.RoleId, roleName: request.RoleName, firstName: request.FirstName, lastName: request.LastName, email: request.Email, phoneNumber: request.PhoneNumber, imageUrl: request.ImageUrl);

        await _repository.UpdateAsync(appUser, cancellationToken);

        // Add Domain Events to be raised after the commit
        appUser.DomainEvents.Add(EntityUpdatedEvent.WithEntity(appUser));

        return appUser.Id;
    }
}