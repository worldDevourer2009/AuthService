using AuthService.Application.Queries.Users;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    [Route("get-user-id")]
    public async Task<IActionResult> GetUserById([FromBody] GetUserByIdDto entry)
    {
        var request = new GetUserByIdRequest(entry.Id);

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return BadRequest(new { Success = response.Success, Message = response.User });
        }
        
        return Ok(new { Success = response.Success, User = response.User });
    }

    [HttpGet]
    [Route("get-user-email")]
    public async Task<IActionResult> GetUserByEmail([FromBody] GetUserByEmailDto entry)
    {
        var request = new GetUserByEmailQueryRequest(entry.Email);
        
        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return BadRequest(new {Success = response.Success, User = response.User});
        }
        
        return Ok(new {Success = response.Success, User = response.User});
    }
}