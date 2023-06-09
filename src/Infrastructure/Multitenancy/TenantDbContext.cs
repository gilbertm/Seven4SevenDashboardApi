﻿using Finbuckle.MultiTenant.Stores;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UNIFIEDDASHBOARD.WebApi.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<RAFFLETenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RAFFLETenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}