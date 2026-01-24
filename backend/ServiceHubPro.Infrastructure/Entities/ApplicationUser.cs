using Microsoft.AspNetCore.Identity;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid? LocationId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Skills { get; set; } // JSON array or comma-separated
    public decimal? HourlyRate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation properties
    public Tenant? Tenant { get; set; }
    public Location? Location { get; set; }
    // Note: Navigation properties to Domain entities are configured in EF Core configurations
}
