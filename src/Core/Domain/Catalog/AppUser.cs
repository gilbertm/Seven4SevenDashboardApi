using UNIFIEDDASHBOARD.WebApi.Domain.Common;

namespace UNIFIEDDASHBOARD.WebApi.Domain.Catalog;

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
    public string? RaffleUserId { get; set; }
    public string? RaffleUserId747 { get; set; }
    public string? RaffleUsername747 { get; set; }
    public bool IsAgent { get; set; }
    public string? UniqueCode { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public bool CanLinkAgent { get; set; }
    public bool CanLinkPlayer { get; set; }
    public string SocialCode { get; set; }

    public AppUser(string applicationUserId, string homeAddress = default!, string homeCity = default!, string homeRegion = default!, string homeCountry = default!, string roleId = default!, string roleName = default!, string raffleUserId = default!, string raffleUserId747 = default!, string raffleUsername747 = default!, bool isAgent = false, string uniqueCode = default!, string facebookUrl = default!, string instagramUrl = default!, string twitterUrl = default!, bool canLinkPlayer = default!, bool canLinkAgent = default!, string socialCode = default!)
    {
        ApplicationUserId = applicationUserId;
        HomeAddress = homeAddress;
        HomeCity = homeCity;
        HomeRegion = homeRegion;
        HomeCountry = homeCountry;

        RoleId = roleId;
        RoleName = roleName;
        RaffleUserId = raffleUserId;
        RaffleUserId747 = raffleUserId747;
        RaffleUsername747 = raffleUsername747;
        IsAgent = isAgent;
        UniqueCode = uniqueCode;
        FacebookUrl = facebookUrl;
        TwitterUrl = twitterUrl;
        InstagramUrl = instagramUrl;

        CanLinkPlayer = canLinkPlayer;
        CanLinkAgent = canLinkAgent;
        SocialCode = socialCode;
    }

    public AppUser Update(string applicationUserId = default!, string homeAddress = default!, string homeCity = default!, string homeRegion = default!, string homeCountry = default!, string roleId = default!, string roleName = default!, string raffleUserId = default!, string raffleUserId747 = default!, string raffleUsername747 = default!, bool isAgent = false, string uniqueCode = default!, string facebookUrl = default!, string instagramUrl = default!, string twitterUrl = default!, bool canLinkPlayer = default!, bool canLinkAgent = default!, string socialCode = default!)
    {
        if (applicationUserId is not null && (ApplicationUserId == applicationUserId) is not true) ApplicationUserId = applicationUserId;
        if (homeAddress is not null && (HomeAddress == homeAddress) is not true) HomeAddress = homeAddress;
        if (homeCity is not null && (HomeCity == homeCity) is not true) HomeCity = homeCity;
        if (homeRegion is not null && (HomeRegion == homeRegion) is not true) HomeRegion = homeRegion;
        if (homeCountry is not null && (HomeCountry == homeCountry) is not true) HomeCountry = homeCountry;

        if (roleId is not null && (RoleId == roleId) is not true) RoleId = roleId;
        if (roleName is not null && (RoleName == roleName) is not true) RoleName = roleName;
        if (raffleUserId is not null && (RaffleUserId == raffleUserId747) is not true) RaffleUserId = raffleUserId747;
        if (raffleUserId747 is not null && (RaffleUserId747 == raffleUserId747) is not true) RaffleUserId747 = raffleUserId747;
        if (raffleUsername747 is not null && (RaffleUsername747 == raffleUsername747) is not true) RaffleUsername747 = raffleUsername747;

        if (isAgent is not false && (IsAgent == isAgent) is not true) IsAgent = isAgent;
        if (uniqueCode is not null && (UniqueCode == uniqueCode) is not true) UniqueCode = uniqueCode;

        if (facebookUrl is not null && (FacebookUrl == facebookUrl) is not true) FacebookUrl = facebookUrl;
        if (instagramUrl is not null && (InstagramUrl == instagramUrl) is not true) InstagramUrl = instagramUrl;
        if (twitterUrl is not null && (TwitterUrl == twitterUrl) is not true) TwitterUrl = twitterUrl;

        if (canLinkPlayer is not false && (CanLinkPlayer == canLinkPlayer) is not true) CanLinkPlayer = canLinkPlayer;
        if (canLinkAgent is not false && (CanLinkAgent == canLinkAgent) is not true) CanLinkAgent = canLinkAgent;
        if (socialCode is not null && (SocialCode == socialCode) is not true) SocialCode = socialCode;

        return this;
    }
}
