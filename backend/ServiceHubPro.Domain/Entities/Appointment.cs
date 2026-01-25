using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Appointment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public bool IsReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Service? Service { get; set; }
    // Note: ApplicationUser (Staff) navigation is in Infrastructure layer to maintain Clean Architecture
    public Location Location { get; set; } = null!;
    public WorkOrder? WorkOrder { get; set; }
}
