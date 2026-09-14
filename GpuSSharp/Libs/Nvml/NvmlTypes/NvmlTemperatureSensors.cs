namespace GpuSSharp.Libs.Nvml.NvmlTypes;

#pragma warning disable CA1707
public enum NvmlTemperatureSensors
{
    // Temperature sensor for the GPU die
    NVML_TEMPERATURE_GPU = 0,
    NVML_TEMPERATURE_GPU_MAX = 1,
    NVML_TEMPERATURE_COUNT
}
#pragma warning restore CA1707