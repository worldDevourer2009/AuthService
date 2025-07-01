using AuthService.Application.Queries.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskHandler.Shared.DTO.Users.Queries;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var idGuid = Guid.Parse(id);

        if (idGuid == Guid.Empty)
        {
            return BadRequest(new { Success = false, Message = "Invalid Id" });
        }
        
        var request = new GetUserByIdRequest(idGuid);

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return BadRequest(new { Success = response.Success, Message = response.User });
        }
        var user = response.User;
        
        if (user is null)
        {
            return BadRequest(new { Success = response.Success, Message = response.User });
        }
        
        return Ok(new UserDto()
        {
            Id = user.UserIdentity.Id,
            FirstName = user.UserIdentity.FirstName!,
            LastName = user.UserIdentity.LastName!,
            Email = user.Email.EmailAddress!,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive,
        });
    }

    [HttpGet("by-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        var request = new GetUserByEmailQueryRequest(email);
        
        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return BadRequest(new {Success = response.Success, User = response.User});
        }
        
        var user = response.User;
        
        if (user is null)
        {
            return BadRequest(new { Success = response.Success, Message = response.User });
        }
        
        return Ok(new UserDto()
        {
            Id = user.UserIdentity.Id,
            FirstName = user.UserIdentity.FirstName!,
            LastName = user.UserIdentity.LastName!,
            Email = user.Email.EmailAddress!,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive,
        });
    }
}