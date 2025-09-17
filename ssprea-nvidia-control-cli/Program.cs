using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using GpuSSharp;
using GpuSSharp.Libs.AmdOpenSysfs;
using GpuSSharp.Libs.Nvml;
using GpuSSharp.Libs.Nvml.NvmlTypes;
using GpuSSharp.Types;
using ssprea_nvidia_control_cli.Types;
using FanCurve = GpuSSharp.Libs.Nvml.FanCurve;

namespace ssprea_nvidia_control_cli;

public class Program
{
    // [Option(CommandOptionType.SingleValue, Description = "select gpu id", LongName = "gpu", ShortName = "g")]
    // public static uint? GpuId { get; set; }
    
    [Option(CommandOptionType.SingleValue, Description = "select gpu pci address", LongName = "gpu", ShortName = "g")]
    public static string? GpuPciAddr { get; set; }

    [Option(CommandOptionType.SingleValue, Description = "select 'amd' or 'nvidia'", LongName = "vendor", ShortName = "v")]
    public static string? Vendor { get; set; }

    [Option(CommandOptionType.NoValue, Description = "list available gpus", LongName = "listGpu")]
    public static bool DoListGpus { get; set; }
    
    [Option(CommandOptionType.NoValue, Description = "list specified gpu info", LongName = "info", ShortName = "i")]
    public static bool ShowGpuInfo { get; set; }

    [Option(CommandOptionType.SingleValue, Description = "set core offset MHz", LongName = "coreOffset",
        ShortName = "c")]
    public static int CoreOffset { get; set; } = -1;

    [Option(CommandOptionType.SingleValue, Description = "set mem offset MHz", LongName = "memoryOffset",
        ShortName = "m")]
    public static int MemoryOffset { get; set; } = -1;

    [Option(CommandOptionType.SingleValue, Description = "set power limit in mw", LongName = "powerLimit",
        ShortName = "p")]
    public static uint PowerLimit { get; set; } = 0;

    [Option(CommandOptionType.SingleValue, Description = "set fan speed", LongName = "fanSpeed", ShortName = "fs")]
    public static int FanSpeed { get; set; } = -1;

    [Option(CommandOptionType.NoValue, Description = "enable auto fan speed", LongName = "autoFanSpeed",
        ShortName = "afs")]
    public static bool AutoFanSpeed { get; set; } = false;

    [Option(CommandOptionType.SingleValue, Description = "load a fan speed curve json from the specified path.",
        LongName = "fanProfile", ShortName = "fp")]
    public static string FanSpeedCurveJson { get; set; } = "";

    [Option(CommandOptionType.SingleValue,
        Description = "load a oc profile json from the specified path. fan curve must be loaded separately",
        LongName = "ocProfile", ShortName = "op")]
    public static string OcProfileJson { get; set; } = "";

    [Option(CommandOptionType.NoValue,
        Description =
            "WARNING: this can cause problems. Skip checking if another snvctl process is already running (when applying fan profile).",
        LongName = "forceOpen")]
    public static bool SkipMultipleInstancesCheck { get; set; } = false;


    // [Option(CommandOptionType.MultipleValue, Description = "select fan id", LongName = "fanId",ShortName = "fi")]
    // public static int[] FanIds { get; set; }



    IGpu? _selectedGpu = null;

    CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
    
    
    public static void Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    // public static void Main(string[] args)
    // {
    //     var fancurve = FanCurve.DefaultFanCurve();
    //     Console.WriteLine(fancurve.ToString());
    //     return;
    // }
    

    public void HandleAmdSysfs()
    {
        SysfsWrapper.GetAllGpus().ForEach(x=>Console.WriteLine($"FOUND GPU: DRMID: {x.DrmId}\t DRMPATH: {x.DrmPath}\t PCI ADDRESS: {x.DevicePciAddress} \t NAME: {x.Name}"));
        
    }
    
