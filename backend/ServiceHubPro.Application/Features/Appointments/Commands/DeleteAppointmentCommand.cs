using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Appointments.Commands;

public class DeleteAppointmentCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public DeleteAppointmentCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.TenantId == tenantId.Value && !a.IsDeleted, cancellationToken);

        if (appointment == null)
        {
            return ApiResponse<object>.ErrorResponse("Appointment not found");
        }

        // Soft delete by changing status to Cancelled
        appointment.Status = "Cancelled";
        appointment.IsDeleted = true;
        appointment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Appointment deleted successfully");
    }
}
