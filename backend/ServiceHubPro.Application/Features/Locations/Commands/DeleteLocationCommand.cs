using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Locations.Commands;

public class DeleteLocationCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public DeleteLocationCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.TenantId == tenantId.Value && !l.IsDeleted, cancellationToken);

        if (location == null)
        {
            return ApiResponse<object>.ErrorResponse("Location not found");
        }

        // Check if location has associated appointments or work orders
        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.LocationId == request.Id && !a.IsDeleted, cancellationToken);

        var hasWorkOrders = await _context.WorkOrders
            .AnyAsync(w => w.LocationId == request.Id && !w.IsDeleted, cancellationToken);

        if (hasAppointments || hasWorkOrders)
        {
            return ApiResponse<object>.ErrorResponse("Cannot delete location with associated appointments or work orders");
        }

        location.IsDeleted = true;
        location.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Location deleted successfully");
    }
}
