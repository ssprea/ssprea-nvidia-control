using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Models.Exceptions;
using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control;

sealed class Program
{
    
    public static string DefaultDataPath =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.snvctl-gui";

    public static Process? FanCurveProcess = null;

    public static string SelectedLocale = "System";
    
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // 
    [STAThread]
    public static void Main(string[] args)
    {
        SelectedLocale = File.Exists(DefaultDataPath+"/SelectedLocale.txt") ? File.ReadAllText(DefaultDataPath+"/SelectedLocale.txt").Trim() : "System";

        if (SelectedLocale != "System")
        {
            Lang.Resources.Culture = new CultureInfo(SelectedLocale);
        }

        var maximizeThread = new Thread(async () =>
        {
            
        });
        
        Task.Run(async Task?() =>
        {
            while (true)
            {
                await Utils.Lockfile.WaitForMaximizeMessageAsync(CancellationToken.None);
                
                Console.WriteLine("Maximize command received!");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var win = WindowsManager.AllWindows.FirstOrDefault(x => x.Name == "MainOcWindow");
                    win?.Show();
                });
                
            }
            
        });
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        
        
    }

   
    
    
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    
    
    public static void KillFanCurveProcess()
    {
        if (Program.FanCurveProcess is null || Program.FanCurveProcess.HasExited)
        {
            Console.WriteLine("Fan curve process not running");
            return;
        }

        Utils.General.RunSudoCliCommand("kill", Program.FanCurveProcess.Id.ToString());
    }

    
}