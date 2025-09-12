using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GpuSSharp.Libs.AmdOpenSysfs;

public static class SysfsWrapper
{
    public static List<AmdSysfsGpu> GetAllGpus()
    {
        var cardRegex = new Regex("^card[0-9]$");
        var foundGpus = new List<AmdSysfsGpu>();
        foreach (var cardPath in Directory.GetDirectories("/sys/class/drm/"))
        {
            var card = Path.GetFileName(cardPath);
            if (!cardRegex.IsMatch(card))
                continue;

            Console.Write("found possible card device: "+cardPath);
            var vendorPath = cardPath + "/device/vendor";

            if (!File.Exists(vendorPath)) //check if gpu is amd
                continue;
            var vendor = File.ReadAllText(vendorPath).Trim();
            if (vendor != "0x1002") continue;
            
            foundGpus.Add(new AmdSysfsGpu(card));
        }
        return foundGpus;
    }

    public static string DrmPathToPciAddress(string drmPath)
    {
        var p = new Process();
        p.StartInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = $"-c \"readlink {drmPath}\"", RedirectStandardOutput = true};
        Console.WriteLine($"executing: {p.StartInfo.FileName} {p.StartInfo.Arguments}");
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var pciAddress = Path.GetFileName(result);
        return pciAddress;
    }

    public static string GetGpuName(string vendorId, string deviceId)
    {
        var p = new Process();
        p.StartInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = $"-c \"lspci -d {vendorId}:{deviceId}\"", RedirectStandardOutput = true};
        Console.WriteLine($"executing: {p.StartInfo.FileName} {p.StartInfo.Arguments}");
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var gpuName = result.Split(":")[2].Replace("Advanced Micro Devices, Inc. [AMD/ATI]", "");
        return gpuName;
    }
}