namespace ServiceHubPro.Application.DTOs;

public class CreateWorkOrderDto
{
    public Guid CustomerId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid LocationId { get; set; }
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
}
