using System.Numerics;

namespace RAFFLE.WebApi.Application.SevenFourSeven.SendgridTwilio;

public class TwilioCodeRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}