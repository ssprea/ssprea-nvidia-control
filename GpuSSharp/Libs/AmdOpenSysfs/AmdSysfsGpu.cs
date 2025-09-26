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
    
    public uint MaxCoreClockValue {private set; get; }
    public uint MinCoreClockValue {private set; get; }
    public uint MaxMemClockValue {private set; get; }
    public uint MinMemClockValue {private set; get; }
    

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

        var maxClocks = SysfsWrapper.GetClockLimits(DevPath);
        
        MinCoreClockValue = maxClocks.Item1.Item1;
        MaxCoreClockValue = maxClocks.Item1.Item2;

        MinMemClockValue = maxClocks.Item2.Item1;
        MaxMemClockValue = maxClocks.Item2.Item2;

    }


    public (string,string) GetVendorAndCardId()
    {
        if (File.Exists(DevPath+"/vendor") &&  File.Exists(DevPath+"/device"))
            return (File.ReadAllText(DevPath+"/vendor").Trim(),File.ReadAllText(DevPath+"/device").Trim());
        return ("", "");
    }


    
    

    private bool SetClockOffset(GpuPState pState, GpuClockType clockType, int clockOffsetMhz)
    {
        //reset clocks
        SysfsWrapper.ResetOcSettings(DevPath);
        
        
        //get current clock targets
        var clockTargetsBase = SysfsWrapper.GetCurrentLevelClocks(DevPath,1);
        if (clockTargetsBase is null)
            return false;
        
        
        //sum current clock target with offset
        var coreOffsetClock = clockTargetsBase.Value.Item1 + clockOffsetMhz;
        var memOffsetClock = clockTargetsBase.Value.Item2 + clockOffsetMhz;
        
        if (coreOffsetClock > MaxCoreClockValue)
            coreOffsetClock = MaxCoreClockValue;
        
        if (memOffsetClock > MaxMemClockValue)
            memOffsetClock = MaxMemClockValue;
        
        
        if (coreOffsetClock < MinCoreClockValue)
            coreOffsetClock = MinCoreClockValue;
        
        if (memOffsetClock < MinMemClockValue)
            memOffsetClock = MinMemClockValue;
        
        //set and apply offset clock

        switch (clockType)
        {
            case GpuClockType.GPU_CLOCK_CORE:
                SysfsWrapper.SetGpuClock(DevPath,"s","1",coreOffsetClock.ToString());
                break;
            case GpuClockType.GPU_CLOCK_MEM:
                SysfsWrapper.SetGpuClock(DevPath,"m","1",memOffsetClock.ToString());
                break;
        }
        SysfsWrapper.ApplyGpuClockSettings(DevPath);
        return true;
    }
    
    public bool SetCoreOffset(GpuPState pState, int clockOffsetMhz)
    {
        return SetClockOffset(pState,GpuClockType.GPU_CLOCK_CORE,clockOffsetMhz);
    }

    public bool SetMemOffset(GpuPState pState, int clockOffsetMhz)
    {
        return SetClockOffset(pState,GpuClockType.GPU_CLOCK_MEM,clockOffsetMhz);
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