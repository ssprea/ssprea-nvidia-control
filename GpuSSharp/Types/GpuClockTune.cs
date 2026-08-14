namespace GpuSSharp.Types;

public abstract record GpuClockTune
{
    public sealed record Offset(int OffsetMhz, GpuPState PState) : GpuClockTune;

    public sealed record Overdrive(uint Percent) : GpuClockTune;

    public sealed record ClockRange(uint MinMhz, uint MaxMhz) : GpuClockTune;
}