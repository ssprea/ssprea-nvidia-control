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
using Serilog;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Models.Exceptions;
using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control;

sealed class Program
{
    
    public static string DefaultDataPath =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.snvctl-gui";

    public static string SettingsFilePath = DefaultDataPath + "/settings.json";
    
    
    public static Process? FanCurveProcess = null;

    //public static string SelectedLocale = "System";
    public static Settings LoadedSettings = Settings.Default();
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // 
    [STAThread]
    public static void Main(string[] args)
    {
        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = log;
        
        // SelectedLocale = File.Exists(DefaultDataPath+"/SelectedLocale.txt") ? File.ReadAllText(DefaultDataPath+"/SelectedLocale.txt").Trim() : "System";
        CheckAndConvertLegacySettings();
        CheckAndLoadSettings();
        
        if (LoadedSettings.SelectedLocale != "System")
        {
            Lang.Resources.Culture = new CultureInfo(LoadedSettings.SelectedLocale);
        }
        
        
        Task.Run(async Task?() =>
        {
            while (true)
            {
                await Utils.Lockfile.WaitForMaximizeMessageAsync(CancellationToken.None);
                
                Log.Debug("Maximize command received!");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var win = WindowsManager.AllWindows.FirstOrDefault(x => x.Name == "MainOcWindow");
                    win?.Show();
                });
                
            }
            
        });
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        
        
    }

    private static void CheckAndLoadSettings()
    { 

        if (!File.Exists(SettingsFilePath))
        {
            Log.Warning("Settings file not found. Generating default.");
            File.WriteAllText(SettingsFilePath, Settings.Default().ToJson());
        }

        var parsedSettings = Settings.FromJson(File.ReadAllText(SettingsFilePath));
        if (parsedSettings is null)
        {
            Log.Warning("Error while reading settings file, loading default. ");
            return;
        }
        
        LoadedSettings = parsedSettings;
    }

    private static void CheckAndConvertLegacySettings()
    {

        if (File.Exists(SettingsFilePath))
        {
            Log.Debug("New settings format found, skipping legacy settings conversion.");
            return;
        }
        
        Directory.CreateDirectory(DefaultDataPath + "/backup");
        var defaultSettings = Settings.Default();

        if (File.Exists(DefaultDataPath + "/SelectedLocale.txt"))
        {
            defaultSettings.SelectedLocale = File.ReadAllText(DefaultDataPath + "/SelectedLocale.txt").Trim();
            File.Move(DefaultDataPath + "/SelectedLocale.txt",DefaultDataPath + "/backup/SelectedLocale.txt");
        }

        if (File.Exists(DefaultDataPath + "/SelectedGui.txt"))
        {
            defaultSettings.SelectedGui = File.ReadAllText(DefaultDataPath + "/SelectedGui.txt").Trim();
            File.Move(DefaultDataPath + "/SelectedGui.txt",DefaultDataPath + "/backup/SelectedGui.txt");
            
        }
        
        File.WriteAllText(SettingsFilePath, defaultSettings.ToJson());
        Log.Information("Settings converted succesfully!");
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
            Log.Debug("Fan curve process not running");
            return;
        }

        Utils.General.RunSudoCliCommand("kill", Program.FanCurveProcess.Id.ToString());
    }

    
}