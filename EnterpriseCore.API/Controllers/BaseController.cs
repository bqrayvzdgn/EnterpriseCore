using EnterpriseCore.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { message = result.Error }),
            "FORBIDDEN" => Forbid(),
            "UNAUTHORIZED" => Unauthorized(new { message = result.Error }),
            _ => BadRequest(new { message = result.Error, code = result.ErrorCode })
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { message = result.Error }),
            "FORBIDDEN" => Forbid(),
            "UNAUTHORIZED" => Unauthorized(new { message = result.Error }),
            _ => BadRequest(new { message = result.Error, code = result.ErrorCode })
        };
    }
}
