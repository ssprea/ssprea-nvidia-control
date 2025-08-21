using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using DynamicData;
using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control.Models;

public class NvmlService
{
    ObservableCollection<NvmlGpu> _gpuList = new();
    //List<NvmlGpuVM> _gpuListVm = new();

    CancellationTokenSource _cts = new();
    
    public ObservableCollection<NvmlGpu> GpuList => _gpuList;
    //public IReadOnlyList<NvmlGpuVM> GpuListVm => _gpuListVm;
    
    public bool IsInitialized { get; private set; }

    public NvmlService()
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
            var g = new NvmlGpu(i);
            _gpuList.Add(g);
            //_gpuListVm.Add(new NvmlGpuVM(g));
        }

        //StartFanCurveUpdaterThread();
        
        IsInitialized = true;
        Console.WriteLine("NvmlService initialized");
    }

    
    
    ~NvmlService()
    {
        Shutdown();
    }
    
    
}