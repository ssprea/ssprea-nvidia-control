using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GpuSSharp.Types;
using Serilog;
using SLimit.Contracts;
using sspreaNvidiaControl.Models;
using sspreaNvidiaControl.Utils;

namespace sspreaNvidiaControl.ViewModels;

public partial class GpuViewModel : ViewModelBase, IDisposable
{
    private readonly IGpu _gpu;
    private CancellationTokenSource? _updateCts;
    
    [ObservableProperty] private GpuMetrics? _latestGpuMetrics;
    [ObservableProperty] private FanCurve? _appliedFanCurve;
    
    public GpuCapabilities Capabilities => _gpu.Capabilities;
    
    private Task? _runningUpdateTask;
    
    public event EventHandler? GpuMetricsUpdatedEvent;
    
    #region Fixed Properties

    public string Name => _gpu.Name;
    public GpuVendor Vendor => _gpu.Vendor;

    public uint DeviceIndex => _gpu.DeviceIndex;
    public string DevicePciAddress => _gpu.DevicePciAddress;
    
    public uint GpuPowerLimitDefaultMilliW => _gpu.PowerLimitDefaultMw;
    public uint GpuPowerLimitMaxMilliW => _gpu.PowerLimitMaxMw;
    public uint GpuPowerLimitMinMilliW => _gpu.PowerLimitMinMw;
    
    public double GpuPowerLimitMinW => GpuPowerLimitMinMilliW / 1000f;
    public double GpuPowerLimitMaxW => GpuPowerLimitMaxMilliW / 1000f;
    public double GpuPowerLimitDefaultW => GpuPowerLimitDefaultMilliW / 1000f;
    
    public uint GpuFansCount => _gpu.FansCount;
    
    public uint GpuTemperatureThresholdShutdown => _gpu.TemperatureThresholdShutdown;
    public uint GpuTemperatureThresholdSlowdown => _gpu.TemperatureThresholdSlowdown;
    public uint GpuTemperatureThresholdThrottle => _gpu.TemperatureThresholdThrottle;
    
    
    
    public uint ClockCoreMaxMhz => _gpu.ClockCoreMaxMhz;
    public uint ClockCoreMinMhz => _gpu.ClockCoreMinMhz;
    public uint ClockMemMaxMhz  => _gpu.ClockMemMaxMhz ;
    public uint ClockMemMinMhz  => _gpu.ClockMemMinMhz ;

    public int VoltageCoreMinOffsetMv => _gpu.VoltageCoreMinOffsetMv;
    public int VoltageCoreMaxOffsetMv => _gpu.VoltageCoreMaxOffsetMv;
    
    //DRIVER

    public string DriverVersion => _gpu.DriverVersion;
    
    
    #endregion
    
    #region Setters

    // private bool SetCoreClockOffset(int clockOffsetMhz) =>
    //     SnvctlCliTool.RunSudoCliCommand($"-c {clockOffsetMhz}", DevicePciAddress) is not null;
    
    
    
    public bool SetPowerLimit(int limitMw)
    {
        if (Program.DaemonSession is null)
            return false;
        
        return Program.DaemonSession.Client.GpuApplyPowerLimit(new PowerLimitSetRequest()
        {
            GpuId = DevicePciAddress,
            PowerLimitMw = (uint)limitMw
        }).StatusCode == 0;
        // SnvctlCliTool.RunSudoCliCommand($"-p {limitMw}", DevicePciAddress) is not null;
    }
    
    public bool SetCoreVoltageOffset(int offsetMv)
    {
        if (Program.DaemonSession is null)
            return false;
        
        Console.WriteLine("VOFF"+offsetMv);
        
        return Program.DaemonSession.Client.GpuApplyVoltageOffset(new VoltageOffsetSetRequest()
        {
            GpuId = DevicePciAddress,
            VoltOffsetMv = offsetMv
        }).StatusCode == 0;
        // SnvctlCliTool.RunSudoCliCommand($"-p {limitMw}", DevicePciAddress) is not null;
    }

    public bool ApplyAutoSpeedToAllFans()
    {
        if (Program.DaemonSession is null)
            return false;
        
        return Program.DaemonSession.Client.GpuResetFan(new FanResetRequest()
        {
            GpuId = DevicePciAddress,
        }).StatusCode == 0;
    }
    
    public bool ApplySpeedToAllFans(uint speed) =>
        SnvctlCliTool.RunSudoCliCommand($"-fs {speed}", DevicePciAddress) is not null;

    public bool ApplyCoreClockTune(GpuClockTune tune)
    {
        if (Program.DaemonSession is null)
            return false;
        
        if (IsTuneValid(tune, Capabilities.CoreClockTuningMode))
        {
            return Program.DaemonSession.Client.GpuApplyCoreTune(new ClockSetRequest()
            {
                GpuId = DevicePciAddress,
                Tune = ToProto(tune)
            }).StatusCode == 0;
        }

        return false;
    }
    
    public bool ApplyMemClockTune(GpuClockTune tune)
    {
        if (Program.DaemonSession is null)
            return false;
        
        if (IsTuneValid(tune, Capabilities.MemoryClockTuningMode))
        {
            return Program.DaemonSession.Client.GpuApplyMemoryTune(new ClockSetRequest()
            {
                GpuId = DevicePciAddress,
                Tune = ToProto(tune)
            }).StatusCode==0;
        }

        return false;
    }
    
    private static bool IsTuneValid(
        GpuClockTune tune,
        GpuClockTuningMode supportedMode)
    {
        ArgumentNullException.ThrowIfNull(tune);

        var requestedMode = tune switch
        {
            GpuClockTune.Offset    => GpuClockTuningMode.Offset,
            GpuClockTune.Overdrive => GpuClockTuningMode.Overdrive,
            GpuClockTune.ClockRange => GpuClockTuningMode.ClockRange,

            _ => throw new NotSupportedException(
                $"Tipo di tuning sconosciuto: {tune.GetType().Name}")
        };

        if (requestedMode != supportedMode)
            return false;
        return true;
    }


    public void ApplyFanCurve(FanCurve fanCurve)
    {
        if (Program.DaemonSession is null)
            return;
        
        Program.DaemonSession.Client.GpuApplyFanCurve(new FancurveSetRequest()
        {
            GpuId = DevicePciAddress,
            CurveJson = fanCurve.ToJson()
        });
        
    }
    
    #endregion
    

    public GpuViewModel(IGpu nvmlGpu)
    {
        _gpu = nvmlGpu;
    }

    public void StartUpdating()
    {
        _updateCts = new CancellationTokenSource();
        
        _runningUpdateTask = Task.Run(async () => await UpdateLoopAsync(_updateCts.Token));
    }

    public void StopUpdating()
    {
        _updateCts?.Cancel();
        Log.Information("Stopped update thread for GPU: {gpuName}",Name);
        
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting update thread for GPU: {gpuName}, poll delay: {pollDelay}s",Name,Program.LoadedSettings.SelectedUpdateTimeoutSeconds);
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(Program.LoadedSettings.SelectedUpdateTimeoutSeconds));
        
        

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var metrics = await Task.Run(() => _gpu.GetMetrics(), cancellationToken);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LatestGpuMetrics = metrics;
                GpuMetricsUpdatedEvent?.Invoke(this,new GpuMetricsUpdatedEventArgs(LatestGpuMetrics));
            }); 
        }
    }

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
    

    public void Dispose()
    {
        _updateCts?.Dispose();
        _runningUpdateTask?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class GpuMetricsUpdatedEventArgs(GpuMetrics metrics) : EventArgs
{
    public GpuMetrics NewMetrics { get; private set; } = metrics;
}