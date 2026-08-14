namespace GpuSSharp.Types;

public record GpuCapabilities(
    
    GpuClockTuningMode CoreClockTuningMode,
    GpuClockTuningMode MemoryClockTuningMode,
    
    bool PowerLimit,
    bool FanControl
);