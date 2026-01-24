using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class StockAdjustment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityChange { get; set; } // Positive for increase, negative for decrease
    public string Reason { get; set; } = string.Empty; // "Received", "Sold", "Damaged", "Returned", "Adjustment"
    public string? Notes { get; set; }
    public Guid? AdjustedByUserId { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
