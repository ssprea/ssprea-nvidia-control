using ssprea_nvidia_control_cli.NVML;

namespace ssprea_nvidia_control_cli;

public class NvmlService
{
    List<NvmlGpu> _gpuList = new();
    //List<NvmlGpuVM> _gpuListVm = new();

    
    public IReadOnlyList<NvmlGpu> GpuList => _gpuList;
    //public IReadOnlyList<NvmlGpuVM> GpuListVm => _gpuListVm;

    public NvmlService()
    {
        Initialize();   
    }

    public void Shutdown()
    {
        NvmlWrapper.nvmlShutdown();
        _gpuList.Clear();
        
        //_gpuListVm.Clear();
        Console.WriteLine("NvmlService destroyed");
    }

    public void Initialize()
    {
        Console.WriteLine("NvmlInit: " + NvmlWrapper.nvmlInit());

        Console.WriteLine("NvmlDeviceGetCount: "+NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));

        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new NvmlGpu(i);
            _gpuList.Add(g);
            //_gpuListVm.Add(new NvmlGpuVM(g));
        }

        
        Console.WriteLine("NvmlService initialized");
    }

 
    

    
    ~NvmlService()
    {
        Shutdown();
    }
    
    
}