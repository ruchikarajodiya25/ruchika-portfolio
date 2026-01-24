using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Locations.Commands;

public class UpdateLocationCommand : IRequest<ApiResponse<LocationDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, ApiResponse<LocationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateLocationCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<LocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<LocationDto>.ErrorResponse("Tenant context not found");
        }

        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.TenantId == tenantId.Value && !l.IsDeleted, cancellationToken);

        if (location == null)
        {
            return ApiResponse<LocationDto>.ErrorResponse("Location not found");
        }

        location.Name = request.Name;
        location.Address = request.Address;
        location.City = request.City;
        location.State = request.State;
        location.ZipCode = request.ZipCode;
        location.Phone = request.Phone;
        location.Email = request.Email;
        location.IsActive = request.IsActive;
        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            State = location.State,
            ZipCode = location.ZipCode,
            Phone = location.Phone,
            Email = location.Email,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt
        };

        return ApiResponse<LocationDto>.SuccessResponse(dto, "Location updated successfully");
    }
}
