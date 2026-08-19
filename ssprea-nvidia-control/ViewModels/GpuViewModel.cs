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
    
    
    #endregion
    
    #region Setters

    public bool SetCoreClockOffset(int clockOffsetMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-c {clockOffsetMhz}", DevicePciAddress) is not null;
    
    public bool SetMemoryClockOffset(int clockOffsetMhz) =>
        SnvctlCliTool.RunSudoCliCommand($"-m {clockOffsetMhz}", DevicePciAddress) is not null;
    
    public bool SetPowerLimit(int limitMw) =>
        SnvctlCliTool.RunSudoCliCommand($"-p {limitMw}", DevicePciAddress) is not null;
    
    public bool ApplyAutoSpeedToAllFans() => 
        SnvctlCliTool.RunSudoCliCommand($"-afs", DevicePciAddress) is not null;
    
    public bool ApplySpeedToAllFans(uint speed) =>
        SnvctlCliTool.RunSudoCliCommand($"-fs {speed}", DevicePciAddress) is not null;

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