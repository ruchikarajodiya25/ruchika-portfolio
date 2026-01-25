using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.BusinessName).HasMaxLength(200);
        builder.Property(t => t.Email).HasMaxLength(100);
        builder.Property(t => t.Phone).HasMaxLength(20);
        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.Email);
    }
}
