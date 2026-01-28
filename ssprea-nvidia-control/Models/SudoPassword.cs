using System;
using System.Diagnostics;
using System.Security;
using Serilog;

namespace ssprea_nvidia_control.Models;

public class SudoPassword
{
    //public SecureString Password { get; private set; }
    public string Password { get; private set; }
    public DateTime ExpirationTime { get; private set; }
    public bool IsExpired => ExpirationTime < DateTime.Now;

    public bool OperationCanceled { get; set; } = false;
    
    public bool IsValid => ValidatePassword();

    // public void SetExpirationTime(TimeSpan timeToExpiration)
    // {
    //     ExpirationTime = DateTime.Now.Add(timeToExpiration);
    // }
    // public void SetExpirationTime(DateTime expirationTime)
    // {
    //     ExpirationTime = expirationTime;
    // }

    public SudoPassword(string password, DateTime expirationTime)
    {
        Password = password;
        ExpirationTime = expirationTime;
    }
    
    // public SudoPassword(string password,DateTime expirationTime)
    // {
    //     Password = password;
    //     ExpirationTime = expirationTime;
    // }
    
    public SudoPassword(string password,TimeSpan timeToExpiration)
    {
        Password = password;
        ExpirationTime = DateTime.Now.Add(timeToExpiration);
    }

    public SudoPassword(string password)
    {
        Password = password;
        ExpirationTime = DateTime.MaxValue;
    }

    private bool ValidatePassword()
    {

        var file = "/usr/local/bin/snvctl";
        
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" -d \"";
        psi.RedirectStandardInput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Log.Debug("Executing: "+psi.FileName+" "+psi.Arguments);
        
        
        var process = Process.Start(psi);
        
        
        process.StandardInput.Write(Password+"\n");
        
        if (!process.WaitForExit(5000))
            return false;
        

        Log.Debug(process.Id.ToString());
        //var output = process.StandardOutput.ReadToEnd();
        
        return true;
        
    }
}