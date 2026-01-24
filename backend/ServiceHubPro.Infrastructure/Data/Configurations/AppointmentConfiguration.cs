using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TenantId).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => new { a.TenantId, a.ScheduledStart });
        builder.HasIndex(a => new { a.TenantId, a.StaffId, a.ScheduledStart });

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Staff relationship (ApplicationUser is in Infrastructure layer)
        builder.HasOne<ServiceHubPro.Infrastructure.Entities.ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Location)
            .WithMany(l => l.Appointments)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
