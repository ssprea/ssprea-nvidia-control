using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using GpuSSharp;
using GpuSSharp.Libs.AmdSmi;
using GpuSSharp.Types;
using Grpc.Net.Client;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Serilog;
using SLimit.Contracts;
using sspreaNvidiaControlCli.Types;

namespace sspreaNvidiaControlCli;

public class Program
{
    [Option(CommandOptionType.SingleValue, Description = "select gpu by pci address", LongName = "gpu", ShortName = "g")]
    public static string? GpuPciIdStr { get; set; }
    
    [Option(CommandOptionType.NoValue, Description = "list available gpus", LongName = "listGpu")]
    public static bool DoListGpus { get; set; }
    
    [Option(CommandOptionType.NoValue, Description = "list specified gpu info", LongName = "info", ShortName = "i")]
    public static bool ShowGpuInfo { get; set; }
    
    [Option(CommandOptionType.SingleValue, Description = "set core overclock", LongName = "coreOffset", ShortName = "c")]
    public static string CoreOffsetStr { get; set; } = "";
        
    [Option(CommandOptionType.SingleValue, Description = "set mem overclock", LongName = "memoryOffset",ShortName = "m")]
    public static string MemoryOffsetStr { get; set; }= "";
    
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
    
    
    // [Option(CommandOptionType.SingleValue, Description = "Set the logging level. Can be 0 = DEBUG, 1 = INFO, 2 = WARN, 3 = ERR", LongName = "logLevel",ShortName = "ll")]
    // public static int LogLevel { get; set; }= 1;
    
    // [Option(CommandOptionType.MultipleValue, Description = "select fan id", LongName = "fanId",ShortName = "fi")]
    // public static int[] FanIds { get; set; }
    
    

    
    static GpuService? _gpuService;
    IGpu? _selectedGpu;

    public static void Main(string[] args)
    {
        foreach (var a in args)
        {
            Console.WriteLine(a);
        }
        CommandLineApplication.Execute<Program>(args);
    }

    
    
    
    // public static void Main(string[] args)
    // {
    //     var fancurve = FanCurve.DefaultFanCurve();
    //     Console.WriteLine(fancurve.ToString());
    //     return;
    // }

    public static string? GpuPciAddress { get; set; }

    private static bool IsValidPciAddress(string address)
    {
        return Regex.IsMatch(address, @"^[0-9a-fA-F]{4}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}\.[0-9a-fA-F]$");
    }

