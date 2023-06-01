using UNIFIEDDASHBOARD.WebApi.Infrastructure.Multitenancy;

namespace UNIFIEDDASHBOARD.WebApi.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(RAFFLETenantInfo tenant, CancellationToken cancellationToken);
}