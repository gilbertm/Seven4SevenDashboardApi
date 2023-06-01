namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Bridge;

public class User
{
    public long ClientId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public long? ParentClientId { get; set; } = default!;
}

public class BridgeHierarchyResponse
{
    public int Status { get; set; } = default!;
    public string Message { get; set; } = default!;
    public User? User { get; set; } = default!;
}