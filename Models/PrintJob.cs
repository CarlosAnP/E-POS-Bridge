namespace EposBridge.Models;

public class PrintJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string PrinterName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Printed, Failed
    public string? ErrorMessage { get; set; }
}
