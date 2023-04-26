using System.Numerics;

namespace RAFFLE.WebApi.Application.SevenFourSeven.SendgridTwilio;

public class TwilioVerificationRequest
{
    public string Channel { get; set; } = default!; // either "email", "sms"
    public string Recipient { get; set; } = default!;
}