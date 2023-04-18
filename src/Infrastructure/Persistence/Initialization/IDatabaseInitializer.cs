using RAFFLE.WebApi.Infrastructure.Multitenancy;

namespace RAFFLE.WebApi.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(RAFFLETenantInfo tenant, CancellationToken cancellationToken);
}