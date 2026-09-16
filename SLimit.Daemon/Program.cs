using System.Globalization;
using GpuSSharp;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using SLimit.Daemon;


namespace SLimit.Daemon;

public class Program
{
    public static GpuService? GpuService { get; set; }
    
    public static async Task Main(string[] args)
    {
        if (GpuService is null)
            GpuService = new GpuService();
        
        
        
        const string socketPath = "/run/slimit-grpc.sock";

        await using var log = new LoggerConfiguration() 
            .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
            .MinimumLevel.Debug()
            .CreateLogger();

        Log.Logger = log;
        
        
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenUnixSocket(socketPath, endpoint =>
            {
                endpoint.Protocols = HttpProtocols.Http2;
            });
        });

        builder.Services.AddGrpc();

        await using var app = builder.Build();

        app.MapGrpcService<GpuControlService>();
        
        


        await app.StartAsync();

        try
        {
            
            File.SetUnixFileMode(
                socketPath,
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite);

            await app.WaitForShutdownAsync();
        }
        finally
        {
            await app.StopAsync();
            File.Delete(socketPath);
        }
    }
}

