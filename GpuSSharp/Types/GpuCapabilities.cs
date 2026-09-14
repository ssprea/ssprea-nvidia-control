namespace GpuSSharp.Types;

public record GpuCapabilities(
    
    GpuClockTuningMode CoreClockTuningMode,
    GpuClockTuningMode MemoryClockTuningMode,
    bool GpuVoltageOffset,
    bool MemoryVoltageOffset,
    
    bool PowerLimit,
    bool FanControl
);