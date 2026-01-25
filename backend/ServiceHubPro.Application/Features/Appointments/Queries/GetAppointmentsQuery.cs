using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Appointments.Queries;

public class GetAppointmentsQuery : IRequest<ApiResponse<PagedResult<AppointmentDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid? LocationId { get; set; }
    public string? Status { get; set; }
}

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, ApiResponse<PagedResult<AppointmentDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetAppointmentsQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<AppointmentDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Appointments
            .Where(a => a.TenantId == tenantId.Value && !a.IsDeleted);

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.ScheduledStart >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.ScheduledEnd <= request.EndDate.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(a => a.CustomerId == request.CustomerId.Value);
        }

        if (request.StaffId.HasValue)
        {
            query = query.Where(a => a.StaffId == request.StaffId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(a => a.LocationId == request.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        query = query.OrderBy(a => a.ScheduledStart);

        var totalCount = await query.CountAsync(cancellationToken);

        var appointments = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                ServiceId = a.ServiceId,
                StaffId = a.StaffId,
                LocationId = a.LocationId,
                ScheduledStart = a.ScheduledStart,
                ScheduledEnd = a.ScheduledEnd,
                Status = a.Status,
                Notes = a.Notes,
                InternalNotes = a.InternalNotes,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AppointmentDto>
        {
            Items = appointments,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<AppointmentDto>>.SuccessResponse(result);
    }
}
