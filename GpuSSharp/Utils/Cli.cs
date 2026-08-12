using System.Diagnostics;

namespace GpuSSharp.Utils;

public static class Cli
{
    public static Process? ExecuteProgram(string fileName, string args, bool waitForExit = true)
    {
        var proc = new Process();
        proc.StartInfo = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput =  true,
        };

        try
        {
            if (proc.Start())
                return proc;
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns>True if exitcode success, else false</returns>
    public static bool ExecuteAndReadProgramOutput(string fileName, string args, out string programOutput)
    {
        var p = ExecuteProgram(fileName, args, true);
        if (p is null)
        {
            programOutput = "";
            return false;
        }
        
        programOutput = p.StandardOutput.ReadToEnd();
        return p.ExitCode == 0;
    }
}