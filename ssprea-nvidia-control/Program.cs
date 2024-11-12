using Avalonia;
using System;
using System.Collections.Generic;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control;

sealed class Program
{
    // public static void Main(string[] args)
    // {
    //     Console.WriteLine("init: " + NVML.NvmlWrapper.nvmlInit());
    //     uint deviceCount;
    //     Console.WriteLine("getdevicecount: "+NVML.NvmlWrapper.nvmlDeviceGetCount(out deviceCount));
    //     Console.WriteLine("devicecount: "+deviceCount);
    //     
    //     List<NvmlGpu> gpuList = new List<NvmlGpu>();
    //     for (uint i = 0; i < deviceCount; i++)
    //         gpuList.Add(new NvmlGpu(i));
    //     
    //     Console.WriteLine(gpuList[0].Name);
    //     
    //     Console.WriteLine("temp:"+gpuList[0].GetTemperature().Item1);
    //     Console.WriteLine("clock: "+gpuList[0].GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS));
    //     Console.WriteLine("clockoff: "+gpuList[0].GetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS,NvmlPStates.NVML_PSTATE_0));
    //     
    //     Console.WriteLine("getpstate: "+gpuList[0].GetPState());
    //     
    //     //Console.WriteLine("setclockoff: "+gpuList[0].SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS,NvmlPStates.NVML_PSTATE_0,20));
    //     Console.WriteLine("shutdown: "+NVML.NvmlWrapper.nvmlShutdown());
    //
    // }
    
    
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}