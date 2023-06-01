using UNIFIEDDASHBOARD.WebApi.Application.Catalog.Raffles;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;
public class GetEntriesResponse
{
    public int ErorrCode { get; set; }
    public string Message { get; set; } = default!;
    public int CurrentPage { get; set; }
    public List<EntryDto>? AgentEntries { get; set; }
    public List<EntryDto>? PlayerEntries { get; set; }
    public int TotalPagesWithAgentEntries { get; set; }
    public int TotalPagesWithPlayerEntries { get; set; }
}