namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class AppUserDto : IDto
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; } = default!;
    public string? HomeAddress { get; set; }
    public string? HomeCity { get; set; }
    public string? HomeRegion { get; set; }
    public string? HomeCountry { get; set; }
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
    public bool IsVerified { get; set; } = false;
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