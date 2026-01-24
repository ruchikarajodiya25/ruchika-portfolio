using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class WorkOrder : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, InProgress, OnHold, Completed, Cancelled
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public Guid? InvoiceId { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
    public Location Location { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();
    public ICollection<WorkOrderAttachment> Attachments { get; set; } = new List<WorkOrderAttachment>();
    public ICollection<WorkOrderStatusHistory> StatusHistory { get; set; } = new List<WorkOrderStatusHistory>();
}
