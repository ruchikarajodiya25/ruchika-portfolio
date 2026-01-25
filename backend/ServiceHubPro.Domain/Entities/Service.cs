using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Service : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? TaxCategory { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<WorkOrderItem> WorkOrderItems { get; set; } = new List<WorkOrderItem>();
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
