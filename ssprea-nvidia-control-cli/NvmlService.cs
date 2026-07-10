using Serilog;
using Serilog.Core;
using ssprea_nvidia_control_cli.NVML;

namespace ssprea_nvidia_control_cli;

public class NvmlService
{
    List<NvmlGpu> _gpuList = new();

    
    public IReadOnlyList<NvmlGpu> GpuList => _gpuList;

    public NvmlService()
    {
        Initialize();   
    }

    public void Shutdown()
    {
        NvmlWrapper.nvmlShutdown();
        _gpuList.Clear();
        
        Log.Debug("NvmlService destroyed");
    }

    public void Initialize()
    {
        Log.Debug("NvmlInit: " + NvmlWrapper.nvmlInit());

        Log.Debug("NvmlDeviceGetCount: "+NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));

        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new NvmlGpu(i);
            _gpuList.Add(g);
        }

        
        Log.Information("NvmlService initialized");
    }

 
    

    
    ~NvmlService()
    {
        Shutdown();
    }
    
    
}