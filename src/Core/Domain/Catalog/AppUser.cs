using RAFFLE.WebApi.Domain.Common;

namespace RAFFLE.WebApi.Domain.Catalog;

public class AppUser : AuditableEntity, IAggregateRoot
{
    // this application userid is mapped
    // to the main applicationuser
    public string? ApplicationUserId { get; private set; }
    public string? HomeAddress { get; private set; }
    public string? HomeCity { get; private set; }
    public string? HomeRegion { get; private set; }
    public string? HomeCountry { get; private set; }
    public string? RoleId { get; private set; }
    public string? RoleName { get; private set; }
    public string? RaffleUserId { get; private set; }
    public string? RaffleUserId747 { get; private set; }
    public string? RaffleUsername747 { get; private set; }

    public AppUser(string applicationUserId, string? homeAddress, string? homeCity, string? homeRegion, string? homeCountry, string? roleId, string? roleName, string? raffleUserId, string? raffleUserId747, string? raffleUsername747)
    {
        ApplicationUserId = applicationUserId;
        HomeAddress = homeAddress ?? default!;
        HomeCity = homeCity ?? default!;
        HomeRegion = homeRegion ?? default!;
        HomeCountry = homeCountry ?? default!;

        RoleId = roleId ?? default!;
        RoleName = roleName ?? default!;
        RaffleUserId = raffleUserId ?? default!;
        RaffleUserId747 = raffleUserId747 ?? default!;
        RaffleUsername747 = raffleUsername747 ?? default!;
    }

    public AppUser Update(string applicationUserId, string? homeAddress, string? homeCity, string? homeRegion, string? homeCountry, string? roleId, string? roleName, string? raffleUserId, string? raffleUserId747, string? raffleUsername747)
    {
        if (ApplicationUserId is not null && (ApplicationUserId == applicationUserId) is not true) ApplicationUserId = applicationUserId;
        if (homeAddress is not null && (HomeAddress == homeAddress) is not true) HomeAddress = homeAddress;
        if (homeCity is not null && (HomeCity == homeCity) is not true) HomeCity = homeCity;
        if (homeRegion is not null && (HomeRegion == homeRegion) is not true) HomeRegion = homeRegion;
        if (homeCountry is not null && (HomeCountry == homeCountry) is not true) HomeCountry = homeCountry;

        if (roleId is not null && (RoleId == roleId) is not true) RoleId = roleId;
        if (roleName is not null && (RoleName == roleName) is not true) RoleName = roleName;
        if (raffleUserId is not null && (RaffleUserId == raffleUserId747) is not true) RaffleUserId = raffleUserId747;
        if (raffleUserId747 is not null && (RaffleUserId747 == raffleUserId747) is not true) RaffleUserId747 = raffleUserId747;
        if (raffleUsername747 is not null && (RaffleUsername747 == raffleUsername747) is not true) RaffleUsername747 = raffleUsername747;

        return this;
    }
}
