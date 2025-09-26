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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="hwmonPath"></param>
    /// <returns>Current gpu power usage in mW</returns>
    public static uint GetGpuPowerUsage(string hwmonPath)
    {
        var valuePath = hwmonPath + "/power1_average";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint power1))
            return power1/1000;
        return 0;
    }

    public static uint GetGpuPowerLimit(string hwmonPath)
    {
        var valuePath = hwmonPath + "/power1_cap";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint power1))
            return power1/1000;
        return 0;
    }

    public static void SetGpuClock(string devPath,string clockType, string pstate, string coreClockMhz)
    {
        var valuePath = devPath + "/pp_od_clk_voltage";
        if (File.Exists(valuePath))
        {
            Console.WriteLine($"writing: s {clockType} {pstate} {coreClockMhz}");
            File.WriteAllText(valuePath,$"s {clockType} {pstate} {coreClockMhz}\n");
        }
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="devPath"></param>
    /// <returns>Two uint tuples, first one is minCoreClock,maxCoreClock and second one is minMemClock,maxMemClock</returns>
    public static ((uint, uint), (uint, uint)) GetClockLimits(string devPath)
    {
        var valuePath = devPath + "/pp_od_clk_voltage";
        if (File.Exists(valuePath))
        {
            var rawLines =  File.ReadAllLines(valuePath);

            (uint,uint) coreRangeTuple = (0,0);
            (uint,uint) memRangeTuple = (0,0);
            
            foreach (var rawLine in rawLines)
            {
                if (rawLine.StartsWith("SCLK:"))
                {
                    var rawSValues = rawLine.Split(':')[1].ToLower().Split("mhz");
                    
                    if (uint.TryParse(rawSValues[0], out var sclkMin) && uint.TryParse(rawSValues[1], out var sclkMax))
                        coreRangeTuple = (sclkMin, sclkMax);
                }
                else if (rawLine.StartsWith("MCLK:"))
                {
                    var rawMValues = rawLine.Split(':')[1].ToLower().Split("mhz");
                    
                    if (uint.TryParse(rawMValues[0], out var mclkMin) && uint.TryParse(rawMValues[1], out var mclkMax))
                        memRangeTuple = (mclkMin, mclkMax);
                }
            }
            
            return (coreRangeTuple, memRangeTuple);
        }

        return ((0, 0), (0, 0));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="devPath"></param>
    /// <param name="pState"></param>
    /// <returns>core clock target, mem clock target</returns>
    public static (uint, uint)? GetCurrentLevelClocks(string devPath, uint pState)
    {
        var valuePath = devPath + "/pp_od_clk_voltage";
        
        if (File.Exists(valuePath))
        {
            var rawSclkLines =  File.ReadAllLines(valuePath).TakeWhile(line => line.Trim() != "OD_MCLK:").ToList();
            var rawMclkLines =  File.ReadAllLines(valuePath).TakeWhile(line => line.Trim() != "OD_VDDGFX_OFFSET:").ToList();

            rawMclkLines.RemoveRange(0,rawSclkLines.Count());
            
            if (uint.TryParse(rawSclkLines.First(x => x.StartsWith($"{pState.ToString()}:")).Split(":")[1].Trim().ToLower().Replace("mhz",""), out var curCoreClockTarget ) && 
                uint.TryParse(rawMclkLines.First(x => x.StartsWith($"{pState.ToString()}:")).Split(":")[1].Trim().ToLower().Replace("mhz",""), out var curMemClockTarget))
                return (curCoreClockTarget, curMemClockTarget);

        }
        return null;
    }
    
    public static void ApplyGpuClockSettings(string devPath)
    {
        var valuePath = devPath + "/pp_od_clk_voltage";
        if (File.Exists(valuePath))
        {
            File.WriteAllText(valuePath,"c\n");
        }
    }

    public static void ResetOcSettings(string devPath)
    {
        var valuePath = devPath + "/pp_od_clk_voltage";
        if (File.Exists(valuePath))
            File.WriteAllText(valuePath,"r\n");
    }
    
    public static bool SetGpuPowerLimit(string hwmonPath, uint newPlmW)
    {
        var valuePath = hwmonPath + "/power1_cap";
        if (File.Exists(valuePath))
            try
            {
                File.WriteAllText(valuePath, (newPlmW*1000).ToString());
                return true;
            }
            catch
            {
                return false;
            }

        return false;
    }
    
    public static string GetGpuFanMode(string hwmonPath)
    {
        var valuePath = hwmonPath + "/pwm1_enable";
        if (File.Exists(valuePath))
            return File.ReadAllText(valuePath);
        return "unknown";
    }
    
    public static bool SetGpuFanSpeed(string hwmonPath, uint fanSpeedPercent)
    {
        if (fanSpeedPercent > 100) fanSpeedPercent = 100;
        
        var valuePath = hwmonPath + "/pwm1";
        if (File.Exists(valuePath))
            try
            {
                File.WriteAllText(valuePath, fanSpeedPercent.ToString());
                return true;
            }
            catch
            {
                return false;
            }

        return false;
    }
    
    public static bool SetGpuFanMode(string hwmonPath, string mode)
    {
        if (mode != "automatic" || mode != "manual")
            return false;
        
        var valuePath = hwmonPath + "/pwm1_enable";
        if (File.Exists(valuePath))
            try
            {
                File.WriteAllText(valuePath, mode);
                return true;
            }
            catch
            {
                return false;
            }

        return false;
    }
    
    public static uint GetGpuPowerLimitMax(string hwmonPath)
    {
        var valuePath = hwmonPath + "/power1_cap_max";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint power1))
            return power1/1000;
        return 0;
    }
    
    public static uint GetGpuPowerLimitMin(string hwmonPath)
    {
        var valuePath = hwmonPath + "/power1_cap_min";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint power1))
            return power1/1000;
        return 0;
    }
    
    public static uint GetFanSpeedPercent(string hwmonPath)
    {
        var valuePath = hwmonPath + "/pwm1";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint pwm1))
            return pwm1;
        return 0;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hwmonPath"></param>
    /// <returns>The current GPU temperature in degrees celsius</returns>
    public static double GetGpuTemperature(string hwmonPath)
    {
        if (File.Exists(hwmonPath+"/temp1_input") && double.TryParse(File.ReadAllText(hwmonPath+"/temp1_input"), out double temp1))
            return temp1/1000;
        return 0;
    }

    public static uint GetGpuCoreUtilizationPercent(string devPath)
    {
        if (!File.Exists(devPath + "/gpu_busy_percent"))
            return 0;
        
        if (uint.TryParse(File.ReadAllText(devPath+"/gpu_busy_percent"),out var usedBytes))  
            return usedBytes;
        return 0;
    }

    public static uint GetMemCtrlUtilizationPercent(string devPath)
    {
        var valuePath = devPath + "/mem_busy_percent";
        if (File.Exists(valuePath)&& uint.TryParse(File.ReadAllText(valuePath), out uint membusy))
            return membusy;
        return 0;
    }
    
    
    // public static uint GetGpuCoreClock(string hwmonPath)
    // {
    //     if (File.Exists(hwmonPath+"/freq1_input")&& uint.TryParse(File.ReadAllText(hwmonPath+"/freq1_input"), out uint freq1))
    //         return freq1;
    //     return 0;
    // }
    //
    // public static uint GetGpuMemClock(string hwmonPath)
    // {
    //     if (File.Exists(hwmonPath+"/freq2_input")&& uint.TryParse(File.ReadAllText(hwmonPath+"/freq2_input"), out uint freq2))
    //         return freq2;
    //     return 0;
    // }
    public static uint GetGpuCoreClock(string devPath)
    {
        var readClock = File.ReadAllLines(devPath+"/pp_dpm_sclk").ToList().First(x => x.Contains('*')).Split(':')[1].Replace("*","").Trim().Split('M')[0];
        
        if (uint.TryParse(readClock, out uint finalClock))
            return finalClock;
        return 0;
    }
    
    public static uint GetGpuMemClock(string devPath)
    {
        var readClock = File.ReadAllLines(devPath+"/pp_dpm_mclk").ToList().First(x => x.Contains('*')).Split(':')[1].Replace("*","").Trim().Split('M')[0];
        
        if (uint.TryParse(readClock, out uint finalClock))
            return finalClock;
        return 0;
    }

    public static ulong GetMemoryUsedBytes(string devPath)
    {
        var valuePath = devPath+"/mem_info_vram_used";
        
        if (File.Exists(valuePath) && ulong.TryParse(File.ReadAllText(valuePath).Trim(),out var usedBytes))  
            return usedBytes;
        return 0;
    }
    
    public static ulong GetMemoryTotalBytes(string devPath)
    {
        var valuePath = devPath+"/mem_info_vram_total";
        
        if (File.Exists(valuePath) && ulong.TryParse(File.ReadAllText(valuePath).Trim(),out var usedBytes))
            return usedBytes;
        return 0;
    }
    
    public static string ReadSysfsValue(string devPath, string valuePath)
    {
        return File.ReadAllText(devPath + "/" + valuePath);
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
        return gpuName.Trim();
    }

   
}