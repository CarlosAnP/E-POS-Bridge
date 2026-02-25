using Microsoft.AspNetCore.Mvc;
using EposBridge.Services;
using EposBridge.Models;

namespace EposBridge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly PrintHistoryService _historyService;
    private readonly PrintQueueService _queueService;

    public HistoryController(PrintHistoryService historyService, PrintQueueService queueService)
    {
        _historyService = historyService;
        _queueService = queueService;
    }

    [HttpGet]
    public IActionResult GetHistory()
    {
        return Ok(_historyService.GetHistory());
    }

    [HttpPost("reprint/{id}")]
    public async Task<IActionResult> Reprint(string id)
    {
        var originalJob = _historyService.GetJob(id);
        if (originalJob == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        // Create a new job based on the old one
        var newJob = new PrintJob
        {
            PrinterName = originalJob.PrinterName,
            ContentBase64 = originalJob.ContentBase64,
            Status = "Pending"
        };

        await _queueService.EnqueueJobAsync(newJob);

        return Ok(new { success = true, jobId = newJob.Id, message = "Reprint queued" });
    }
}
