namespace GpuSSharp.Libs.Nvml.NvmlTypes;

public struct NvmlMemory
{
    public ulong Total;        //!< Total physical device memory (in bytes)
    public ulong Free;         //!< Unallocated device memory (in bytes)
    public ulong Used;         //!< Sum of Reserved and Allocated device memory (in bytes).
    //!< Note that the driver/GPU always sets aside a small amount of memory for bookkeeping
}