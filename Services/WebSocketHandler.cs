using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace EposBridge.Services;

public class WebSocketHandler
{
    private readonly PrinterService _printerService;
    private readonly SerialPortService _serialService;
    private readonly PrintQueueService _queueService;

    public WebSocketHandler(PrinterService printerService, SerialPortService serialService, PrintQueueService queueService)
    {
        _printerService = printerService;
        _serialService = serialService;
        _queueService = queueService;
    }

    public async Task Handle(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 64]; 
        
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                var content = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // We don't await ProcessMessage to avoid blocking the receive loop if processing is slow, 
                // but since we are just enqueuing, it is fast.
                await ProcessMessage(webSocket, content);
            }
        }
    }

    private async Task ProcessMessage(WebSocket ws, string json)
    {
        try 
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("type", out var t))
            {
                string type = t.GetString() ?? "";
                
                if (type == "print")
                {
                    string encoded = root.GetProperty("data").GetString() ?? "";
                    string printer = root.GetProperty("printer").GetString() ?? "";
                    
                    if (!string.IsNullOrEmpty(encoded))
                    {
                        var job = new Models.PrintJob
                        {
                            PrinterName = printer,
                            ContentBase64 = encoded
                        };
                        
                        await _queueService.EnqueueJobAsync(job);
                        await SendResponse(ws, "success", "Job queued");
                    }
                }
                else if (type == "open_drawer")
                {
                    string printer = root.GetProperty("printer").GetString() ?? "";
                    
                    if (!string.IsNullOrEmpty(printer))
                    {
                        // Command: ESC p 0 25 250
                        string drawerCmdBase64 = Convert.ToBase64String(new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA });
                        
                        var job = new Models.PrintJob
                        {
                            PrinterName = printer,
                            ContentBase64 = drawerCmdBase64,
                            // You might want to mark this as a "system" job or just treat as print
                        };
                        
                        await _queueService.EnqueueJobAsync(job);
                        await SendResponse(ws, "success", "Drawer job queued");
                    }
                }
                else
                {
                    await SendResponse(ws, "error", "Unknown type");
                }
            }
        }
        catch (Exception ex)
        {
            await SendResponse(ws, "error", ex.Message);
        }
    }


    private async Task SendResponse(WebSocket ws, string status, string message)
    {
        if (ws.State != WebSocketState.Open) return;
        var response = new { status = status, message = message };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
