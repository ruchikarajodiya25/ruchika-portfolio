using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class Customer : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; } // JSON array or comma-separated
    public decimal TotalSpent { get; set; } = 0;
    public int TotalVisits { get; set; } = 0;
    public DateTime? LastVisitAt { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CustomerNote> NotesHistory { get; set; } = new List<CustomerNote>();
}
