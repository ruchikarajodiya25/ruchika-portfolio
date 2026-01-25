using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Vendor : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
