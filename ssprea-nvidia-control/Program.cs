using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control;

sealed class Program
{
    public static string DefaultDataPath =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.snvctl-gui";

    public static Process? FanCurveProcess = null;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // 
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}