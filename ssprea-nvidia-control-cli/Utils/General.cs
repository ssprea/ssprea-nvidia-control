using System;
using System.Diagnostics;

namespace ssprea_nvidia_control_cli.Utils;

public static class General
{
    public static Process? RunSudoCliCommand(string file, string args, bool waitForExit = true,bool redirectStdin = true,bool redirectStdout = false)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \"/usr/bin/sudo "+file+" "+args+"\"";
        psi.RedirectStandardInput = redirectStdin;
        psi.RedirectStandardOutput = redirectStdout;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
        var process = Process.Start(psi);
            
            
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
            
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \""+file+" "+args+"\"";
        psi.RedirectStandardInput = redirectStdin;
        psi.RedirectStandardOutput = redirectStdout;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
        var process = Process.Start(psi);
            
        
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