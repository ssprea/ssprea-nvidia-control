using GpuSSharp.Libs.AmdSmi;
using GpuSSharp.Libs.AmdSmi.AmdSmiTypes;
using GpuSSharp.Libs.Nvml;
using GpuSSharp.Types;
using GpuSSharp.Types.Exceptions;

namespace GpuSSharp;

public class GpuService
{
    public List<IGpu> GpuList = new List<IGpu>();

    public bool IsNvmlInitialized { get; private set; }
    public bool IsAmdSmiInitialized { get; private set; }

    public GpuService()
    {
        if (!InitNvml() && !InitAmdSmi()) //add other initializers to check
            throw new NoSupportedGpusFoundException("No supported gpus could be found. GpuService initialization failure.");

        // if (!InitAmdRocm())
        //     InitAmdSysfs();
    }
    ~GpuService()
    {
        Shutdown();
    }

    // public bool InitAmdRocm()
    // {
    //
    //     // if (!RocmCliWrapper.IsRocmCliPresent())
    //     // {
    //     //     Console.WriteLine("ROCM Cli not found, skipping init.");
    //     //     return false;
    //     // }
    //     
    //     
    // }
    
    private bool InitNvml()
    {
        
        if (IsNvmlInitialized)
            return true;

        //check if system has nvml
        if (!NvmlWrapper.IsNvmlLibPresent()) 
        {
            Console.WriteLine("NVML lib not present, skipping NVidia init.");
            return false;
        }

        //init nvml

        try
        {

            NvmlWrapper.nvmlInit();
            NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount);

            if (deviceCount == 0)
                return false;
            
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
            return false;
        }

        return true;
    }

    private bool InitAmdSmi()
    {
        if (IsAmdSmiInitialized)
            return true;
        
        if (!AmdSmiWrapper.IsAmdSmiLibPresent()) 
        {
            Console.WriteLine("AmdSmi lib not present, skipping AMD init.");
            return false;
        }

        try
        {
            var r = AmdSmiWrapper.amdsmi_init(AmdSmiInitFlags.AMDSMI_INIT_AMD_GPUS);
            if (r != AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
            {
                Console.WriteLine("Error during AmdSmi initialization: "+r);
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("AmdSmi lib initialization failure: "+e);
            return false;
        }
        
        //here lib is initialized
        
        //get available socket handles 

        uint socketCount = 0;
        
        Console.WriteLine("get sockets 1: "+AmdSmiWrapper.amdsmi_get_socket_handles(ref socketCount, null));
        
        IntPtr[]? socketHandlesBuffer = new IntPtr[socketCount];
        
        Console.WriteLine("get sockets 2: "+AmdSmiWrapper.amdsmi_get_socket_handles(ref socketCount, socketHandlesBuffer));
        

        Console.WriteLine("sockets count: " +socketHandlesBuffer.Length);
        //get processors
        
        foreach (var handle in socketHandlesBuffer)
        {
            uint processorCount = 0;
            
            Console.WriteLine("get processors 1: "+AmdSmiWrapper.amdsmi_get_processor_handles(handle,ref processorCount, null));
            
            IntPtr[]? processorHandlesBuffer = new IntPtr[processorCount];
            
            Console.WriteLine("get processors 2: "+AmdSmiWrapper.amdsmi_get_processor_handles(handle,ref processorCount, processorHandlesBuffer));

            foreach (var procHandle in processorHandlesBuffer)
            {
                GpuList.Add(new AmdSmiGpu(procHandle));
            }
        }
        
        
        Console.WriteLine("DONE loaded "+GpuList.Count+" gpus!");
        
        return true;
    }

    // public void InitAmdSysfs()
    // {
    //     //check if system has amd gpus
    //     var amdGpus= SysfsWrapper.GetAllGpus();
    //
    //     if (amdGpus.Count <= 0)
    //     {
    //         Console.WriteLine("No AMD GPUs detected");
    //     }
    //     
    //     Console.WriteLine($"Found {amdGpus.Count} AMD GPUs");
    //     GpuList.AddRange(amdGpus);
    //     
    // }
    
    public void Shutdown()
    {
        GpuList.Clear();
        if (IsNvmlInitialized)
            NvmlWrapper.nvmlShutdown();
        
        if (IsAmdSmiInitialized)
            AmdSmiWrapper.amdsmi_shut_down();
        
        Console.WriteLine("GpuService destroyed");
    }

   
}