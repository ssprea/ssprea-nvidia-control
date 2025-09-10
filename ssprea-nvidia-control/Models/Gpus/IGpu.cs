

using System.Collections.Generic;
using System.ComponentModel;
using GpuSSharp.Libs.Nvml.NvmlTypes;
using ssprea_nvidia_control.Models.Types;

namespace ssprea_nvidia_control.Models;

public interface IGpu : INotifyPropertyChanged
{
    public uint DeviceIndex { get; }
    public string Name { get; }
    public uint GpuTemperature {get;}
    public uint GpuPowerUsage {get;}
    public double GpuPowerUsageW {get;}
    public string GpuPowerUsageWFormatted {get;}

    public GpuPState GpuPState {get;}

    public uint GpuClockCurrent {get;}
    public uint MemClockCurrent {get;}
    public uint SmClockCurrent {get;}
    public uint VideoClockCurrent {get;}

    // public IImmutableSolidColorBrush TemperatureIndicatorColorBrush {get;}
    //     GpuTemperature < TemperatureThresholdThrottle ? Brushes.White : (GpuTemperature < TemperatureThresholdSlowdown ? Brushes.Orange : Brushes.Red  );
    
    
    public uint PowerLimitCurrentMw {get;}
    public uint PowerLimitMinMw {get;}
    public uint PowerLimitMaxMw {get;}
    public uint PowerLimitDefaultMw {get;}
    
    public double PowerLimitCurrentW {get;}
    public double PowerLimitMinW {get;}
    public double PowerLimitMaxW {get;}
    public double PowerLimitDefaultW {get;}
    
    public ulong MemoryTotal {get;}
    public ulong MemoryFree {get;}
    public ulong MemoryUsed {get;}

    public double MemoryTotalMB {get;}
    public double MemoryFreeMB {get;}
    public double MemoryUsedMB {get;}

    public string MemoryTotalMBFormatted {get;}
    public string MemoryFreeMBFormatted {get;}
    public string MemoryUsedMBFormatted {get;}
    
    
    public uint UtilizationCore {get;}
    public uint UtilizationMemCtl {get;}
    

    public string MemoryUsageString {get;}
    

    public uint TemperatureThresholdShutdown {get;}
    public uint TemperatureThresholdSlowdown {get;}
    public uint TemperatureThresholdThrottle {get;}
    
    public uint Fan0SpeedPercent { get; }

    public bool SetClockOffset(NvmlClockType clockType, NvmlPStates pState, int clockOffsetMhz);
    public bool SetPowerLimit(uint limitMw);
    public void ApplyFanCurve(FanCurve fanCurve);

    public bool ApplySpeedToAllFans(uint speed);
    public bool ApplyAutoSpeedToAllFans();
    
    public List<uint> GetFansIds();
    

}