using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
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
    
    [Option(CommandOptionType.SingleValue, Description = "set core offset MHz", LongName = "coreOffset", ShortName = "c")]
    public static int CoreOffset { get; set; } = -1;
        
    [Option(CommandOptionType.SingleValue, Description = "set mem offset MHz", LongName = "memoryOffset",ShortName = "m")]
    public static int MemoryOffset { get; set; }= -1;
    
    [Option(CommandOptionType.SingleValue, Description = "set power limit in mw", LongName = "powerLimit",ShortName = "p")]
    public static uint PowerLimit { get; set; }= 0;
    
    [Option(CommandOptionType.SingleValue, Description = "set fan speed", LongName = "fanSpeed",ShortName = "fs")]
    public static int FanSpeed { get; set; }= -1;
    
    [Option(CommandOptionType.NoValue, Description = "enable auto fan speed", LongName = "autoFanSpeed",ShortName = "afs")]
    public static bool AutoFanSpeed { get; set; }= false;
    
    [Option(CommandOptionType.SingleValue, Description = "load a fan speed curve json from the specified path.", LongName = "fanProfile",ShortName = "fp")]
    public static string FanSpeedCurveJson { get; set; }= "";
    
    [Option(CommandOptionType.SingleValue, Description = "load a oc profile json from the specified path. fan curve must be loaded separately", LongName = "ocProfile",ShortName = "op")]
    public static string OcProfileJson { get; set; }= "";
    
    [Option(CommandOptionType.NoValue, Description = "WARNING: this can cause problems. Skip checking if another snvctl process is already running (when applying fan profile).", LongName = "forceOpen")]
    public static bool SkipMultipleInstancesCheck { get; set; }= false;
    
    // [Option(CommandOptionType.SingleValue, Description = "Set the logging level. Can be 0 = DEBUG, 1 = INFO, 2 = WARN, 3 = ERR", LongName = "logLevel",ShortName = "ll")]
    // public static int LogLevel { get; set; }= 1;
    
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
        
        
        using var log = new LoggerConfiguration() 
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = log;
        
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
        
        
        
        
        if (OcProfileJson != string.Empty)
        {
            if (File.Exists(OcProfileJson))
            {
                var ocProfile = OcProfile.FromJson(File.ReadAllText(OcProfileJson));

                if (ocProfile is null)
                {
                    Log.Fatal("Invalid oc profile json");
                    Environment.Exit(1);
                }
            
                CoreOffset = (int)ocProfile.GpuClockOffset;
                MemoryOffset = (int)ocProfile.MemClockOffset;
                PowerLimit = ocProfile.PowerLimitMw;
                
                Log.Information("Applying settings from loaded profile: CORE OFFSET: {CoreOffset} MHz, MEM OFFSET: {MemoryOffset} MHz, POWER LIMIT: {PowerLimit} mW",CoreOffset,MemoryOffset,PowerLimit);
                
            }
            else
            {
                Log.Error("OC profile file does not exist at path: {ocProfileJson} Skipping...",OcProfileJson);
            }
            
        }
        
        

        
        foreach (var gpu in _nvmlService.GpuList)
        {
            if (gpu.DeviceIndex == GpuId)
                _selectedGpu = gpu;
        }
        

        if (_selectedGpu == null)
        {
            Log.Fatal("GPU index not found");
            return;
        }


        if (CoreOffset >= 0)
        {

            var clockRes = _selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0, CoreOffset);
            if (clockRes != NvmlReturnCode.NVML_SUCCESS)
                Log.Error("Error while applying core clock offset: {coreClockOffsetApplyErrorDesc}",clockRes);
        }

        if (MemoryOffset >= 0)
        {
            
            var memRes = _selectedGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0, MemoryOffset);
            if (memRes != NvmlReturnCode.NVML_SUCCESS)
                Log.Error("Error while applying memory clock offset: {memoryClockOffsetApplyErrorDesc}",memRes);

        }

        if (PowerLimit > 0)
        {
            var plRes = _selectedGpu.SetPowerLimit(PowerLimit);
            if (plRes != NvmlReturnCode.NVML_SUCCESS)
                Log.Error("Error while applying power limit: {powerLimitApplyErrorDesc}",plRes);
                
        }

        if (FanSpeed >= 0)
        {
            if (!_selectedGpu.ApplySpeedToAllFans((uint)FanSpeed,out var fsRc))
                Log.Error("Error while applying static fan speed: {fanSpeedReturnCode}",fsRc);
            else
                Log.Information("Successfully applied static fan speed: {appliedFanSpeed}%",FanSpeed);
        }

        if (AutoFanSpeed)
            if (!_selectedGpu.ApplyPolicyToAllFans(NvmlFanControlPolicy.NVML_FAN_POLICY_TEMPERATURE_CONTINOUS_SW,out var afsRc))
                Log.Error("Error while applying auto fan speed: {autoFanSpeedReturnCode}",afsRc);
            else
                Log.Information("Successfully applied auto fan speed");
            


        
        
        
        if (FanSpeedCurveJson != "")
        {
            //check if another instance is running
            if (!SkipMultipleInstancesCheck && IsAnotherInstanceRunning("snvctl","ssprea-nvidia-control-cli"))
            {
                Log.Fatal("Another instance of this program is already running. Exiting...");
                Environment.Exit(1);
            }

            if (File.Exists(FanSpeedCurveJson))
            {
                var curve = JsonConvert.DeserializeObject<FanCurve>(File.ReadAllText(FanSpeedCurveJson));
                if (curve is null)
                {
                    Log.Error("Fan curve not valid.");
                    return;
                }
                Thread t = new Thread(() => FanSpeedProfileThread(500,curve,cancelTokenSource.Token));
                t.Start();
            }
            else
            {
                Log.Error("Fan curve file does not exist at path: {fanSpeedCurveJson} Skipping...",FanSpeedCurveJson);
            }
        }

    }

    private uint LastFanTemp = 0;
    
    private void FanSpeedProfileThread(int updateDelayMilliseconds, FanCurve fanCurve,CancellationToken cancelToken)
    {
        int errorCounter = 0;
        int errorQuitThreshold = 50;
        
        while (!cancelToken.IsCancellationRequested)
        {
            Thread.Sleep(updateDelayMilliseconds);
            //get gpu temperature
            if (_selectedGpu is null || _selectedGpu.GpuTemperature == LastFanTemp)
            {
                Log.Debug("No temp change since last update. skipping");
                continue;
            }
                
            LastFanTemp = _selectedGpu.GpuTemperature;
            Log.Debug($"Gpu temp: {_selectedGpu.GpuTemperature}, Fan Speed: {fanCurve.GpuTempToFanSpeedMap[_selectedGpu.GpuTemperature]}");
            if (!_selectedGpu.ApplySpeedToAllFans(fanCurve.GpuTempToFanSpeedMap[_selectedGpu.GpuTemperature],
                    out var rc))
            {
                errorCounter++;
                Log.Error("({errorCount}) Error while applying fan speed: {nvmlFanSpeedReturnCode}", errorCounter, rc);
            }
            else
                errorCounter = 0;


            if (errorCounter > errorQuitThreshold)
            {
                Log.Fatal("More than {quitThreshold} errors when applying fan curve. Quitting program.",errorQuitThreshold);
                Environment.Exit(-1);
            }
        }
    }

    private bool IsAnotherInstanceRunning(params string[] names)
    {
        //check if service is running (this requires service to use --forceOpen switch)
        if (Utils.Systemd.IsSystemdServiceRunning("snvctl.service"))
            return true;
        
        
        var instanceCount = 0;
        
        
        
        foreach(var n in names)
            if (n == Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location))
                instanceCount--;
        
        
        instanceCount += System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location)).Count();
        foreach (var n in names)
        {
            instanceCount += System.Diagnostics.Process.GetProcessesByName(n).Count();
        }
        // Console.WriteLine("instancecount: "+instanceCount);
        return instanceCount > 1;

        
    }

   
}