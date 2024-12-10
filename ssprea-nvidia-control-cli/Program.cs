using System.ComponentModel.Design;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using ssprea_nvidia_control_cli.NVML;
using ssprea_nvidia_control_cli.NVML.NvmlTypes;
using ssprea_nvidia_control_cli.Types;

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
    
    [Option(CommandOptionType.NoValue, Description = "enable auto fan speed", LongName = "autoFanSpeed",ShortName = "afs")]
    public static bool AutoFanSpeed { get; set; }= false;
    
    [Option(CommandOptionType.SingleValue, Description = "load a fan speed curve json from the specified path.", LongName = "fanProfile",ShortName = "fp")]
    public static string FanSpeedCurveJson { get; set; }= "";
    
    // [Option(CommandOptionType.MultipleValue, Description = "select fan id", LongName = "fanId",ShortName = "fi")]
    // public static int[] FanIds { get; set; }
    
    
    
    static NvmlService? _nvmlService;
    NvmlGpu? _selectedGpu = null;
    
    public static void Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    // public static void Main(string[] args)
    // {
    //     var fancurve = FanCurve.DefaultFanCurve();
    //     Console.WriteLine(fancurve.ToString());
    //     return;
    // }
    


    private void OnExecute()
    {
        var cancelTokenSource = new CancellationTokenSource();
        
        _nvmlService = new NvmlService();

        if (DoListGpus)
        {
            foreach (var g in _nvmlService.GpuList)
            {
                Console.WriteLine("Name: " + g.Name + "\tID: " + g.DeviceIndex);
            }

            return;
        }
        
        
        

        
        foreach (var gpu in _nvmlService.GpuList)
        {
            if (gpu.DeviceIndex == GpuId)
                _selectedGpu = gpu;
        }

        if (_selectedGpu == null)
        {
            Console.WriteLine("GPU index not found");
            return;
        }
        
        
        if (CoreOffset >= 0)
            Console.WriteLine(_selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0, CoreOffset));
        if (MemoryOffset >= 0)
            Console.WriteLine(_selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0, MemoryOffset));
        if (PowerLimit > 0)
            Console.WriteLine(_selectedGpu.SetPowerLimit(PowerLimit));

        if (FanSpeed >= 0)
        {
            _selectedGpu.ApplySpeedToAllFans((uint)FanSpeed);
        }

        if (AutoFanSpeed)
            _selectedGpu.ApplyPolicyToAllFans(NvmlFanControlPolicy.NVML_FAN_POLICY_TEMPERATURE_CONTINOUS_SW);

        if (FanSpeedCurveJson != "")
        {
            var curve = JsonConvert.DeserializeObject<FanCurve>(File.ReadAllText(FanSpeedCurveJson));
            if (curve is null)
            {
                Console.WriteLine("Fan curve not valid.");
                return;
            }
            Thread t = new Thread(() => FanSpeedProfileThread(500,curve,cancelTokenSource.Token));
            t.Start();
        }

    }

    private uint LastFanTemp = 0;
    
    private void FanSpeedProfileThread(int updateDelayMilliseconds, FanCurve fanCurve,CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Thread.Sleep(updateDelayMilliseconds);
            //get gpu temperature
            if (_selectedGpu is null || _selectedGpu.GpuTemperature == LastFanTemp)
            {
                Console.WriteLine("No temp change since last update. skipping");
                continue;
            }
                
            LastFanTemp = _selectedGpu.GpuTemperature;
            Console.WriteLine($"Gpu temp: {_selectedGpu.GpuTemperature}, Fan Speed: {fanCurve.GpuTempToFanSpeedMap[_selectedGpu.GpuTemperature]}");
            _selectedGpu.ApplySpeedToAllFans(fanCurve.GpuTempToFanSpeedMap[_selectedGpu.GpuTemperature]);
        }
    }

   
}