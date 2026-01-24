using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.WorkOrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(w => w.Status).IsRequired().HasMaxLength(50);
        builder.Property(w => w.TenantId).IsRequired();
        builder.HasIndex(w => w.TenantId);
        builder.HasIndex(w => new { w.TenantId, w.WorkOrderNumber }).IsUnique();
        builder.HasIndex(w => new { w.TenantId, w.Status });

        builder.HasOne(w => w.Tenant)
            .WithMany()
            .HasForeignKey(w => w.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Customer)
            .WithMany(c => c.WorkOrders)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // AssignedToUser relationship (ApplicationUser is in Infrastructure layer)
        builder.HasOne<ServiceHubPro.Infrastructure.Entities.ApplicationUser>()
            .WithMany()
            .HasForeignKey(w => w.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
