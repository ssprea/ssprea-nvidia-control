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
        Log.Information(" [{swElapsedMs}] Starting sLimit Daemon...", sw.ElapsedMilliseconds);
        
        GpuService ??= new GpuService();

        if (GpuService.GpuList.Count == 0)
        {
            Log.Fatal("No supported GPUs found! Quitting.");
            return;
        }
        Log.Information(" [{swElapsedMs}] GPU service started successfully", sw.ElapsedMilliseconds);
       
        
        
        
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions()
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        Log.Debug("[{swElapsedMs}] Builder created", sw.ElapsedMilliseconds);

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

        Log.Debug(" [{swElapsedMs}] Kestrel created", sw.ElapsedMilliseconds);
        
        
        builder.Services.AddRouting();
        builder.Services.AddGrpc();
        
        Log.Debug(" [{swElapsedMs}] Grpc added", sw.ElapsedMilliseconds);
        

        await using var app = builder.Build();
        
        Log.Debug(" [{swElapsedMs}] App built", sw.ElapsedMilliseconds);
        

        app.MapGrpcService<GpuControlService>();
        
        Log.Debug(" [{swElapsedMs}] Service mapped", DateTime.Now,sw.ElapsedMilliseconds);
        




        try
        {
            await app.StartAsync();

            Log.Information(" [{swElapsedMs}] App started", sw.ElapsedMilliseconds);
            
            
            File.SetUnixFileMode(
                socketPath,
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
            
            Log.Information(" [{swElapsedMs}] Socket configured", sw.ElapsedMilliseconds);
            

            Log.Information(" [{swElapsedMs}] Applying startup profiles.",  sw.ElapsedMilliseconds);
            
            StartupProfiles.ApplyAllStartupProfiles();
            
            Log.Information(" [{swElapsedMs}] Daemon started successfully, ready for clients.",  sw.ElapsedMilliseconds);
            
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

