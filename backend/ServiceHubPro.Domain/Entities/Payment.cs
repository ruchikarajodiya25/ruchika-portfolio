using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Payment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Online, Check, Other
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? ProcessedByUserId { get; set; }
    
    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
