using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Appointments.Commands;

public class UpdateAppointmentStatusCommand : IRequest<ApiResponse<AppointmentDto>>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, ApiResponse<AppointmentDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateAppointmentStatusCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse("Tenant context not found");
        }

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.TenantId == tenantId.Value && !a.IsDeleted, cancellationToken);

        if (appointment == null)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");
        }

        var validStatuses = new[] { "Scheduled", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow" };
        if (!validStatuses.Contains(request.Status))
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        appointment.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            appointment.Notes = request.Notes;
        }
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            ServiceId = appointment.ServiceId,
            StaffId = appointment.StaffId,
            LocationId = appointment.LocationId,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            Status = appointment.Status,
            Notes = appointment.Notes,
            InternalNotes = appointment.InternalNotes,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };

        return ApiResponse<AppointmentDto>.SuccessResponse(dto, "Appointment status updated successfully");
    }
}
