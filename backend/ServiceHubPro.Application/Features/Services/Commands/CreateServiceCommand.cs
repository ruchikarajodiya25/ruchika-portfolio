using MediatR;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Services.Commands;

public class CreateServiceCommand : IRequest<ApiResponse<ServiceDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? TaxCategory { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ApiResponse<ServiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateServiceCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ServiceDto>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ServiceDto>.ErrorResponse("Tenant context not found");
        }

        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            Category = request.Category,
            TaxCategory = request.TaxCategory,
            TaxRate = request.TaxRate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Services.Add(service);
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
            CreatedAt = service.CreatedAt
        };

        return ApiResponse<ServiceDto>.SuccessResponse(dto, "Service created successfully");
    }
}
