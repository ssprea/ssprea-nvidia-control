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
    
    public static bool StartSystemdService(string serviceName)
    {
        return General.RunSudoCliCommand("systemctl","start "+serviceName)?.ExitCode == 0;
    }
}