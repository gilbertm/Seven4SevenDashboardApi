namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;

public class Info747
{
    public string? Username747 { get; set; }
    public string? UserId747 { get; set; }
    public string? UniqueCode { get; set; }
}

public class SocialProfiles
{
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
}

public class RegisterUserRequest
{
    public string AuthCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Surname { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public Info747 Info747 { get; set; } = default!;
    public SocialProfiles SocialProfiles { get; set; } = default!;
    public bool IsAgent { get; set; } = false;

    public bool CanLinkPlayer { get; set; } = default!;
    public bool CanLinkAgent { get; set; } = default!;
    public string SocialCode { get; set; } = default!;

    // this will be used for
    // password authentication on the reward system
    public string OwnUserAuthCode { get; set; } = default!;
}