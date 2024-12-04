namespace LarpakeServer.Controllers;

[ApiController]
[Route("[Controller]")]
public class OpenApiController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOpenApi()
    {
        return new RedirectResult("/scalar/v1");
    }
}
