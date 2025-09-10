using System;
using System.Collections.ObjectModel;
using System.Threading;
using GpuSSharp;
using GpuSSharp.Libs.Nvml;

namespace ssprea_nvidia_control.Models;

public class GpuService
{
    ObservableCollection<IGpu> _gpuList = new();
    //List<NvmlGpuVM> _gpuListVm = new();

    CancellationTokenSource _cts = new();
    
    public ObservableCollection<IGpu> GpuList => _gpuList;
    //public IReadOnlyList<NvmlGpuVM> GpuListVm => _gpuListVm;
    
    public bool IsInitialized { get; private set; }

    public GpuService()
    {
        //Initialize();
    }

    public void Shutdown()
    {
        _cts.Cancel();
        NvmlWrapper.nvmlShutdown();
        _gpuList.Clear();
        
        //_gpuListVm.Clear();
        Console.WriteLine("NvmlService destroyed");
    }

    public void Initialize()
    {
        if (IsInitialized)
            return;
        
        //Console.WriteLine("NvmlInit: " + NvmlWrapper.nvmlInit());

        
        
        // Console.WriteLine("NvmlDeviceGetCount: "+NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));


        NvmlWrapper.nvmlInit();
        NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount);
        
        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new GpuNvidia(new NvmlGpu(i),TimeSpan.FromMilliseconds(500));
            _gpuList.Add(g);
        }

        
        IsInitialized = true;
        Console.WriteLine("NvmlService initialized");
        
        //TODO: initialize amd gpus
    }

    
    
    ~GpuService()
    {
        Shutdown();
    }
    
    
}