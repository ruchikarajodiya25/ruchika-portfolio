using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Commands;

public class UpdateWorkOrderCommand : IRequest<ApiResponse<WorkOrderDto>>
{
    public Guid Id { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InternalNotes { get; set; }
}

public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, ApiResponse<WorkOrderDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateWorkOrderCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<WorkOrderDto>> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<WorkOrderDto>.ErrorResponse("Tenant context not found");
        }

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

        if (workOrder == null)
        {
            return ApiResponse<WorkOrderDto>.ErrorResponse("Work order not found");
        }

        var validStatuses = new[] { "Draft", "InProgress", "OnHold", "Completed", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
        {
            return ApiResponse<WorkOrderDto>.ErrorResponse($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        if (request.AssignedToUserId.HasValue)
        {
            workOrder.AssignedToUserId = request.AssignedToUserId.Value;
        }

        workOrder.Status = request.Status;
        
        if (request.Status == "InProgress" && workOrder.StartedAt == null)
        {
            workOrder.StartedAt = DateTime.UtcNow;
        }

        if (request.Status == "Completed" && workOrder.CompletedAt == null)
        {
            workOrder.CompletedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            workOrder.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.InternalNotes))
        {
            workOrder.InternalNotes = request.InternalNotes;
        }

        workOrder.UpdatedAt = DateTime.UtcNow;
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
            StartedAt = workOrder.StartedAt,
            CompletedAt = workOrder.CompletedAt,
            CreatedAt = workOrder.CreatedAt
        };

        return ApiResponse<WorkOrderDto>.SuccessResponse(dto, "Work order updated successfully");
    }
}
