using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Domain.Common;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Entities;
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

        // Configure one-to-one relationship between Appointment and WorkOrder
        builder.Entity<Appointment>()
            .HasOne(a => a.WorkOrder)
            .WithOne(w => w.Appointment)
            .HasForeignKey<WorkOrder>(w => w.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure one-to-one relationship between WorkOrder and Invoice
        // Invoice is the dependent entity with WorkOrderId as the foreign key
        builder.Entity<WorkOrder>()
            .HasOne(w => w.Invoice)
            .WithOne(i => i.WorkOrder)
            .HasForeignKey<Invoice>(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // GLOBAL RULE: Set all foreign keys to NoAction by default to prevent cascade path conflicts
        // This runs AFTER all entity configurations, so it overrides any CASCADE set in config files
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                // Skip Identity-related FKs (they're managed by Identity framework)
                if (foreignKey.DeclaringEntityType.ClrType.Name.Contains("Identity") ||
                    foreignKey.PrincipalEntityType.ClrType.Name.Contains("Identity"))
                    continue;

                // Set all FKs to NoAction by default
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

        // Fix shadow FK warnings by explicitly configuring relationships with correct FK properties
        // Note: Customer->Tenant and Product->Tenant are configured in CustomerConfiguration.cs and ProductConfiguration.cs
        // Do NOT duplicate them here to avoid shadow FK warnings (TenantId1)
        
        // ApplicationUser -> Tenant: Use ApplicationUser.TenantId (prevents TenantId1 shadow FK)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ApplicationUser -> Location: Use ApplicationUser.LocationId (prevents LocationId1 shadow FK)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Location)
            .WithMany()
            .HasForeignKey(u => u.LocationId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseOrderItem -> Product: Use PurchaseOrderItem.ProductId (prevents ProductId1 shadow FK)
        builder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Note: PurchaseOrder -> Tenant/Vendor/Location relationships are configured in PurchaseOrderConfiguration.cs
        // They are set to NoAction to prevent SQL 1785 multiple cascade paths error

        // Explicitly enable Cascade ONLY for true aggregate children (parent-child relationships)
        // PurchaseOrderItem -> PurchaseOrder: Cascade (child belongs to parent)
        builder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkOrderItem -> WorkOrder: Cascade
        builder.Entity<WorkOrderItem>()
            .HasOne(woi => woi.WorkOrder)
            .WithMany(wo => wo.Items)
            .HasForeignKey(woi => woi.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkOrderAttachment -> WorkOrder: Cascade
        builder.Entity<WorkOrderAttachment>()
            .HasOne(woa => woa.WorkOrder)
            .WithMany(wo => wo.Attachments)
            .HasForeignKey(woa => woa.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkOrderStatusHistory -> WorkOrder: Cascade
        builder.Entity<WorkOrderStatusHistory>()
            .HasOne(wosh => wosh.WorkOrder)
            .WithMany(wo => wo.StatusHistory)
            .HasForeignKey(wosh => wosh.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // InvoiceItem -> Invoice: Cascade
        builder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // CustomerNote -> Customer: Cascade
        builder.Entity<CustomerNote>()
            .HasOne(cn => cn.Customer)
            .WithMany(c => c.NotesHistory)
            .HasForeignKey(cn => cn.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global decimal precision configuration
        ConfigureDecimalPrecision(builder);

        // Global query filter for tenant isolation
        ApplyTenantFilter(builder);
    }

    private void ConfigureDecimalPrecision(ModelBuilder builder)
    {
        // Configure all decimal properties with default precision (18,2) for money amounts
        // Only set precision if not already configured
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    // Skip if precision is already set (from configuration files)
                    if (property.GetPrecision().HasValue)
                        continue;

                    var propertyName = property.Name;
                    
                    // Quantity fields: (18,4) - for fractional quantities
                    // Includes: InvoiceItem.Quantity, WorkOrderItem.Quantity, and any QuantityOrdered/QuantityReceived that are decimal
                    if (propertyName.Contains("Quantity", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetColumnType("decimal(18,4)");
                        property.SetPrecision(18);
                        property.SetScale(4);
                    }
                    // TaxRate fields: (5,2) - allows 0.00 to 100.00 (percent)
                    else if (propertyName.Contains("TaxRate", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetColumnType("decimal(5,2)");
                        property.SetPrecision(5);
                        property.SetScale(2);
                    }
                    // All other decimal fields (money amounts): (18,2)
                    else
                    {
                        property.SetColumnType("decimal(18,2)");
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }
        }
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
