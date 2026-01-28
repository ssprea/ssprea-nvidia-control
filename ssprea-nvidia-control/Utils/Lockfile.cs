using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Serilog;

namespace ssprea_nvidia_control.Utils;

public static class Lockfile
{
    private static readonly string LockFilePath = Program.DefaultDataPath + "/.guiLock";
    
    public static bool IsAnotherInstanceRunning()
    {
        if (File.Exists(LockFilePath))
        {
            var pid = File.ReadAllText(LockFilePath);
            try
            {
                var oldProc = Process.GetProcessById(int.Parse(pid));
                return !oldProc.HasExited;
            }
            catch (Exception)
            {
                return false;
            }
        }

        return false;

    }

    public static void CheckAndUpdateLockfile()
    {
        if (Design.IsDesignMode)
            return;
        
        if (IsAnotherInstanceRunning())
        {
            SendMaximizeMessage();
            Log.Information("Another instance of this tool is running! Exiting...");
            Environment.Exit(0);
        }
        
        File.WriteAllText(Program.DefaultDataPath+"/.guiLock",Process.GetCurrentProcess().Id.ToString());
    }

    private static bool SendMaximizeMessage()
    {
        using (var c = new TcpClient())
        {
            try
            {
                c.Connect(IPAddress.Loopback, 43565);
                var sendmsg = "MAX";
                c.GetStream().Write(Encoding.UTF8.GetBytes(sendmsg));
                c.Close();
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    public static async Task WaitForMaximizeMessageAsync(CancellationToken token)
    {
        var s = new TcpListener(IPAddress.Loopback, 43565);
        s.Start();

        while (!token.IsCancellationRequested)
        {
            
            using var client = await s.AcceptTcpClientAsync(token);


            var buffer = new byte[3];
            await client.GetStream().ReadExactlyAsync(buffer,token);

            var readStr = Encoding.UTF8.GetString(buffer);
            
            if (readStr == "MAX")
                break;

        }
        
        
        s.Stop();
        s.Dispose();
    }
}