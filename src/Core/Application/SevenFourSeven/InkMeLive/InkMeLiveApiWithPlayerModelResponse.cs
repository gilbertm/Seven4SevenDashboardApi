namespace RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;
public class InkMeLiveApiWithPlayerModelResponse
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public PlayersModel? Data { get; set; } = default!;
}