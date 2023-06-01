namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;
public class InkMeLiveApiResponse
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}