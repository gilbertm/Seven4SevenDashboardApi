namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;
public class InkMeLiveTokenResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public string AuthToken { get; set; } = default!;
    public string TokenType { get; set; } = default!;
}