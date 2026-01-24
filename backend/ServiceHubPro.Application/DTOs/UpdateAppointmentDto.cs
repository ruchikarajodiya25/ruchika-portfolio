namespace ServiceHubPro.Application.DTOs;

public class UpdateAppointmentDto
{
    public Guid CustomerId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}
