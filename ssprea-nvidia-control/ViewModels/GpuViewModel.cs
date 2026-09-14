using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GpuSSharp.Types;
using Serilog;
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

    private bool SetCoreClockOffset(int clockOffsetMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-c {clockOffsetMhz}", DevicePciAddress) is not null;
    
    private bool SetMemoryClockOffset(int clockOffsetMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-m {clockOffsetMhz}", DevicePciAddress) is not null;
    
    private bool SetCoreClockRange(int clockMinMhz, int clockMaxMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-c {clockMinMhz}:{clockMaxMhz}", DevicePciAddress) is not null;
    
    private bool SetMemoryClockRange(int clockMinMhz, int clockMaxMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-m {clockMinMhz}:{clockMaxMhz}", DevicePciAddress) is not null;
    
    
    public bool SetPowerLimit(int limitMw) =>
        SnvctlCliTool.RunSudoCliCommand($"-p {limitMw}", DevicePciAddress) is not null;
    
    public bool ApplyAutoSpeedToAllFans() => 
        SnvctlCliTool.RunSudoCliCommand($"-afs", DevicePciAddress) is not null;
    
    public bool ApplySpeedToAllFans(uint speed) =>
        SnvctlCliTool.RunSudoCliCommand($"-fs {speed}", DevicePciAddress) is not null;

    public bool ApplyCoreClockTune(GpuClockTune tune)
    {
        if (IsTuneValid(tune, Capabilities.CoreClockTuningMode))
        {
            switch (tune)
            {
                case GpuClockTune.ClockRange range:
                    return SetCoreClockRange((int)range.MinMhz, (int)range.MaxMhz);
                    

                case GpuClockTune.Offset offset:
                    return SetCoreClockOffset(offset.OffsetMhz);
                    
                

                default:
                    throw new NotSupportedException();
            }
        }

        return false;
    }
    
    public bool ApplyMemClockTune(GpuClockTune tune)
    {
        if (IsTuneValid(tune, Capabilities.MemoryClockTuningMode))
        {
            switch (tune)
            {
                case GpuClockTune.ClockRange range:
                    return SetMemoryClockRange((int)range.MinMhz, (int)range.MaxMhz);
                    

                case GpuClockTune.Offset offset:
                    return SetMemoryClockOffset(offset.OffsetMhz);
                    
                

                default:
                    throw new NotSupportedException();
            }
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
    

    public void ApplyFanCurve(FanCurve fanCurve) =>
        SnvctlCliTool.RunFanProcess(fanCurve, DevicePciAddress);
    
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