using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<ApiResponse<ProductDto>>
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 10;
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateProductCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ProductDto>.ErrorResponse("Tenant context not found");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            LocationId = request.LocationId,
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            Category = request.Category,
            UnitPrice = request.UnitPrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            Unit = request.Unit,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Category = product.Category,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            Unit = product.Unit,
            IsActive = product.IsActive,
            LocationId = product.LocationId,
            CreatedAt = product.CreatedAt
        };

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product created successfully");
    }
}
