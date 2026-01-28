using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.NVML.NvmlTypes;

namespace ssprea_nvidia_control.NVML;

public class NvmlGpuFan : INotifyPropertyChanged
{
    public NvmlGpuFan(NvmlGpu parentGpu, uint fanId)
    {
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep((int)Program.LoadedSettings.SelectedUpdateTimeout.TotalMilliseconds);
                Updater();
            }
        });
        
        ParentGpu = parentGpu;
        FanId = fanId;
    }
    
    public NvmlGpu ParentGpu { get; private set; }
    public uint FanId { get; private set; }
    public uint TargetSpeed => ParentGpu.GetFanTargetSpeed(FanId).Item2;
    public uint CurrentSpeed => ParentGpu.GetFanCurrentSpeed(FanId).Item2;
    public string Name => "Fan"+FanId;

    private void Updater()
    {
        //Console.WriteLine("update");
        //OnPropertyChanged(nameof(GpuClockCurrent));
        foreach (var p in GetType().GetProperties())
        {
            OnPropertyChanged(p.Name);
        }
        
        // GpuClockCurrent = _nvmlGpu.GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS).Item2;
    }
    
    public bool SetSpeed(uint speed)
    {
        var r= ParentGpu.SetFanSpeed(FanId, speed);
        Console.WriteLine(r);
        return r == NvmlReturnCode.NVML_SUCCESS;
    }

    public bool SetPolicy(NvmlFanControlPolicy policy)
    {
        var r= ParentGpu.SetFanControlPolicy(FanId, policy);
        Log.Debug(r.ToString());
        return r == NvmlReturnCode.NVML_SUCCESS;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}