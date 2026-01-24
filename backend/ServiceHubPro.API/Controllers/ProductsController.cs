using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Products.Commands;
using ServiceHubPro.Application.Features.Products.Queries;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] bool? lowStock = null)
    {
        var query = new GetProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            LocationId = locationId,
            LowStock = lowStock
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand
        {
            LocationId = dto.LocationId,
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Category = dto.Category,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            Unit = dto.Unit,
            IsActive = dto.IsActive
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Category = dto.Category,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            Unit = dto.Unit,
            IsActive = dto.IsActive
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return result.Data == null ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
