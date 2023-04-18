using System.Numerics;

namespace RAFFLE.WebApi.Application.SevenFourSeven.Raffle;

public class PlayerInfo
{
    public string? TwitterUrl { get; set; } = default!;
    public string? FacebookUrl { get; set; } = default!;
    public string? InstagramUrl { get; set; } = default!;
    public string? Username747 { get; set; } = default!;
    public long UserId747 { get; set; } = default!;
    public string? UniqueCod { get; set; } = default!;
}

public class AgentInfo
{
    public string? TwitterUrl { get; set; } = default!;
    public string? FacebookUrl { get; set; } = default!;
    public string? InstagramUrl { get; set; } = default!;
    public string? Username747 { get; set; } = default!;
    public long UserId747 { get; set; } = default!;
    public string? UniqueCod { get; set; } = default!;
}

public class GetUserInfoResponse
{
    public PlayerInfo? PlayerInfo { get; set; } = default!;
    public AgentInfo? AgentInfo { get; set; } = default!;
    public DateTime? CreatedUtc { get; set; } = default!;
    public string? Phone { get; set; } = default!;
    public string? Name { get; set; } = default!;
    public string? Surname { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public bool CanLinkPlayer { get; set; } = default!;
    public bool CanLinkAgent { get; set; } = default!;
    public string? SocialCode { get; set; } = default!;
    public string? AuthCode { get; set; } = default!;
    public int ErorrCode { get; set; } = default!;
    public string Message { get; set; } = default!;
}