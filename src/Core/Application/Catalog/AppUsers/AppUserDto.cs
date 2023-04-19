namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class AppUserDto : IDto
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