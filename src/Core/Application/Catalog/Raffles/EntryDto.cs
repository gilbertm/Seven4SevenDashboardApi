namespace RAFFLE.WebApi.Application.Catalog.Raffles;

public class EntryDto : IDto
{
    public string EntryCode { get; set; } = default!;
    public string CreatedUtc { get; set; } = default!;
    public string Class { get; set; } = default!;
    public int Serial { get; set; } = default!;

}