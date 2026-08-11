using GpuSSharp.Libs.Nvml;
using GpuSSharp.Libs.Nvml.NvmlTypes;

namespace GpuSSharp.Types;

/// <summary>
/// Represents a GPU of any vendor, contains static GPU information.
/// </summary>
public interface IGpu
{
    //BASIC GPU INFO
    
    /// <summary>
    /// nvml index on NVidia, drm card n on amd
    /// </summary>
    public uint DeviceIndex { get; }
    public string DevicePciAddress { get; }
    public string Name { get; }
    public GpuVendor Vendor { get; }
    
    //GPU METRICS
    public GpuMetrics GetMetrics();
    
    //POWER LIMIT
    
    public uint PowerLimitMinMw {get;}
    public uint PowerLimitMaxMw {get;}
    public uint PowerLimitDefaultMw {get;}
    

    // public double MemoryTotalMB => MemoryTotal / 1000000f;
    
    //FANS
    public uint FansCount { get; }
    
    
    //TEMPERATURE THRESHOLDS
    public uint TemperatureThresholdShutdown {get;}
    public uint TemperatureThresholdSlowdown {get;}
    public uint TemperatureThresholdThrottle {get;}
    
    //SETTERS
    
    public bool SetCoreOffset(GpuPState pState, int clockOffsetMhz);
    public bool SetMemOffset(GpuPState pState, int clockOffsetMhz);
    public bool SetGpuPowerLimit(uint limitMw);

    public bool ApplySpeedToAllFans(uint speed);
    public bool ApplyAutoSpeedToAllFans();
    
    
    // public double GpuTemperature {get;}
    // public uint GpuPowerUsage {get;}
    // public double GpuPowerUsageW => GpuPowerUsage / 1000f;

    // public GpuPState GpuPState {get;}

    // public uint GpuClockCurrent {get;}
    // public uint MemClockCurrent {get;}
    // public uint SmClockCurrent {get;}
    // public uint VideoClockCurrent {get;}
    //
    //
    // public uint PowerLimitCurrentMw {get;}


    // public double PowerLimitCurrentW => PowerLimitCurrentMw / 1000f;

    
    // public ulong MemoryFree {get;}
    // public ulong MemoryUsed {get;}

    // public double MemoryFreeMB => MemoryFree / 1000000f;
    // public double MemoryUsedMB => MemoryUsed / 1000000f;
    
    
    // public uint UtilizationCore {get;}
    // public uint UtilizationMemCtl {get;}
    

    


    
    // public uint Fan0SpeedPercent { get; }


}