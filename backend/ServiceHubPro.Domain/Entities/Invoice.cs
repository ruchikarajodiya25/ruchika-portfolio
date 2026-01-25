using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Invoice : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? WorkOrderId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, PartiallyPaid, Overdue, Cancelled
    public decimal SubTotal { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public WorkOrder? WorkOrder { get; set; }
    public Location Location { get; set; } = null!;
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
