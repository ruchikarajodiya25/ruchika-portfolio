namespace ServiceHubPro.Application.DTOs;

public class WorkOrderDto
{
    public Guid Id { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkOrderItemDto> Items { get; set; } = new();
}
