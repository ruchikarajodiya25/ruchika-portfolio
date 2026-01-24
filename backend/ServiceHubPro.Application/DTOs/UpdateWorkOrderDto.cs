namespace ServiceHubPro.Application.DTOs;

public class UpdateWorkOrderDto
{
    public Guid? AssignedToUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
}
