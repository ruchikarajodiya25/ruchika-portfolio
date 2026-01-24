using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Services.Commands;
using ServiceHubPro.Application.Features.Services.Queries;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ServiceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ServiceDto>>>> GetServices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetServicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Category = category,
            IsActive = isActive,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> GetService(Guid id)
    {
        var query = new GetServiceByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> CreateService([FromBody] CreateServiceDto dto)
    {
        var command = new CreateServiceCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            Category = dto.Category,
            TaxCategory = dto.TaxCategory,
            TaxRate = dto.TaxRate,
            IsActive = dto.IsActive
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetService), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> UpdateService(Guid id, [FromBody] UpdateServiceDto dto)
    {
        var command = new UpdateServiceCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            Category = dto.Category,
            TaxCategory = dto.TaxCategory,
            TaxRate = dto.TaxRate,
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
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var command = new DeleteServiceCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
