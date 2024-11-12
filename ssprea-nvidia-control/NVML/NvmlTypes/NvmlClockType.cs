namespace ssprea_nvidia_control.NVML.NvmlTypes;

public enum NvmlClockType
{
    NVML_CLOCK_GRAPHICS  = 0,        //!< Graphics clock domain
    NVML_CLOCK_SM        = 1,        //!< SM clock domain
    NVML_CLOCK_MEM       = 2,        //!< Memory clock domain
    NVML_CLOCK_VIDEO     = 3,        //!< Video encoder/decoder clock domain

    // Keep this last
    NVML_CLOCK_COUNT //!< Count of clock types
}