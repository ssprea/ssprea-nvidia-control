using System;
using System.Collections.ObjectModel;
using System.Threading;
using GpuSSharp;
using GpuSSharp.Libs.Nvml;
using GpuSSharp.Libs.Nvml.NvmlTypes;
using GpuSSharp.Types;

namespace ssprea_nvidia_control.Models;

public class MonitoredGpuService
{
    private GpuService _baseGpuService;
    
    public bool IsInitialized { private set; get; }
    public ObservableCollection<MonitoredGpu> GpuList { get; set; } = new();

    public MonitoredGpuService(TimeSpan updateInterval)
    {
        _baseGpuService = new GpuService();
        foreach (var g in _baseGpuService.GpuList)
        {
            GpuList.Add(new MonitoredGpu(g,updateInterval));
        }
        IsInitialized = true;
    }


    
}