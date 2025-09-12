using GpuSSharp.Types;

namespace GpuSSharp.Libs.AmdOpenSysfs;

public class AmdSysfsGpu : IGpu
{
    public uint DeviceIndex { get; }
    public string Name { private set; get; }
    public uint GpuTemperature { get; }
    public uint GpuPowerUsage { get; }
    public GpuPState GpuPState { get; }
    public uint GpuClockCurrent { get; }
    public uint MemClockCurrent { get; }
    public uint SmClockCurrent { get; }
    public uint VideoClockCurrent { get; }
    public uint PowerLimitCurrentMw { get; }
    public uint PowerLimitMinMw { get; }
    public uint PowerLimitMaxMw { get; }
    public uint PowerLimitDefaultMw { get; }
    public ulong MemoryTotal { get; }
    public ulong MemoryFree { get; }
    public ulong MemoryUsed { get; }
    public double MemoryTotalMB { get; }
    public double MemoryFreeMB { get; }
    public double MemoryUsedMB { get; }
    public uint UtilizationCore { get; }
    public uint UtilizationMemCtl { get; }
    public uint TemperatureThresholdShutdown { get; }
    public uint TemperatureThresholdSlowdown { get; }
    public uint TemperatureThresholdThrottle { get; }
    public uint Fan0SpeedPercent { get; }

    public GpuVendor Vendor => GpuVendor.Amd;

    public uint DrmId { private set; get; }
    public string DevicePciAddress { private set; get; }
    public string DrmPath { private set; get; }
    public string DevPath => DrmPath + "/device";
    
    public string VendorId { private set; get; }
    public string CardHwId { private set; get; }

    public AmdSysfsGpu(string drmCardName)
    {
        DrmPath = "/sys/class/drm/"+drmCardName;

        DrmId = uint.Parse(drmCardName.ToCharArray().Last().ToString());
        
        DevicePciAddress = SysfsWrapper.DrmPathToPciAddress(DevPath);

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
        throw new NotImplementedException();
    }

    public bool ApplySpeedToAllFans(uint speed)
    {
        throw new NotImplementedException();
    }

    public bool ApplyAutoSpeedToAllFans()
    {
        throw new NotImplementedException();
    }

    public List<uint> GetFansIds()
    {
        throw new NotImplementedException();
    }
}