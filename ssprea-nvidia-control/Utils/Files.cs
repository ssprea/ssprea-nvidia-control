namespace ssprea_nvidia_control.Utils;

public static class Files
{
    public static bool WriteAllTextSudo(string path, string text)
    {
        return General.RunSudoCliCommand("echo ", text + " > " + path)?.ExitCode == 0; 
    }

    public static bool MakeDirectorySudo(string path)
    {
        return General.RunSudoCliCommand("mkdir", path )?.ExitCode == 0;
    }
    
    public static bool CopySudo(string pathFrom, string pathTo)
    {
        return General.RunSudoCliCommand("cp ", pathFrom + " " + pathTo )?.ExitCode == 0;
    }
}