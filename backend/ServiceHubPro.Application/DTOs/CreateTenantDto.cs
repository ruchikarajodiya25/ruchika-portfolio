namespace ServiceHubPro.Application.DTOs;

public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
