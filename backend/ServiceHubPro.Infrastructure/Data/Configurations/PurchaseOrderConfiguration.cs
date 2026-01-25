using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(po => po.Id);
        builder.Property(po => po.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(po => po.Status).IsRequired().HasMaxLength(50);
        builder.Property(po => po.TenantId).IsRequired();
        builder.Property(po => po.VendorId).IsRequired();
        builder.Property(po => po.LocationId).IsRequired();
        
        builder.HasIndex(po => po.TenantId);
        builder.HasIndex(po => new { po.TenantId, po.PurchaseOrderNumber });
        builder.HasIndex(po => po.VendorId);
        builder.HasIndex(po => po.LocationId);

        // CRITICAL: Configure PurchaseOrder relationships with NoAction to prevent SQL 1785 multiple cascade paths error
        // PurchaseOrder -> Tenant: NoAction
        builder.HasOne(po => po.Tenant)
            .WithMany()
            .HasForeignKey(po => po.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseOrder -> Vendor: NoAction
        builder.HasOne(po => po.Vendor)
            .WithMany(v => v.PurchaseOrders)
            .HasForeignKey(po => po.VendorId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseOrder -> Location: NoAction
        builder.HasOne(po => po.Location)
            .WithMany()
            .HasForeignKey(po => po.LocationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
