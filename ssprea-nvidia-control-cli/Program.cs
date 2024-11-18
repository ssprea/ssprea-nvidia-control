using System.ComponentModel.Design;
using McMaster.Extensions.CommandLineUtils;
using ssprea_nvidia_control_cli.NVML;
using ssprea_nvidia_control_cli.NVML.NvmlTypes;

namespace ssprea_nvidia_control_cli;

public class Program
{
    [Option(CommandOptionType.SingleValue, Description = "select gpu id", LongName = "gpu", ShortName = "g")]
    public static uint GpuId { get; set; }
    
    [Option(CommandOptionType.NoValue, Description = "list available gpus", LongName = "listGpu")]
    public static bool DoListGpus { get; set; }
    
    [Option(CommandOptionType.SingleValue, Description = "set core offset mHz", LongName = "coreOffset", ShortName = "c")]
    public static int CoreOffset { get; set; } = -1;
        
    [Option(CommandOptionType.SingleValue, Description = "set mem offset mHz", LongName = "memoryOffset",ShortName = "m")]
    public static int MemoryOffset { get; set; }= -1;
    
    [Option(CommandOptionType.SingleValue, Description = "set power limit in mw", LongName = "powerLimit",ShortName = "p")]
    public static uint PowerLimit { get; set; }= 0;
    
    [Option(CommandOptionType.SingleValue, Description = "set fan speed", LongName = "fanSpeed",ShortName = "fs")]
    public static int FanSpeed { get; set; }= -1;
    
    // [Option(CommandOptionType.MultipleValue, Description = "select fan id", LongName = "fanId",ShortName = "fi")]
    // public static int[] FanIds { get; set; }
    
    
    
    static NvmlService? _nvmlService;
    
    
    public static void Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);
    


    private void OnExecute()
    {
        _nvmlService = new NvmlService();

        if (DoListGpus)
        {
            foreach (var g in _nvmlService.GpuList)
            {
                Console.WriteLine("Name: " + g.Name + "\tID: " + g.DeviceIndex);
            }

            return;
        }

        NvmlGpu? selectedGpu = null;
        foreach (var gpu in _nvmlService.GpuList)
        {
            if (gpu.DeviceIndex == GpuId)
                selectedGpu = gpu;
        }

        if (selectedGpu == null)
        {
            Console.WriteLine("GPU index not found");
            return;
        }
        
        
        if (CoreOffset >= 0)
            Console.WriteLine(selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0, CoreOffset));
        if (MemoryOffset >= 0)
            Console.WriteLine(selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0, MemoryOffset));
        if (PowerLimit > 0)
            Console.WriteLine(selectedGpu.SetPowerLimit(PowerLimit));

        if (FanSpeed >= 0)
        {
            selectedGpu.ApplySpeedToAllFans((uint)FanSpeed);
        }
        
        
    }

   
}