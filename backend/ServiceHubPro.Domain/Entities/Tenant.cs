using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SettingsJson { get; set; } // Store business settings as JSON
    
    // Navigation properties
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
