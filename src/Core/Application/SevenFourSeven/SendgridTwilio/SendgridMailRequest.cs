using System.Numerics;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.SendgridTwilio;

public class SendgridMailRequest
{
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
}