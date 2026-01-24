using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Appointments.Commands;

public class UpdateAppointmentCommand : IRequest<ApiResponse<AppointmentDto>>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateAppointmentCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
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

        // Check for conflicts (excluding current appointment)
        var hasConflict = await _context.Appointments
            .Where(a => a.TenantId == tenantId.Value && 
                       !a.IsDeleted &&
                       a.Id != request.Id &&
                       a.Status != "Cancelled" &&
                       a.Status != "NoShow" &&
                       ((a.StaffId.HasValue && request.StaffId.HasValue && a.StaffId == request.StaffId) ||
                        (a.LocationId == request.LocationId)) &&
                       ((a.ScheduledStart <= request.ScheduledStart && a.ScheduledEnd > request.ScheduledStart) ||
                        (a.ScheduledStart < request.ScheduledEnd && a.ScheduledEnd >= request.ScheduledEnd) ||
                        (a.ScheduledStart >= request.ScheduledStart && a.ScheduledEnd <= request.ScheduledEnd)))
            .AnyAsync(cancellationToken);

        if (hasConflict)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse("Appointment conflicts with an existing appointment");
        }

        appointment.CustomerId = request.CustomerId;
        appointment.ServiceId = request.ServiceId;
        appointment.StaffId = request.StaffId;
        appointment.LocationId = request.LocationId;
        appointment.ScheduledStart = request.ScheduledStart;
        appointment.ScheduledEnd = request.ScheduledEnd;
        appointment.Notes = request.Notes;
        appointment.InternalNotes = request.InternalNotes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await _context.Appointments
            .Where(a => a.Id == appointment.Id)
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
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ApiResponse<AppointmentDto>.SuccessResponse(dto!, "Appointment updated successfully");
    }
}
