using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class InvoiceItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; set; }
    public string ItemType { get; set; } = string.Empty; // "Service", "Product", "Labor"
    public Guid? ServiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    
    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public Service? Service { get; set; }
    public Product? Product { get; set; }
}
