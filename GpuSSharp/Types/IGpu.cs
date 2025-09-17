using GpuSSharp.Libs.Nvml;
using GpuSSharp.Libs.Nvml.NvmlTypes;

namespace GpuSSharp.Types;

public interface IGpu
{
    /// <summary>
    /// nvml index on NVidia, drm card n on amd
    /// </summary>
    public uint DeviceIndex { get; }
    public string DevicePciAddress { get; }
    public string Name { get; }
    public GpuVendor Vendor { get; }
    
    public double GpuTemperature {get;}
    public uint GpuPowerUsage {get;}
    public double GpuPowerUsageW => GpuPowerUsage / 1000f;

    public GpuPState GpuPState {get;}

    public uint GpuClockCurrent {get;}
    public uint MemClockCurrent {get;}
    public uint SmClockCurrent {get;}
    public uint VideoClockCurrent {get;}
    
    
    public uint PowerLimitCurrentMw {get;}
    public uint PowerLimitMinMw {get;}
    public uint PowerLimitMaxMw {get;}
    public uint PowerLimitDefaultMw {get;}

    public double PowerLimitCurrentW => PowerLimitCurrentMw / 1000f;
    public double PowerLimitMinW => PowerLimitMinMw / 1000f;
    public double PowerLimitMaxW => PowerLimitMaxMw / 1000f;
    public double PowerLimitDefaultW => PowerLimitDefaultMw / 1000f;
    
    public ulong MemoryTotal {get;}
    public ulong MemoryFree {get;}
    public ulong MemoryUsed {get;}

    public double MemoryTotalMB => MemoryTotal / 1000000f;
    public double MemoryFreeMB => MemoryFree / 1000000f;
    public double MemoryUsedMB => MemoryUsed / 1000000f;
    
    
    public uint UtilizationCore {get;}
    public uint UtilizationMemCtl {get;}
    

    

    public uint TemperatureThresholdShutdown {get;}
    public uint TemperatureThresholdSlowdown {get;}
    public uint TemperatureThresholdThrottle {get;}
    
    public uint Fan0SpeedPercent { get; }

    public bool SetCoreOffset(GpuPState pState, int clockOffsetMhz);
    public bool SetMemOffset(GpuPState pState, int clockOffsetMhz);
    public bool SetGpuPowerLimit(uint limitMw);

    public bool ApplySpeedToAllFans(uint speed);
    public bool ApplyAutoSpeedToAllFans();
    
    public List<uint> GetFansIds();
}