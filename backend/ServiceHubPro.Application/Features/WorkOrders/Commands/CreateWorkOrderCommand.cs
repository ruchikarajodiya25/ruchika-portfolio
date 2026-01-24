using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Commands;

public class CreateWorkOrderCommand : IRequest<ApiResponse<WorkOrderDto>>
{
    public Guid CustomerId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid LocationId { get; set; }
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
}

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, ApiResponse<WorkOrderDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateWorkOrderCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<WorkOrderDto>.ErrorResponse("Tenant context not found");
        }

        // Generate work order number
        var lastOrder = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId.Value)
            .OrderByDescending(w => w.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var orderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{((lastOrder?.CreatedAt.Date == DateTime.UtcNow.Date ? 
            await _context.WorkOrders.CountAsync(w => w.TenantId == tenantId.Value && 
                w.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken) : 0) + 1):D4}";

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            WorkOrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            AppointmentId = request.AppointmentId,
            AssignedToUserId = request.AssignedToUserId,
            LocationId = request.LocationId,
            Status = "Draft",
            Description = request.Description,
            InternalNotes = request.InternalNotes,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new WorkOrderDto
        {
            Id = workOrder.Id,
            WorkOrderNumber = workOrder.WorkOrderNumber,
            CustomerId = workOrder.CustomerId,
            AppointmentId = workOrder.AppointmentId,
            AssignedToUserId = workOrder.AssignedToUserId,
            LocationId = workOrder.LocationId,
            Status = workOrder.Status,
            Description = workOrder.Description,
            InternalNotes = workOrder.InternalNotes,
            TotalAmount = workOrder.TotalAmount,
            CreatedAt = workOrder.CreatedAt
        };

        return ApiResponse<WorkOrderDto>.SuccessResponse(dto, "Work order created successfully");
    }
}
