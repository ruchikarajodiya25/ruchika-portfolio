using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Domain.Common;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Services;
using System.Linq.Expressions;

namespace ServiceHubPro.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ITenantContextService? _tenantContextService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContextService tenantContextService) : base(options)
    {
        _tenantContextService = tenantContextService;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderItem> WorkOrderItems { get; set; }
    public DbSet<WorkOrderAttachment> WorkOrderAttachments { get; set; }
    public DbSet<WorkOrderStatusHistory> WorkOrderStatusHistories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for tenant isolation
        ApplyTenantFilter(builder);
    }

    private void ApplyTenantFilter(ModelBuilder builder)
    {
        // Note: Global query filters with dynamic tenant IDs are not supported in EF Core
        // Tenant filtering will be enforced in the application layer via ITenantContextService
        // This method is kept for potential future use with static filters if needed
        
        // We can add static filters for soft-deleted entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        EnforceTenantIsolation();
        UpdateBaseEntityTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceTenantIsolation();
        UpdateBaseEntityTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnforceTenantIsolation()
    {
        var tenantId = _tenantContextService?.GetCurrentTenantId();
        if (!tenantId.HasValue) return;

        var entries = ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = tenantId.Value;
            }
        }
    }

    private void UpdateBaseEntityTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                    }
                    break;
            }
        }
    }
}
