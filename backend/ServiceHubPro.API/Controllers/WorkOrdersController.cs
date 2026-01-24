using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.WorkOrders.Commands;
using ServiceHubPro.Application.Features.WorkOrders.Queries;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkOrderDto>>>> GetWorkOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? assignedToUserId = null)
    {
        var query = new GetWorkOrdersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            CustomerId = customerId,
            AssignedToUserId = assignedToUserId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<WorkOrderDto>>> CreateWorkOrder([FromBody] CreateWorkOrderDto dto)
    {
        var command = new CreateWorkOrderCommand
        {
            CustomerId = dto.CustomerId,
            AppointmentId = dto.AppointmentId,
            AssignedToUserId = dto.AssignedToUserId,
            LocationId = dto.LocationId,
            Description = dto.Description,
            InternalNotes = dto.InternalNotes
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetWorkOrders), new { id = result.Data!.Id }, result);
    }
}
