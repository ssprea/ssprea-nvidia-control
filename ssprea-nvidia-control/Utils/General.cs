using System;
using System.Diagnostics;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Utils;

public static class General
{
    public static Process? RunSudoCliCommand(string file, string args, bool waitForExit = true,bool redirectStdin = true,bool redirectStdout = false)
    {
        if (SudoPasswordManager.CurrentPassword is not null && SudoPasswordManager.CurrentPassword.OperationCanceled)
        {
            SudoPasswordManager.CurrentPassword = null;
            return null;
        }
            
        if (SudoPasswordManager.CurrentPassword?.Password == null || SudoPasswordManager.CurrentPassword.IsExpired || !SudoPasswordManager.CurrentPassword.IsValid )
        {
            throw new SudoPasswordExpiredException("Sudo password is expired or invalid");
        }
            
            
            
            
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" "+args+"\"";
        psi.RedirectStandardInput = redirectStdin;
        psi.RedirectStandardOutput = redirectStdout;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
        var process = Process.Start(psi);
            
            
        process.StandardInput.Write(SudoPasswordManager.CurrentPassword.Password+"\n");
        if (waitForExit)
        {
            if (!process.WaitForExit(4000))
                return null;
        }

        Console.WriteLine(process.Id);
        //var output = process.StandardOutput.ReadToEnd();
            
        return process;
    }
    
    public static Process? RunCliCommand(string file, string args, bool waitForExit = true,bool redirectStdin = true,bool redirectStdout = false)
    {
#if LINUX
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \""+file+" "+args+"\"";
        psi.RedirectStandardInput = redirectStdin;
        psi.RedirectStandardOutput = redirectStdout;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
#elif WINDOWS

        var psi = new ProcessStartInfo();
        psi.FileName = file;
        psi.Arguments = args;
        psi.RedirectStandardInput = redirectStdin;
        psi.RedirectStandardOutput = redirectStdout;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
#endif
        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);

        try
        {
            var process = Process.Start(psi);
            if (waitForExit && process is not null)
            {
                if (!process.WaitForExit(4000))
                    return null;
            }

            Console.WriteLine(process.Id);
            //var output = process.StandardOutput.ReadToEnd();
            
            return process;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsCapsLockEnabled()
    {
#if WINDOWS
        return false; //TODO: windows support
#elif LINUX
        return bool.Parse((RunCliCommand("xset -q | sed -n 's/^.*Caps Lock:\\s*\\(\\S*\\).*$/\\1/p'", "",redirectStdin:false, redirectStdout: true)?.StandardOutput.ReadToEnd() ?? "off").Replace("off","false").Replace("on","true") );
#endif
    }
}