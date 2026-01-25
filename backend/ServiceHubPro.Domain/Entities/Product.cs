using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Product : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 10;
    public string? Unit { get; set; } // "Each", "Box", "Pack", etc.
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public ICollection<WorkOrderItem> WorkOrderItems { get; set; } = new List<WorkOrderItem>();
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    public ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
}
