using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Services.Commands;

public class UpdateServiceCommand : IRequest<ApiResponse<ServiceDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? TaxCategory { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ApiResponse<ServiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateServiceCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ServiceDto>.ErrorResponse("Tenant context not found");
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.TenantId == tenantId.Value && !s.IsDeleted, cancellationToken);

        if (service == null)
        {
            return ApiResponse<ServiceDto>.ErrorResponse("Service not found");
        }

        service.Name = request.Name;
        service.Description = request.Description;
        service.Price = request.Price;
        service.DurationMinutes = request.DurationMinutes;
        service.Category = request.Category;
        service.TaxCategory = request.TaxCategory;
        service.TaxRate = request.TaxRate;
        service.IsActive = request.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Category = service.Category,
            TaxCategory = service.TaxCategory,
            TaxRate = service.TaxRate,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };

        return ApiResponse<ServiceDto>.SuccessResponse(dto, "Service updated successfully");
    }
}
