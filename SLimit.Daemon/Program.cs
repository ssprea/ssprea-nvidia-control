using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using GpuSSharp;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;


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

        if (Environment.UserName != "root")
        {
            Log.Fatal("Run the program as root.");
            return;
        }
        
        if (File.Exists(socketPath))
            File.Delete(socketPath);

        var sw = Stopwatch.StartNew();
        Log.Information("[{dateTime}] [{swElapsedMs}] Starting SSLimit Daemon...", DateTime.Now,sw.ElapsedMilliseconds);
        
        GpuService ??= new GpuService();

        if (GpuService.GpuList.Count == 0)
        {
            Log.Fatal("No supported GPUs found! Quitting.");
            return;
        }
        Log.Information("[{dateTime}] [{swElapsedMs}] GPU service started successfully", DateTime.Now,sw.ElapsedMilliseconds);
       
        
        
        
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions()
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        Log.Information("[{dateTime}] [{swElapsedMs}] Builder created", DateTime.Now,sw.ElapsedMilliseconds);

        builder.Services.AddSerilog(log, dispose: false);
        
        builder.Host.UseConsoleLifetime();
        
        builder.WebHost.UseKestrelCore();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenUnixSocket(socketPath, endpoint =>
            {
                endpoint.Protocols = HttpProtocols.Http2;
            });
        });

        Log.Information("[{dateTime}] [{swElapsedMs}] Kestrel created", DateTime.Now,sw.ElapsedMilliseconds);
        
        
        builder.Services.AddRouting();
        builder.Services.AddGrpc();
        
        Log.Information("[{dateTime}] [{swElapsedMs}] Grpc added", DateTime.Now,sw.ElapsedMilliseconds);
        

        await using var app = builder.Build();
        
        Log.Information("[{dateTime}] [{swElapsedMs}] App built", DateTime.Now,sw.ElapsedMilliseconds);
        

        app.MapGrpcService<GpuControlService>();
        
        Log.Information("[{dateTime}] [{swElapsedMs}] Service mapped", DateTime.Now,sw.ElapsedMilliseconds);
        




        try
        {
            await app.StartAsync();

            Log.Information("[{dateTime}] [{swElapsedMs}] App started", DateTime.Now,sw.ElapsedMilliseconds);
            
            
            File.SetUnixFileMode(
                socketPath,
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
            
            Log.Information("[{dateTime}] [{swElapsedMs}] Socket configured", DateTime.Now,sw.ElapsedMilliseconds);
            

            Log.Information("[{dateTime}] [{swElapsedMs}] Daemon started successfully, ready for clients.", DateTime.Now, sw.ElapsedMilliseconds);
            
            sw.Stop();
            
            await app.WaitForShutdownAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal("Failed to start SLimit Daemon: {exMsg}",ex);
        }
        
        await app.StopAsync();
        File.Delete(socketPath);
    }
}

