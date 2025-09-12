using GpuSSharp.Libs.AmdOpenSysfs;
using GpuSSharp.Libs.Nvml;
using GpuSSharp.Types;

namespace GpuSSharp;

public class GpuService
{
    public List<IGpu> GpuList = new List<IGpu>();

    public bool IsNvmlInitialized { get; private set; }

    public GpuService()
    {
        InitNvml();
        InitAmdSysfs();
    }
    ~GpuService()
    {
        Shutdown();
    }
    
    public void InitNvml()
    {
        
        if (IsNvmlInitialized)
            return;

        //check if system has nvml
        if (!NvmlWrapper.IsNvmlLibPresent()) 
        {
            Console.WriteLine("NVML lib not present, skipping init.");
            return;
        }

        //init nvml

        try
        {

            NvmlWrapper.nvmlInit();
            NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount);

            for (uint i = 0; i < deviceCount; i++)
            {
                var g = new NvmlGpu(i);
                GpuList.Add(g);
            }


            IsNvmlInitialized = true;
            Console.WriteLine($"NvmlService initialized, found {deviceCount} NVidia GPUs");

        }
        catch (Exception e)
        {
            Console.WriteLine("NvmlService initialization failure: "+e);
        }
        
    }

    public void InitAmdSysfs()
    {
        //check if system has amd gpus
        var amdGpus= SysfsWrapper.GetAllGpus();

        if (amdGpus.Count <= 0)
        {
            Console.WriteLine("No AMD GPUs detected");
        }
        
        Console.WriteLine($"Found {amdGpus.Count} AMD GPUs");
        GpuList.AddRange(amdGpus);
        
    }
    
    public void Shutdown()
    {
        NvmlWrapper.nvmlShutdown();
        GpuList.Clear();
        
        Console.WriteLine("GpuService destroyed");
    }

   
}