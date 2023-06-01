using System.Numerics;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.SendgridTwilio;

public class TwilioVerificationRequest
{
    public string Channel { get; set; } = default!; // either "email", "sms"
    public string Recipient { get; set; } = default!;
}