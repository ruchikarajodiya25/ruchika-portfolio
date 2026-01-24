using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Invoices.Commands;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("from-workorder/{workOrderId}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoiceFromWorkOrder(Guid workOrderId)
    {
        var command = new CreateInvoiceFromWorkOrderCommand { WorkOrderId = workOrderId };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(CreateInvoiceFromWorkOrder), new { id = result.Data!.Id }, result);
    }
}
