using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Notification : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "AppointmentReminder", "InvoiceSent", "LowStock", "System"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? LinkUrl { get; set; }
    public string? RelatedEntityType { get; set; } // "Appointment", "Invoice", "Product", etc.
    public Guid? RelatedEntityId { get; set; }
    
    // Navigation properties
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
