using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQuery : IRequest<ApiResponse<DashboardStatsDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalAppointments { get; set; }
    public int ActiveAppointments { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingInvoices { get; set; }
    public decimal PendingInvoiceAmount { get; set; }
    public int LowStockProducts { get; set; }
    public List<TopServiceDto> TopServices { get; set; } = new();
    public List<RecentAppointmentDto> RecentAppointments { get; set; } = new();
}

public class TopServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentAppointmentDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashboardStatsDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetDashboardStatsQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<DashboardStatsDto>.ErrorResponse("Tenant context not found");
        }

        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var stats = new DashboardStatsDto
        {
            TotalRevenue = await _context.Invoices
                .Where(i => i.TenantId == tenantId.Value && 
                           !i.IsDeleted && 
                           i.Status == "Paid" &&
                           i.InvoiceDate >= startDate && 
                           i.InvoiceDate <= endDate)
                .SumAsync(i => i.TotalAmount, cancellationToken),

            TotalAppointments = await _context.Appointments
                .Where(a => a.TenantId == tenantId.Value && 
                           !a.IsDeleted &&
                           a.ScheduledStart >= startDate && 
                           a.ScheduledStart <= endDate)
                .CountAsync(cancellationToken),

            ActiveAppointments = await _context.Appointments
                .Where(a => a.TenantId == tenantId.Value && 
                           !a.IsDeleted &&
                           a.Status != "Completed" && 
                           a.Status != "Cancelled" && 
                           a.Status != "NoShow")
                .CountAsync(cancellationToken),

            TotalCustomers = await _context.Customers
                .Where(c => c.TenantId == tenantId.Value && !c.IsDeleted)
                .CountAsync(cancellationToken),

            PendingInvoices = await _context.Invoices
                .Where(i => i.TenantId == tenantId.Value && 
                           !i.IsDeleted && 
                           i.Status == "Pending")
                .CountAsync(cancellationToken),

            PendingInvoiceAmount = await _context.Invoices
                .Where(i => i.TenantId == tenantId.Value && 
                           !i.IsDeleted && 
                           i.Status == "Pending")
                .SumAsync(i => i.TotalAmount, cancellationToken),

            LowStockProducts = await _context.Products
                .Where(p => p.TenantId == tenantId.Value && 
                           !p.IsDeleted && 
                           p.IsActive &&
                           p.StockQuantity <= p.LowStockThreshold)
                .CountAsync(cancellationToken)
        };

        // Top services
        stats.TopServices = await _context.Appointments
            .Where(a => a.TenantId == tenantId.Value && 
                       !a.IsDeleted &&
                       a.ServiceId.HasValue &&
                       a.ScheduledStart >= startDate && 
                       a.ScheduledStart <= endDate)
            .GroupBy(a => new { a.ServiceId, a.Service!.Name })
            .Select(g => new TopServiceDto
            {
                ServiceId = g.Key.ServiceId!.Value,
                ServiceName = g.Key.Name,
                Count = g.Count(),
                Revenue = g.Sum(a => a.Service!.Price)
            })
            .OrderByDescending(s => s.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Recent appointments
        stats.RecentAppointments = await _context.Appointments
            .Where(a => a.TenantId == tenantId.Value && !a.IsDeleted)
            .OrderByDescending(a => a.ScheduledStart)
            .Take(5)
            .Select(a => new RecentAppointmentDto
            {
                Id = a.Id,
                CustomerName = a.Customer.FirstName + " " + a.Customer.LastName,
                ScheduledStart = a.ScheduledStart,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<DashboardStatsDto>.SuccessResponse(stats);
    }
}
