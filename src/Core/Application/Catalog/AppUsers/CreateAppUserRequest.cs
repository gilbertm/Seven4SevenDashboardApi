using RAFFLE.WebApi.Domain.Common.Events;

namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class CreateAppUserRequest : IRequest<Guid>
{
    public string ApplicationUserId { get; set; } = default!;
    public string? HomeAddress { get; set; }
    public string? HomeCity { get; set; }
    public string? HomeRegion { get; set; }
    public string? HomeCountry { get; set; }
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
    public bool? IsVerified { get; set; }
    public VerificationStatus AddressStatus { get; set; } = VerificationStatus.Initial;
    public VerificationStatus DocumentsStatus { get; set; } = VerificationStatus.Initial;
    public VerificationStatus RolePackageStatus { get; set; } = VerificationStatus.Initial;    
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

public class CreateAppUserRequestHandler : IRequestHandler<CreateAppUserRequest, Guid>
{
    private readonly IRepositoryWithEvents<AppUser> _repository;

    public CreateAppUserRequestHandler(IRepositoryWithEvents<AppUser> repository) => _repository = repository;

    public async Task<Guid> Handle(CreateAppUserRequest request, CancellationToken cancellationToken)
    {
        AppUser? appUser = new AppUser(applicationUserId: request.ApplicationUserId, homeAddress: request.HomeAddress ?? default, homeCity: request.HomeCity ?? default, homeRegion: request.HomeRegion ?? default, homeCountry: request.HomeCountry ?? default, longitude: request.Longitude ?? default, latitude: request.Latitude ?? default, isVerified: request.IsVerified ?? false, addressStatus: request.AddressStatus,  roleId: request.RoleId, roleName: request.RoleName, firstName: request.FirstName, lastName: request.LastName, email: request.Email, phoneNumber: request.PhoneNumber, imageUrl: request.ImageUrl);

        await _repository.AddAsync(appUser, cancellationToken);

        // Add Domain Events to be raised after the commit
        appUser.DomainEvents.Add(EntityCreatedEvent.WithEntity(appUser));

        return appUser.Id;
    }
}