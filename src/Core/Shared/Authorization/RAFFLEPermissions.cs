using System.Collections.ObjectModel;

namespace RAFFLE.WebApi.Shared.Authorization;

public static class RAFFLEAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
}

public static class RAFFLEResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);

    public const string AppUsers = nameof(AppUsers);
}

public static class RAFFLEPermissions
{
    private static readonly RAFFLEPermission[] _all = new RAFFLEPermission[]
    {
        new("View Dashboard", RAFFLEAction.View, RAFFLEResource.Dashboard, IsBasic: true),
        new("View Hangfire", RAFFLEAction.View, RAFFLEResource.Hangfire),
        new("View Users", RAFFLEAction.View, RAFFLEResource.Users, IsBasic: true),
        new("Search Users", RAFFLEAction.Search, RAFFLEResource.Users, IsBasic: true),
        new("Create Users", RAFFLEAction.Create, RAFFLEResource.Users),
        new("Update Users", RAFFLEAction.Update, RAFFLEResource.Users),
        new("Delete Users", RAFFLEAction.Delete, RAFFLEResource.Users),
        new("Export Users", RAFFLEAction.Export, RAFFLEResource.Users),
        new("View UserRoles", RAFFLEAction.View, RAFFLEResource.UserRoles, IsBasic: true),
        new("Update UserRoles", RAFFLEAction.Update, RAFFLEResource.UserRoles, IsBasic: true),
        new("View Roles", RAFFLEAction.View, RAFFLEResource.Roles, IsBasic: true),
        new("Create Roles", RAFFLEAction.Create, RAFFLEResource.Roles),
        new("Update Roles", RAFFLEAction.Update, RAFFLEResource.Roles),
        new("Delete Roles", RAFFLEAction.Delete, RAFFLEResource.Roles),
        new("View RoleClaims", RAFFLEAction.View, RAFFLEResource.RoleClaims),
        new("Update RoleClaims", RAFFLEAction.Update, RAFFLEResource.RoleClaims),
        new("View Tenants", RAFFLEAction.View, RAFFLEResource.Tenants, IsRoot: true),
        new("Create Tenants", RAFFLEAction.Create, RAFFLEResource.Tenants, IsRoot: true),
        new("Update Tenants", RAFFLEAction.Update, RAFFLEResource.Tenants, IsRoot: true),

        new("View AppUsers", RAFFLEAction.View, RAFFLEResource.AppUsers, IsBasic: true),
        new("Search AppUsers", RAFFLEAction.Search, RAFFLEResource.AppUsers),
        new("Create AppUsers", RAFFLEAction.Create, RAFFLEResource.AppUsers, IsBasic: true),
        new("Update AppUsers", RAFFLEAction.Update, RAFFLEResource.AppUsers, IsBasic: true),
        new("Delete AppUsers", RAFFLEAction.Delete, RAFFLEResource.AppUsers),
        new("Export AppUsers", RAFFLEAction.Export, RAFFLEResource.AppUsers),
    };

    public static IReadOnlyList<RAFFLEPermission> All { get; } = new ReadOnlyCollection<RAFFLEPermission>(_all);
    public static IReadOnlyList<RAFFLEPermission> Root { get; } = new ReadOnlyCollection<RAFFLEPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<RAFFLEPermission> Admin { get; } = new ReadOnlyCollection<RAFFLEPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<RAFFLEPermission> Basic { get; } = new ReadOnlyCollection<RAFFLEPermission>(_all.Where(p => p.IsBasic).ToArray());

}

public record RAFFLEPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
