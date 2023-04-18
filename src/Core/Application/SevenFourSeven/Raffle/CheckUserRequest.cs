using System.Numerics;

namespace RAFFLE.WebApi.Application.SevenFourSeven.Raffle;

public class CheckUserRequest
{
    public string AuthCode { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string UserId747 { get; set; } = default!;
    public string UserName747 { get; set; } = default!;
    public string Email { get; set; } = default!;
}
