namespace RAFFLE.WebApi.Application.SevenFourSeven.Raffle;

public class ResetAuthCodeUserToken
{
    public string AuthCode { get; set; } = default!;
    public string Email { get; set; } = default!;
}