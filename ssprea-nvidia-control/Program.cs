using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // 
    [STAThread]
    public static void Main(string[] args) =>  BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    
    
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
        RunSudoCliCommand(Program.FanCurveProcess.Id.ToString(),"kill");
    }
    
    private static Process RunSudoCliCommand(string args, string file,bool waitForExit = true)
    {
        if (SudoPasswordManager.CurrentPassword?.Password == null || SudoPasswordManager.CurrentPassword.IsExpired)
        {
            throw new SudoPasswordExpiredException("Sudo password is expired");
        }
            
            
            
            
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" "+args+"\"";
        psi.RedirectStandardInput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
        var process = Process.Start(psi);
            
            
        process.StandardInput.Write(SudoPasswordManager.CurrentPassword.Password+"\n");
        if (waitForExit)
            process.WaitForExit();

        Console.WriteLine(process.Id);
        //var output = process.StandardOutput.ReadToEnd();
            
        return process;
    }
}