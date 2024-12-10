namespace ssprea_nvidia_control_cli.NVML.NvmlTypes;


/// <summary>
/// GPU Utilization pair. Contains info on kernel execution time and gpu memory utilization
/// </summary>
public struct NvmlUtilization
{
    /*
     * % time over the past sample period during which one or more kernels
     * were executing on the GPU
     */
    public uint gpu;

    /* % time over the past sample period during which global (device) memory
     * was being read or written
     */
    public uint memory;
}