    private void OnExecute()
    {

        var gpuServ = new GpuService();
        
        
        
        if (DoListGpus)
        {
            foreach (var g in gpuServ.GpuList)
            {
                Console.WriteLine("Name: " + g.Name + "\tAddress: " + g.DevicePciAddress);
            }

            return;
        }


        if (GpuPciAddr == null)
        {
            Console.WriteLine("please select a gpu!");
            return;
        }
        
        foreach (var gpu in gpuServ.GpuList)
        {
            if (gpu.DevicePciAddress.Trim() == GpuPciAddr.Trim())
                _selectedGpu = gpu;
        }
        
        if (_selectedGpu == null)
        {
            Console.WriteLine("specified GPU not found!");
            return;
        }

        if (ShowGpuInfo)
        {
            int textPadding = 35;
            var infoRows = new List<string>([
                $"|\n|  {(_selectedGpu.Vendor == GpuVendor.Nvidia ? "NVML" : "DRM")} ID: {_selectedGpu.DeviceIndex}".PadRight(textPadding) + $"PState: {_selectedGpu.GpuPState}".PadRight(textPadding)+ $"Mem Use: {_selectedGpu.MemoryUsedMB:F2} MB".PadRight(textPadding)+$"Power limit: {_selectedGpu.PowerLimitCurrentW}W".PadRight(textPadding)+$"Power Use: {_selectedGpu.GpuPowerUsageW:F2}"+"",
                $"|\n|  Vendor: {_selectedGpu.Vendor}".PadRight(textPadding)+ $"Core clock: {_selectedGpu.GpuClockCurrent} MHz".PadRight(textPadding) + $"Gpu Use: {_selectedGpu.UtilizationCore} %".PadRight(textPadding)+$"PL MAX: {_selectedGpu.PowerLimitMaxW}W".PadRight(textPadding)+ $"Temp: {_selectedGpu.GpuTemperature} °C",
                $"|\n|  Address: {_selectedGpu.DevicePciAddress}".PadRight(textPadding)+ $"Mem clock: {_selectedGpu.MemClockCurrent} MHz".PadRight(textPadding)+ $"Mem Ctrl Use: {_selectedGpu.UtilizationMemCtl} %".PadRight(textPadding)+$"PL MIN: {_selectedGpu.PowerLimitMinW}W".PadRight(textPadding)+$"Fan 0 speed: {_selectedGpu.Fan0SpeedPercent} %",
            ]);
            
            var longestRow = infoRows.Max(row => row.Length);
            Console.WriteLine(longestRow);
            var title = $"[ GPU Info: {_selectedGpu.Name} ]";
            Console.WriteLine("\no"+new string('=',(longestRow-title.Length)/2)+ title +new string('=',(longestRow-title.Length)/2)+"o");
            
            infoRows.ForEach(Console.WriteLine);
            
            Console.WriteLine("memusemb:"+_selectedGpu.MemoryUsedMB);
            Console.WriteLine("memfreemb:"+_selectedGpu.MemoryFreeMB);
            Console.WriteLine("memtotmb:"+_selectedGpu.MemoryTotalMB);
            Console.WriteLine("memuse:"+_selectedGpu.MemoryUsed);
            Console.WriteLine("memfree:"+_selectedGpu.MemoryFree);
            Console.WriteLine("memtot:"+_selectedGpu.MemoryTotal);
            Console.WriteLine($"\t\t  ");
            Console.WriteLine("\t");
                // \t DRMID: {((AmdSysfsGpu)_selectedGpu).DrmId} 
        }
        
        if (OcProfileJson != string.Empty)
        {
            if (File.Exists(OcProfileJson))
            {
                var ocProfile = OcProfile.FromJson(File.ReadAllText(OcProfileJson));

                if (ocProfile is null)
                {
                    Console.WriteLine("Invalid oc profile json");
                    Environment.Exit(1);
                }
            
                CoreOffset = (int)ocProfile.GpuClockOffset;
                MemoryOffset = (int)ocProfile.GpuClockOffset;
                PowerLimit = ocProfile.PowerLimitMw;
                
            }
            else
            {
                Console.WriteLine("OC profile file does not exist at path: "+OcProfileJson+" Skipping...");
            }
            
        }
        
        

        
        
        

        
        
        
        if (CoreOffset >= 0)
            Console.WriteLine(_selectedGpu.SetMemOffset(GpuPState.GPU_PSTATE_0, CoreOffset));
        if (MemoryOffset >= 0)
            Console.WriteLine(_selectedGpu.SetCoreOffset(GpuPState.GPU_PSTATE_0, MemoryOffset));
        if (PowerLimit > 0)
            Console.WriteLine(_selectedGpu.SetGpuPowerLimit(PowerLimit));

        if (FanSpeed >= 0)
        {
            _selectedGpu.ApplySpeedToAllFans((uint)FanSpeed);
        }

        if (AutoFanSpeed)
            _selectedGpu.ApplyAutoSpeedToAllFans();

        
        
        
        if (FanSpeedCurveJson != "")
        {
            //check if another instance is running
            if (!SkipMultipleInstancesCheck && IsAnotherInstanceRunning("snvctl","ssprea-nvidia-control-cli"))
            {
                Console.WriteLine("Another instance of this program is already running. Exiting...");
                Environment.Exit(1);
            }

            if (File.Exists(FanSpeedCurveJson))
            {
                var curve = JsonConvert.DeserializeObject<FanCurve>(File.ReadAllText(FanSpeedCurveJson));
                if (curve is null)
                {
                    Console.WriteLine("Fan curve not valid.");
                    return;
                }
                Thread t = new Thread(() => FanSpeedProfileThread(500,curve,CancelTokenSource.Token));
                t.Start();
            }
            else
            {
                Console.WriteLine("Fan curve file does not exist at path: "+FanSpeedCurveJson+" Skipping...");
            }
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
                
            LastFanTemp = (uint)_selectedGpu.GpuTemperature;
            Console.WriteLine($"Gpu temp: {_selectedGpu.GpuTemperature}, Fan Speed: {fanCurve.GpuTempToFanSpeedMap[(uint)_selectedGpu.GpuTemperature]}");
            _selectedGpu.ApplySpeedToAllFans(fanCurve.GpuTempToFanSpeedMap[(uint)_selectedGpu.GpuTemperature]);
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