using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.ViewModels;

namespace ssprea_nvidia_control.Models;

public class NvmlService
{
    List<NvmlGpu> _gpuList = new();
    //List<NvmlGpuVM> _gpuListVm = new();

    CancellationTokenSource _cts = new();
    
    public IReadOnlyList<NvmlGpu> GpuList => _gpuList;
    //public IReadOnlyList<NvmlGpuVM> GpuListVm => _gpuListVm;

    public NvmlService()
    {
        Initialize();   
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
        Console.WriteLine("NvmlInit: " + NvmlWrapper.nvmlInit());

        Console.WriteLine("NvmlDeviceGetCount: "+NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));

        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new NvmlGpu(i);
            _gpuList.Add(g);
            //_gpuListVm.Add(new NvmlGpuVM(g));
        }

        StartFanCurveUpdaterThread();
        
        Console.WriteLine("NvmlService initialized");
    }

    private void StartFanCurveUpdaterThread()
    {
        var t = new Thread(() =>
        {
            while (_cts.Token.IsCancellationRequested == false)
            {
                foreach (var g in GpuList)
                {
                    if (g.AppliedFanCurve !=null)
                        g.ApplySpeedToAllFans(g.AppliedFanCurve.GpuTempToFanSpeedMap[g.GpuTemperature]);
                }
            }

        });
    }
    

    
    ~NvmlService()
    {
        Shutdown();
    }
    
    
}