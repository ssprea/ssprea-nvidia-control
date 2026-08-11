namespace GpuSSharp.Types;

public enum GpuClockType
{
    GPU_CLOCK_CORE  = 0,        //!< Graphics clock domain
    GPU_CLOCK_SM        = 1,        //!< SM clock domain
    GPU_CLOCK_MEM       = 2,        //!< Memory clock domain
    GPU_CLOCK_VIDEO     = 3,        //!< Video encoder/decoder clock domain

    // Keep this last
    GPU_CLOCK_COUNT //!< Count of clock types
}