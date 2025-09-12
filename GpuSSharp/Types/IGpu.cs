using GpuSSharp.Libs.Nvml;
using GpuSSharp.Libs.Nvml.NvmlTypes;

namespace GpuSSharp.Types;

public interface IGpu
{
    public uint DeviceIndex { get; }
    public string DevicePciAddress { get; }
    public string Name { get; }
    public GpuVendor Vendor { get; }
    
    public uint GpuTemperature {get;}
    public uint GpuPowerUsage {get;}

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
    public double PowerLimitMinW => PowerLimitCurrentMw / 1000f;
    public double PowerLimitMaxW => PowerLimitCurrentMw / 1000f;
    public double PowerLimitDefaultW => PowerLimitCurrentMw / 1000f;
    
    public ulong MemoryTotal {get;}
    public ulong MemoryFree {get;}
    public ulong MemoryUsed {get;}

    public double MemoryTotalMB {get;}
    public double MemoryFreeMB {get;}
    public double MemoryUsedMB {get;}

    
    
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