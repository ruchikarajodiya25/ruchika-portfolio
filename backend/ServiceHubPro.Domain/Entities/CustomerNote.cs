using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class CustomerNote : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string Note { get; set; } = string.Empty;
    public string? NoteType { get; set; } // "General", "Call", "Email", "Meeting", etc.
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ApplicationUser? CreatedByUser { get; set; }
}
