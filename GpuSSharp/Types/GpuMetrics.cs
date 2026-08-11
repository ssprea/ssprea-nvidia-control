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
    
    //PSTATE
    GpuPState GpuPState,
    
    //FANS
    GpuFansMetrics FansSpeedPercent
);