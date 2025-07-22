namespace ssprea_nvidia_control.Utils;

public static class Systemd
{
    public static bool RestartSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","restart "+serviceName)?.ExitCode == 0;
    }
    
    public static bool StopSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","stop "+serviceName)?.ExitCode == 0;
    }
    
    public static bool DisableSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","disable "+serviceName)?.ExitCode == 0;
    }
    
    public static bool StartSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","start "+serviceName)?.ExitCode == 0;
    }
    
    public static bool EnableSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","enable "+serviceName)?.ExitCode == 0;
    }

    public static bool RunSystemdCommand(string args)
    {
        return General.RunSudoCliCommand("systemctl", args)?.ExitCode == 0;
    }

    public static bool IsSystemdServiceRunning(string serviceName)
    {
        var p = General.RunCliCommand("systemctl", "is-active --quiet " + serviceName);
        if (p is null) return false;
        
        return p.ExitCode == 0;
    }
}