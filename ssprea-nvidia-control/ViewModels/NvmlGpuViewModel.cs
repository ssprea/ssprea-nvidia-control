using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control.ViewModels;

public class NvmlGpuViewModel : ViewModelBase
{
    private readonly NvmlGpu _nvmlGpu;

    public NvmlGpuViewModel(NvmlGpu nvmlGpu)
    {
        _nvmlGpu = nvmlGpu;
    }
    
    public uint GpuTemperature { private set; get; }
    public uint GpuPowerUsage { private set; get; }
    
    
}