using BlazorWasmSignalR.SignalRServer.BackgroundServices;
using BlazorWasmSignalR.SignalRServer.Hubs;

namespace BlazorWasmSignalR.SignalRServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
        });

        // Add services to the container.

        builder.Services.AddSignalR();
        builder.Services.AddSingleton<RealTimeDataStreamWriter>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseCors();

        app.MapHub<RealTimeDataHub>("/realtimedata");

        app.Run();
    }
}
