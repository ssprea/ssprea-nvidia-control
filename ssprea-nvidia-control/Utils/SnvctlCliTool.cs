using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Serilog;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Utils;

public static class SnvctlCliTool
{
    public static Process? RunSudoCliCommand(string args,uint deviceIdx, string file="/usr/local/bin/snvctl", bool waitForExit = true)
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
        psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" -g "+deviceIdx+" "+args+"\"";
        psi.RedirectStandardInput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Log.Information("Executing: "+psi.FileName+" "+psi.Arguments);
        
        
        var process = Process.Start(psi);

        if (process is null) return null;
        
        process.StandardInput.Write(SudoPasswordManager.CurrentPassword.Password+"\n");
        if (waitForExit)
        {
            if (!process.WaitForExit(4000))
                return null;
        }

        Log.Debug("PID: "+process.Id);
        //var output = process.StandardOutput.ReadToEnd();
        
        return process;
    }

    public static void RunFanProcess(FanCurve fanCurve, uint deviceIdx)
    {
        if (Program.FanCurveProcess is not null)
            Program.FanCurveProcess.Kill();
        
        try
        {
            var tempPath = Program.DefaultDataPath + "/temp/fanCurve-" + fanCurve.Name.Replace(" ","_").Replace("/","_").Replace("\\","_").Replace(":","_") +
                           DateTime.Now.ToString("yyyyMMddHHmmss")+".json";
            File.WriteAllText(tempPath,JsonConvert.SerializeObject(fanCurve, Formatting.None));
            
            Program.FanCurveProcess = RunSudoCliCommand($"-fp {tempPath}",deviceIdx,waitForExit:false);
        }catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
}