namespace RAFFLE.WebApi.Application.SevenFourSeven.Raffle;

public class GetEntriesRequest
{
    public string AuthCode { get; set; } = default!;
    public string UserId747 { get; set; } = default!;
    public string UserName747 { get; set; } = default!;
    public string RaffleId { get; set; } = default!;
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 0;
}
