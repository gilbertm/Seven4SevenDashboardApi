namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;

public class PlayerInfo
{
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? Username747 { get; set; }
    public long? UserId747 { get; set; } = default!;
    public string? UniqueCode { get; set; }
}

public class AgentInfo
{
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? Username747 { get; set; }
    public long? UserId747 { get; set; } = default!;
    public string? UniqueCode { get; set; }
}

public class GetUserInfoResponse
{
    public PlayerInfo? PlayerInfo { get; set; }
    public AgentInfo? AgentInfo { get; set; }
    public DateTime? CreatedUtc { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public bool CanLinkPlayer { get; set; } = default!;
    public bool CanLinkAgent { get; set; } = default!;
    public string? SocialCode { get; set; }
    public string? AuthCode { get; set; }
    public int ErorrCode { get; set; } = default!;
    public string Message { get; set; } = default!;
}