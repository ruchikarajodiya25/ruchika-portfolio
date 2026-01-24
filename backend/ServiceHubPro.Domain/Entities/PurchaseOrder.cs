using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class PurchaseOrder : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Ordered, PartiallyReceived, Received, Cancelled
    public decimal TotalAmount { get; set; } = 0;
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public Location Location { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
