using GpuSSharp.Types;

namespace GpuSSharp.Libs.AmdOpenSysfs;

public class AmdSysfsGpu : IGpu
{
    public uint DeviceIndex => DrmId;
    public string Name { private set; get; }
    public double GpuTemperature => SysfsWrapper.GetGpuTemperature(HwmonPath);
    public uint GpuPowerUsage => SysfsWrapper.GetGpuPowerUsage(HwmonPath);
    public GpuPState GpuPState { get; } = GpuPState.GPU_PSTATE_UNKNOWN;
    public uint GpuClockCurrent => SysfsWrapper.GetGpuCoreClock(DevPath);
    public uint MemClockCurrent => SysfsWrapper.GetGpuMemClock(DevPath);
    public uint SmClockCurrent { get; } = 0;
    public uint VideoClockCurrent { get; } = 0;
    public uint PowerLimitCurrentMw => SysfsWrapper.GetGpuPowerLimit(HwmonPath);
    public uint PowerLimitMinMw => SysfsWrapper.GetGpuPowerLimitMin(HwmonPath);
    public uint PowerLimitMaxMw => SysfsWrapper.GetGpuPowerLimitMax(HwmonPath);
    public uint PowerLimitDefaultMw { get; } = 0;
    public ulong MemoryTotal => SysfsWrapper.GetMemoryTotalBytes(DevPath);
    public ulong MemoryFree => MemoryTotal - MemoryUsed;
    public ulong MemoryUsed => SysfsWrapper.GetMemoryUsedBytes(DevPath);
    public uint UtilizationCore => SysfsWrapper.GetGpuCoreUtilizationPercent(DevPath);
    public uint UtilizationMemCtl => SysfsWrapper.GetMemCtrlUtilizationPercent(DevPath);
    public uint TemperatureThresholdShutdown { get; } = 100;
    public uint TemperatureThresholdSlowdown { get; } = 100;
    public uint TemperatureThresholdThrottle { get; } = 100;
    public uint Fan0SpeedPercent => SysfsWrapper.GetFanSpeedPercent(HwmonPath);

    public GpuVendor Vendor => GpuVendor.Amd;

    public uint DrmId { private set; get; }
    public string DevicePciAddress { private set; get; }
    public string DrmPath { private set; get; }
    public string DevPath => DrmPath + "/device";
    
    public string HwmonPath { private set; get; }
    
    public string VendorId { private set; get; }
    public string CardHwId { private set; get; }

    public AmdSysfsGpu(string drmCardName)
    {
        DrmPath = "/sys/class/drm/"+drmCardName;

        DrmId = uint.Parse(drmCardName.ToCharArray().Last().ToString());
        
        DevicePciAddress = SysfsWrapper.DrmPathToPciAddress(DevPath).Trim();

        HwmonPath = Directory.GetDirectories(DevPath + "/hwmon").First(x => Path.GetFileName(x).StartsWith("hwmon"));

        var ids = GetVendorAndCardId();
        
        VendorId = ids.Item1;
        CardHwId = ids.Item2;
        
        Name = SysfsWrapper.GetGpuName(VendorId, CardHwId);
        
    }


    public (string,string) GetVendorAndCardId()
    {
        if (File.Exists(DevPath+"/vendor") &&  File.Exists(DevPath+"/device"))
            return (File.ReadAllText(DevPath+"/vendor").Trim(),File.ReadAllText(DevPath+"/device").Trim());
        return ("", "");
    }


    
    
    public bool SetCoreOffset(GpuPState pState, int clockOffsetMhz)
    {
        throw new NotImplementedException();
    }

    public bool SetMemOffset(GpuPState pState, int clockOffsetMhz)
    {
        throw new NotImplementedException();
    }

    public bool SetGpuPowerLimit(uint limitMw)
    {
        if (limitMw > PowerLimitMaxMw)
            limitMw = PowerLimitMaxMw;
        else if (limitMw < PowerLimitMinMw)
            limitMw = PowerLimitMinMw;
        
        return SysfsWrapper.SetGpuPowerLimit(HwmonPath, limitMw);
    }

    public bool ApplySpeedToAllFans(uint speed)
    {
        if (SysfsWrapper.GetGpuFanMode(HwmonPath) == "automatic")
            SysfsWrapper.SetGpuFanMode(HwmonPath, "manual");
        
        return SysfsWrapper.SetGpuFanSpeed(HwmonPath, speed);
            
    }

    public bool ApplyAutoSpeedToAllFans()
    {
        return SysfsWrapper.SetGpuFanMode(HwmonPath, "automatic");
    }

    public List<uint> GetFansIds()
    {
        throw new NotImplementedException();
    }
}