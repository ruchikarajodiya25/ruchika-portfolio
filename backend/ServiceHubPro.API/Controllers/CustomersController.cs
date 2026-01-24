using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Customers.Commands;
using ServiceHubPro.Application.Features.Customers.Queries;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetCustomersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(Guid id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var command = new CreateCustomerCommand
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Mobile = dto.Mobile,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            DateOfBirth = dto.DateOfBirth,
            Notes = dto.Notes,
            Tags = dto.Tags
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetCustomer), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        var command = new UpdateCustomerCommand
        {
            Id = id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Mobile = dto.Mobile,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            DateOfBirth = dto.DateOfBirth,
            Notes = dto.Notes,
            Tags = dto.Tags
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
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
