namespace GpuSSharp.Types;

public record GpuMetrics
(
    //CLOCKS
    uint GpuClockCurrent,
    uint MemClockCurrent,
    uint SmClockCurrent,
    uint VideoClockCurrent,
    
    
    //POWER
    uint PowerLimitCurrentMilliW,
    uint GpuPowerUsageMilliW,
    
    //MEMORY
    ulong MemoryFreeB,
    ulong MemoryUsedB,
    ulong MemoryTotalB,
    
    //UTILIZATION
    uint UtilizationCore,
    uint UtilizationMemCtl,
    
    //TEMPERATURE
    double GpuTemperature,
    double GpuTemperatureHotspot,
    
    //PSTATE
    GpuPState GpuPState,
    
    //FANS
    GpuFansMetrics FansSpeedPercent,
    
    //OVERCLOCK
    
    //in offset mode this is applied offset, in range mode it's applied max clock and in overdrive mode it's applied overdrive
    uint AppliedCoreOverclockMhz,
    uint AppliedMemoryOverclockMhz,
    uint AppliedCoreMinClockMhz,
    uint AppliedMemoryMinClockMhz,
    
    //VOLTAGE
    int AppliedCoreVoltageOffsetMv,
    int AppliedMemoryVoltageOffsetMv
    
);