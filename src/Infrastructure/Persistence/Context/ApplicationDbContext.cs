using Finbuckle.MultiTenant;
using RAFFLE.WebApi.Application.Common.Events;
using RAFFLE.WebApi.Application.Common.Interfaces;
using RAFFLE.WebApi.Domain.Catalog;
using RAFFLE.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace RAFFLE.WebApi.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(currentTenant, options, currentUser, serializer, dbSettings, events)
    {
    }


    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Catalog);
    }
}