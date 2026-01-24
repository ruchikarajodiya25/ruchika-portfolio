using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<ApiResponse<ProductDto>>
{
    public Guid Id { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetProductByIdQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ProductDto>.ErrorResponse("Tenant context not found");
        }

        var product = await _context.Products
            .Where(p => p.Id == request.Id && p.TenantId == tenantId.Value && !p.IsDeleted)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Category = p.Category,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                Unit = p.Unit,
                IsActive = p.IsActive,
                LocationId = p.LocationId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
        {
            return ApiResponse<ProductDto>.ErrorResponse("Product not found");
        }

        return ApiResponse<ProductDto>.SuccessResponse(product);
    }
}
