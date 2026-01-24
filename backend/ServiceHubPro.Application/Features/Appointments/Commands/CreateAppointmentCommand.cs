using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Appointments.Commands;

public class CreateAppointmentCommand : IRequest<ApiResponse<AppointmentDto>>
{
    public Guid CustomerId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateAppointmentCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse("Tenant context not found");
        }

        // Check for conflicts
        var hasConflict = await _context.Appointments
            .Where(a => a.TenantId == tenantId.Value && 
                       !a.IsDeleted &&
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

        // Validate service duration if provided
        if (request.ServiceId.HasValue)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == request.ServiceId.Value && s.TenantId == tenantId.Value, cancellationToken);
            
            if (service != null)
            {
                var expectedDuration = (request.ScheduledEnd - request.ScheduledStart).TotalMinutes;
                if (Math.Abs(expectedDuration - service.DurationMinutes) > 5) // Allow 5 minute tolerance
                {
                    return ApiResponse<AppointmentDto>.ErrorResponse($"Appointment duration should be approximately {service.DurationMinutes} minutes for this service");
                }
            }
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            CustomerId = request.CustomerId,
            ServiceId = request.ServiceId,
            StaffId = request.StaffId,
            LocationId = request.LocationId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Status = "Scheduled",
            Notes = request.Notes,
            InternalNotes = request.InternalNotes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
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
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ApiResponse<AppointmentDto>.SuccessResponse(dto!, "Appointment created successfully");
    }
}
