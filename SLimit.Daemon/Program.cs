using System.Globalization;
using System.Runtime.InteropServices;
using GpuSSharp;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using SLimit.Daemon;


namespace SLimit.Daemon;

public class Program
{
    public static GpuService? GpuService { get; private set; }
    
    public static async Task Main(string[] args)
    {
        const string socketPath = "/run/slimit-grpc.sock";

        await using var log = new LoggerConfiguration() 
            .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
            .MinimumLevel.Debug()
            .CreateLogger();

        Log.Logger = log;
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Log.Fatal("Only Linux is supported currently.");
            return;
        }
        
        Log.Information("[{dateTime}] Starting SLimit Daemon...", DateTime.Now);
        
        if (GpuService is null)
            GpuService = new GpuService();

       
        
        
        
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

        Log.Information("[{dateTime}] Daemon started successfully, ready for clients.", DateTime.Now);
        
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

