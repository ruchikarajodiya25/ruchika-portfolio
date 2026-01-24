using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<ApiResponse<ProductDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateProductCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ProductDto>.ErrorResponse("Tenant context not found");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId.Value && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            return ApiResponse<ProductDto>.ErrorResponse("Product not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.SKU = request.SKU;
        product.Category = request.Category;
        product.UnitPrice = request.UnitPrice;
        product.CostPrice = request.CostPrice;
        product.StockQuantity = request.StockQuantity;
        product.LowStockThreshold = request.LowStockThreshold;
        product.Unit = request.Unit;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

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
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product updated successfully");
    }
}
