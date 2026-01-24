using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Queries;

public class GetWorkOrdersQuery : IRequest<ApiResponse<PagedResult<WorkOrderDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, ApiResponse<PagedResult<WorkOrderDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetWorkOrdersQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<WorkOrderDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<WorkOrderDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.WorkOrders
            .Where(w => w.TenantId == tenantId.Value && !w.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(w => w.Status == request.Status);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(w => w.CustomerId == request.CustomerId.Value);
        }

        if (request.AssignedToUserId.HasValue)
        {
            query = query.Where(w => w.AssignedToUserId == request.AssignedToUserId.Value);
        }

        query = query.OrderByDescending(w => w.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var workOrders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                WorkOrderNumber = w.WorkOrderNumber,
                CustomerId = w.CustomerId,
                AppointmentId = w.AppointmentId,
                AssignedToUserId = w.AssignedToUserId,
                LocationId = w.LocationId,
                Status = w.Status,
                Description = w.Description,
                InternalNotes = w.InternalNotes,
                TotalAmount = w.TotalAmount,
                StartedAt = w.StartedAt,
                CompletedAt = w.CompletedAt,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<WorkOrderDto>
        {
            Items = workOrders,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiResponse<PagedResult<WorkOrderDto>>.SuccessResponse(result);
    }
}
