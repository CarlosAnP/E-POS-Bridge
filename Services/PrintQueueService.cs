using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EposBridge.Models;

namespace EposBridge.Services;

public class PrintQueueService : BackgroundService
{
    private readonly Channel<PrintJob> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrintQueueService> _logger;

    public PrintQueueService(IServiceProvider serviceProvider, ILogger<PrintQueueService> logger)
    {
        _queue = Channel.CreateUnbounded<PrintJob>();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task EnqueueJobAsync(PrintJob job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var printerService = scope.ServiceProvider.GetRequiredService<PrinterService>();
                    var historyService = scope.ServiceProvider.GetRequiredService<PrintHistoryService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    // Execute print
                    bool success = false;
                    try 
                    {
                        byte[] data = Convert.FromBase64String(job.ContentBase64);
                        success = printerService.PrintRaw(job.PrinterName, data);
                    }
                    catch (Exception ex)
                    {
                        job.ErrorMessage = "Error decoding data: " + ex.Message;
                    }

                    // Update job status
                    job.Status = success ? "Success" : "Failed";
                    if (!success && string.IsNullOrEmpty(job.ErrorMessage)) job.ErrorMessage = "Error de comunicación con impresora";

                    // Save to history
                    historyService.AddJob(job);

                    // Notify
                    if (success)
                    {
                        notificationService.ShowNotification("Impresión Exitosa", $"Ticket enviado a {job.PrinterName}", ToolTipIcon.Info);
                    }
                    else
                    {
                        notificationService.ShowNotification("Error de Impresión", $"Falló impresión en {job.PrinterName}", ToolTipIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing print job");
            }
        }
    }
}
