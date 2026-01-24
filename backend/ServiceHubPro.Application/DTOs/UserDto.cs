namespace ServiceHubPro.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public Guid? LocationId { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
}
