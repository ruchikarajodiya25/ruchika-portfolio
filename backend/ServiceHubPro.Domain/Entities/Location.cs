using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Location : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Working hours (stored as JSON or separate table - using JSON for simplicity)
    public string? WorkingHoursJson { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
