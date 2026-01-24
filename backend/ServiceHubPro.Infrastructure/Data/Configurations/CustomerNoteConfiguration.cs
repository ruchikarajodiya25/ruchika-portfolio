using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Data.Configurations;

public class CustomerNoteConfiguration : IEntityTypeConfiguration<CustomerNote>
{
    public void Configure(EntityTypeBuilder<CustomerNote> builder)
    {
        builder.HasKey(cn => cn.Id);
        builder.Property(cn => cn.Note).IsRequired().HasMaxLength(2000);
        builder.Property(cn => cn.NoteType).HasMaxLength(50);
        builder.Property(cn => cn.TenantId).IsRequired();
        builder.Property(cn => cn.CustomerId).IsRequired();
        
        builder.HasIndex(cn => cn.TenantId);
        builder.HasIndex(cn => cn.CustomerId);

        builder.HasOne(cn => cn.Tenant)
            .WithMany()
            .HasForeignKey(cn => cn.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cn => cn.Customer)
            .WithMany(c => c.NotesHistory)
            .HasForeignKey(cn => cn.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // CreatedByUser relationship (ApplicationUser is in Infrastructure layer)
        builder.HasOne<ServiceHubPro.Infrastructure.Entities.ApplicationUser>()
            .WithMany()
            .HasForeignKey(cn => cn.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
