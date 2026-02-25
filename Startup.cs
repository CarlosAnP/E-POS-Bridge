using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EposBridge;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        // Services
        services.AddSingleton<Services.PrinterService>();
        services.AddSingleton<Services.SerialPortService>();
        services.AddSingleton<Services.WebSocketHandler>();
        
        // Phase 2 Services
        services.AddSingleton<Services.INotificationService, Services.NotificationService>();
        services.AddSingleton<Services.PrintHistoryService>();
        
        // Fix: Ensure the Singleton instance and the HostedService instance are the SAME object
        services.AddSingleton<Services.PrintQueueService>();
        services.AddHostedService<Services.PrintQueueService>(provider => provider.GetRequiredService<Services.PrintQueueService>());

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Enable Swagger even in Prod for this local tool
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseWebSockets();
        app.UseCors("AllowAll");

        app.UseDefaultFiles();
        app.UseStaticFiles();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = context.RequestServices.GetRequiredService<Services.WebSocketHandler>();
                    await handler.Handle(ws);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });
        });
    }
}
