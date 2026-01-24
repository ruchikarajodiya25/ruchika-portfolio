using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class AuditLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // "Create", "Update", "Delete", "SoftDelete", "Restore"
    public Guid? UserId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Changes { get; set; } // JSON with field diffs
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
