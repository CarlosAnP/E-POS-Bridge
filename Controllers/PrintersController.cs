using Microsoft.AspNetCore.Mvc;
using EposBridge.Services;

namespace EposBridge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrintersController : ControllerBase
{
    private readonly PrinterService _service;

    public PrintersController(PrinterService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_service.GetInstalledPrinters().Select(p => new { name = p, driver = "Unknown", port = "USB" }));
    }
}
