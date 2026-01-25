using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class PurchaseOrderItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
