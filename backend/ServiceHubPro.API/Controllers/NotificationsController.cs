using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Notifications.Commands;
using ServiceHubPro.Application.Features.Notifications.Queries;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null)
    {
        var query = new GetNotificationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsRead = isRead,
            Type = type
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkNotificationAsReadCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
