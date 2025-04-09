using System;
using System.Diagnostics;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Utils;

public static class General
{
    public static Process? RunSudoCliCommand(string file, string args, bool waitForExit = true)
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
        psi.RedirectStandardInput = true;
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
}