    private async Task OnExecuteAsync()
    {
        
        
        await using var log = new LoggerConfiguration() 
            .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
            .CreateLogger();

        Log.Logger = log;
        
        var cancelTokenSource = new CancellationTokenSource();

        
        
        
        _gpuService = new GpuService();
        
        
        
        if (DoListGpus)
        {
            foreach (var g in _gpuService.GpuList)
            {
                Console.WriteLine("Name: " + g.Name + "\tID: " + g.DevicePciAddress);
            }

            return;
        }

        if (GpuPciIdStr is null)
        {
            Log.Fatal("No gpu address provided. Exiting.");
            Environment.Exit(1);
        }
        
        
        if (IsValidPciAddress(GpuPciIdStr)) //if arg is already valid pci address use that
        {
            GpuPciAddress = GpuPciIdStr;
        }
        else if (Path.Exists(GpuPciIdStr)) //if is valid path read from path and verify 
        {
            var read = (await File.ReadAllTextAsync(GpuPciIdStr)).Trim();
            
            if (IsValidPciAddress(read))
                GpuPciAddress = read;  
            
        }
        
        if (GpuPciAddress is null)
        {
            Log.Fatal("Invalid GPU ID: {gpuInputAddress}, Exiting...",GpuPciIdStr);
            Environment.Exit(-1);
        }
        
        foreach (var gpu in _gpuService.GpuList)
        {
            if (gpu.DevicePciAddress == GpuPciAddress)
                _selectedGpu = gpu;
        }
        

        if (_selectedGpu == null)
        {
            Log.Fatal("GPU address not found");
            return;
        }
        
        
        if (ShowGpuInfo)
        {
            if (_selectedGpu is null)
            {
                Log.Fatal("Invalid GPU , Exiting...");
                Environment.Exit(-1);
            }
            
            var reading = _selectedGpu.GetMetrics();
            
            int textPadding = 35;
            var infoRows = new List<string>([
                $"|\n|  {(_selectedGpu.Vendor == GpuVendor.Nvidia ? "NVML" : "DRM")} ID: {_selectedGpu.DeviceIndex}".PadRight(textPadding) + $"PState: {reading.GpuPState}".PadRight(textPadding)+ $"Mem Use: {(reading.MemoryUsedB/1000000d):F2} MB".PadRight(textPadding)+$"Power limit: {reading.PowerLimitCurrentMilliW/1000}W".PadRight(textPadding)+$"Power Use: {reading.GpuPowerUsageMilliW/1000:F2}"+"",
                $"|\n|  Vendor: {_selectedGpu.Vendor}".PadRight(textPadding)+ $"Core clock: {reading.GpuClockCurrent} MHz".PadRight(textPadding) + $"Gpu Use: {reading.UtilizationCore} %".PadRight(textPadding)+$"PL MAX: {_selectedGpu.PowerLimitMaxMw/1000}W".PadRight(textPadding)+ $"Temp: {reading.GpuTemperature} °C",
                $"|\n|  Address: {_selectedGpu.DevicePciAddress}".PadRight(textPadding)+ $"Mem clock: {reading.MemClockCurrent} MHz".PadRight(textPadding)+ $"Mem Ctrl Use: {reading.UtilizationMemCtl} %".PadRight(textPadding)+$"PL MIN: {_selectedGpu.PowerLimitMinMw/1000}W".PadRight(textPadding)+$"Fan 0 speed: {reading.FansSpeedPercent.Fan0Speed} %",
                $"|\n|  Temp throttle: {_selectedGpu.TemperatureThresholdThrottle}".PadRight(textPadding)+ $"Temp slow: {_selectedGpu.TemperatureThresholdSlowdown} MHz".PadRight(textPadding)+ $"Temp shutdown: {_selectedGpu.TemperatureThresholdShutdown} %".PadRight(textPadding)+$"PL MIN: {_selectedGpu.PowerLimitMinMw/1000}W".PadRight(textPadding)+$"Fan 0 speed: {reading.FansSpeedPercent.Fan0Speed} %",
            ]);
            
            var longestRow = infoRows.Max(row => row.Length);
            Console.WriteLine(longestRow);
            var title = $"[ GPU Info: {_selectedGpu.Name} ]";
            Console.WriteLine("\no"+new string('=',(longestRow-title.Length)/2)+ title +new string('=',(longestRow-title.Length)/2)+"o");
            
            infoRows.ForEach(Console.WriteLine);
            
            // Console.WriteLine("memusemb:"+_selectedGpu.MemoryUsedMB);
            // Console.WriteLine("memfreemb:"+_selectedGpu.MemoryFreeMB);
            // Console.WriteLine("memtotmb:"+_selectedGpu.MemoryTotalMB);
            // Console.WriteLine("memuse:"+_selectedGpu.MemoryUsed);
            // Console.WriteLine("memfree:"+_selectedGpu.MemoryFree);
            // Console.WriteLine("memtot:"+_selectedGpu.MemoryTotal);
            Console.WriteLine($"\t\t  ");
            Console.WriteLine("\t");
                // \t DRMID: {((AmdSysfsGpu)_selectedGpu).DrmId} 
        }

        const string socketPath = "/run/slimit-grpc.sock";
        using var daemonHandler = new SocketsHttpHandler
        {
            UseProxy = false,

            ConnectCallback = async (_, cancellationToken) =>
            {
                var socket = new Socket(
                    AddressFamily.Unix,
                    SocketType.Stream,
                    ProtocolType.Unspecified);

                try
                {
                    await socket.ConnectAsync(
                        new UnixDomainSocketEndPoint(socketPath),
                        cancellationToken);

                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };

        using var daemonChannel = GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions
            {
                HttpHandler = daemonHandler
            });


        var daemonClient = new GpuControl.GpuControlClient(daemonChannel);
        
        
        
        if (OcProfileJson != string.Empty)
        {
            if (File.Exists(OcProfileJson))
            {

                var json = await File.ReadAllTextAsync(OcProfileJson);
                
                var voffResp = daemonClient.GpuApplyOcProfile(new OcProfileApplyRequest()
                {
                    GpuId = _selectedGpu.DevicePciAddress,
                    ProfileJson = json
                });

                if (voffResp.StatusCode != 0)
                {
                    Log.Error("Error response from daemon while applying overclock profile: {msg}", voffResp.Message);
                }

                return;
                // Log.Information("Applying settings from loaded profile: ");
                //
                // switch (ocProfile.GpuClockTune)
                // {
                //     case GpuClockTune.ClockRange range:
                //         Log.Information("Core range: Min: {minMhz}MHz Max:{maxMhz}MHz", range.MinMhz, range.MaxMhz);
                //         break;
                //     case GpuClockTune.Overdrive od:
                //         Log.Information("Core overdrive: +{odPercent}%", od.Percent);
                //         break;
                //     case GpuClockTune.Offset offset:
                //         Log.Information("Core offset: +{minMhz}MHz", offset.OffsetMhz);
                //         break;
                // }
                
                // switch (ocProfile.MemClockTune)
                // {
                //     case GpuClockTune.ClockRange range:
                //         Log.Information("Memory range: Min: {minMhz}MHz Max:{maxMhz}MHz", range.MinMhz, range.MaxMhz);
                //         break;
                //     case GpuClockTune.Overdrive od:
                //         Log.Information("Memory overdrive: +{odPercent}%", od.Percent);
                //         break;
                //     case GpuClockTune.Offset offset:
                //         Log.Information("Memory offset: +{minMhz}MHz", offset.OffsetMhz);
                //         break;
                // }
                //
                // Log.Information("Power limit: {powerLimitW}",ocProfile.PowerLimitMw);
                
                
            }
            else
            {
                Log.Error("OC profile file does not exist at path: {ocProfileJson} Skipping...",OcProfileJson);
            }
            
        }
        
        
        if (!string.IsNullOrWhiteSpace(CoreOffsetStr))
        {
            Log.Information("Read Core offset: {coreOffset}", CoreOffsetStr);
            var clockTune  = OcStringToTune(CoreOffsetStr);
            Log.Information("Core tune: {coreOffset}", clockTune.ToString());
            
            
            var clockResp = daemonClient.GpuApplyCoreTune(new ClockSetRequest()
            {
                GpuId = _selectedGpu.DevicePciAddress,
                Tune = ToProto(clockTune)
            });
            
            
            if (clockResp.StatusCode != 0)
                Log.Error("Error response from daemon while applying core clock tune: {msg}",clockResp.Message);
        }

        if (!string.IsNullOrWhiteSpace(MemoryOffsetStr))
        {
            Log.Information("Read Memory offset: {menOffset}", MemoryOffsetStr);
            var memTune = OcStringToTune(MemoryOffsetStr);

            Log.Information("Memory tune: {memOffset}", memTune.ToString());
            
            var clockResp = daemonClient.GpuApplyMemoryTune(new ClockSetRequest()
            {
                GpuId = _selectedGpu.DevicePciAddress,
                Tune = ToProto(memTune)
            });
            
            
            if (clockResp.StatusCode != 0)
                Log.Error("Error response from daemon while applying memory clock tune: {msg}",clockResp.Message);

        }

        if (PowerLimit > 0)
        {
            var plResp = daemonClient.GpuApplyPowerLimit(new PowerLimitSetRequest()
            {
                GpuId = _selectedGpu.DevicePciAddress,
                PowerLimitMw = PowerLimit
            });
            
            
            if (plResp.StatusCode != 0)
                Log.Error("Error response from daemon while applying power limit: {msg}",plResp.Message);
                
        }

        if (FanSpeed >= 0)
        {
            if (!_selectedGpu.ApplySpeedToAllFans((uint)FanSpeed))
                Log.Error("Error while applying static fan speed.");
            else
                Log.Information("Successfully applied static fan speed: {appliedFanSpeed}%",FanSpeed);
        }

        if (AutoFanSpeed)
        {
            var plResp = daemonClient.GpuResetFan(new FanResetRequest()
            {
                GpuId = _selectedGpu.DevicePciAddress,
            });
            
            
            if (plResp.StatusCode != 0)
                Log.Error("Error response from daemon while resetting fan: {msg}",plResp.Message);
        }
            


        
        
        
        if (FanSpeedCurveJson != "")
        {
            //check if another instance is running
            // SkipMultipleInstancesCheck = _selectedGpu.Vendor == GpuVendor.Amd;
            // if (!SkipMultipleInstancesCheck && IsAnotherInstanceRunning("snvctl","ssprea-nvidia-control-cli"))
            // {
            //     Log.Fatal("Another instance of this program is already running. Exiting...");
            //     Environment.Exit(1);
            // }

            
            
            if (File.Exists(FanSpeedCurveJson))
            {
                
                var json = await File.ReadAllTextAsync(FanSpeedCurveJson);
                
                var curveResp = daemonClient.GpuApplyFanCurve(new FancurveSetRequest()
                {
                    GpuId = _selectedGpu.DevicePciAddress,
                    CurveJson = json
                });

                if (curveResp.StatusCode != 0)
                {
                    Log.Error("Error response from daemon while applying fan curve: {msg}",curveResp.Message);
                }
                
                
            }
            else
            {
                Log.Error("Fan curve file does not exist at path: {fanSpeedCurveJson} Skipping...",FanSpeedCurveJson);
            }
        }

    }

    private static GpuClockTune OcStringToTune(string ocString)
    {
        if (ocString.Contains(':'))
        {
            var range = ocString.Split(':');
            
            if (uint.TryParse(range[0], out uint minClock) && uint.TryParse(range[1], out uint maxClock))
                return new GpuClockTune.ClockRange(minClock, maxClock);
            
        } else if (ocString.Contains('%'))
        {
            var od = ocString.Split('%');

            if (uint.TryParse(od[0], out uint odVal))
                return new GpuClockTune.Overdrive(odVal);
            
            
        } else if (int.TryParse(ocString, out int offset))
        {
            return new GpuClockTune.Offset(offset, GpuPState.GpuPstate0);
        }
        throw new ArgumentException("Invalid oc string "+ocString);
    }
    
   
    
    // private async Task FanSpeedProfileThread(int updateDelayMilliseconds, FanCurve fanCurve,CancellationToken cancelToken)
    // {
    //     int errorCounter = 0;
    //     int errorQuitThreshold = 50;
    //
    //     if (_selectedGpu is null)
    //     {
    //         Log.Error("Cannot start fan curve thread: No gpu selected.");
    //         return;
    //     }
    //
    //     if (_selectedGpu.Vendor == GpuVendor.Amd)
    //     {
    //         Log.Information("Selected GPU is AmdGpu, fan curve thread not required.");
    //         var amdGpu = (AmdSmiGpu)_selectedGpu;
    //         var points = fanCurve.CurvePoints.Select(x => (x.Temperature, x.FanSpeed));
    //         amdGpu.ApplyFirmwareFanCurve(points.ToList());
    //         Log.Information("Succesfully loaded fan curve to GPU firmware: {fanCurveName}. Exiting.",amdGpu.Name);
    //         
    //         return;
    //     }
    //     
    //     Log.Information("Starting fan curve thread for GPU: {gpuName}, update delay: {pollDelay}ms",_selectedGpu.Name,updateDelayMilliseconds);
    //     using var timer = new PeriodicTimer(
    //         TimeSpan.FromMilliseconds(updateDelayMilliseconds));
    //     
    //     while (await timer.WaitForNextTickAsync(cancelToken))
    //     {
    //         //get metrics reading
    //         if (_selectedGpu is null)
    //         {
    //             Log.Fatal("Fan curve thread interrupted: selected gpu became invalid.");
    //             return;
    //         }
    //
    //         try
    //         {
    //
    //
    //             var latestMetrics = _selectedGpu.GetMetrics();
    //             var currentTemp = (uint)latestMetrics.GpuTemperature;
    //
    //             //get gpu temperature
    //             if (_selectedGpu is null || currentTemp == _lastFanTemp)
    //             {
    //                 Log.Debug("No temp change since last update. skipping");
    //                 continue;
    //             }
    //
    //
    //
    //             Log.Debug("Gpu temp: {gpuTemp}, Fan Speed: {fanSpeed}", currentTemp,
    //                 fanCurve.GpuTempToFanSpeedMap[currentTemp]);
    //             if (!_selectedGpu.ApplySpeedToAllFans(fanCurve.GpuTempToFanSpeedMap[currentTemp]))
    //             {
    //                 errorCounter++;
    //                 Log.Error("({errorCount}) Error while applying fan speed.", errorCounter);
    //             }
    //             else
    //                 errorCounter = 0;
    //
    //
    //             if (errorCounter > errorQuitThreshold)
    //             {
    //                 Log.Fatal("More than {quitThreshold} errors when applying fan curve. Quitting program.",
    //                     errorQuitThreshold);
    //                 Environment.Exit(-1);
    //             }
    //
    //             _lastFanTemp = (uint)latestMetrics.GpuTemperature;
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine(ex);
    //         }
    //     }
    // }
    //
    // private bool IsAnotherInstanceRunning(params string[] names)
    // {
    //     //check if service is running (this requires service to use --forceOpen switch)
    //     if (Utils.Systemd.IsSystemdServiceRunning(_serviceName))
    //         return true;
    //     
    //     
    //     var instanceCount = 0;
    //     
    //     
    //     
    //     foreach(var n in names)
    //         if (n == Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location))
    //             instanceCount--;
    //     
    //     
    //     instanceCount += System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location)).Length;
    //     foreach (var n in names)
    //     {
    //         instanceCount += System.Diagnostics.Process.GetProcessesByName(n).Length;
    //     }
    //     // Console.WriteLine("instancecount: "+instanceCount);
    //     return instanceCount > 1;
    //
    //     
    // }

    private static ClockTuneMessage ToProto(GpuClockTune tune) =>
        tune switch
        {
            GpuClockTune.Offset x => new ClockTuneMessage
            {
                Offset = new OffsetTune
                {
                    OffsetMhz = x.OffsetMhz,
                    PState = (int)x.PState
                }
            },

            GpuClockTune.Overdrive x => new ClockTuneMessage
            {
                Overdrive = new OverdriveTune
                {
                    Percent = x.Percent
                }
            },

            GpuClockTune.ClockRange x => new ClockTuneMessage
            {
                ClockRange = new ClockRangeTune
                {
                    MinMhz = x.MinMhz,
                    MaxMhz = x.MaxMhz
                }
            },

            _ => throw new ArgumentException(
                "Unknown tuning type", nameof(tune))
        };
   
}