using System.Numerics;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.SendgridTwilio;

public class TwilioCodeRequest
{
    public string Recipient { get; set; } = default!;
    public string Code { get; set; } = default!;
}