using System.Collections.ObjectModel;

namespace RAFFLE.WebApi.Shared.Authorization;

public static class RAFFLERoles
{
    // As long as it is not root level
    public const string Admin = nameof(Admin);

    // Basic is the least role that provides a way to see what's the system can offer
    // View and search Loands, Products and Brands
    public const string Basic = nameof(Basic);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Basic
    });

    public static bool IsDefault(string roleName) => DefaultRoles.Any(r => r == roleName);
}