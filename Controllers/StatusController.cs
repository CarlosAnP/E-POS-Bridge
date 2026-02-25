using Microsoft.AspNetCore.Mvc;

namespace EposBridge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "online", version = "1.0.0" });
    }
}
