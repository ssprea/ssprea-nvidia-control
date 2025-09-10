using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NvmlSharp;
using NvmlSharp.NvmlTypes;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Models;

public class GpuNvidia : INotifyPropertyChanged, IGpu
{
    public GpuNvidia(NvmlGpu baseGpu, TimeSpan updateDelay)
    {
        _nvmlGpu = baseGpu;
        _updateDelay = updateDelay;
        
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(500);
                UpdateProperties();
            }
        });
    }
    
    
    private readonly NvmlGpu _nvmlGpu;
    private readonly TimeSpan _updateDelay;
    
    public IReadOnlyList<NvmlGpuFan> FansList => _nvmlGpu.FansList;

    public FanCurve? AppliedFanCurve { get; private set; }
    
    
    public uint DeviceIndex => _nvmlGpu.DeviceIndex;
    public uint GpuTemperature => _nvmlGpu.GetTemperature().Item2;
    public uint GpuPowerUsage => _nvmlGpu.GetPowerUsage().Item2;
    public double GpuPowerUsageW => GpuPowerUsage/1000f;
    public string GpuPowerUsageWFormatted => GpuPowerUsageW.ToString("0.00");

    public NvmlPStates GpuPState => _nvmlGpu.GetPState().Item2;

    public uint GpuClockCurrent => _nvmlGpu.GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS).Item2;
    public uint MemClockCurrent => _nvmlGpu.GetCurrentClock(NvmlClockType.NVML_CLOCK_MEM).Item2;
    public uint SmClockCurrent => _nvmlGpu.GetCurrentClock(NvmlClockType.NVML_CLOCK_SM).Item2;
    public uint VideoClockCurrent => _nvmlGpu.GetCurrentClock(NvmlClockType.NVML_CLOCK_VIDEO).Item2;

    // public IImmutableSolidColorBrush TemperatureIndicatorColorBrush =>
    //     GpuTemperature < TemperatureThresholdThrottle ? Brushes.White : (GpuTemperature < TemperatureThresholdSlowdown ? Brushes.Orange : Brushes.Red  );
    
    
    public uint PowerLimitCurrentMw => _nvmlGpu.GetPowerLimitCurrent().Item2;
    public uint PowerLimitMinMw => _nvmlGpu.GetPowerLimitConstraints().Item2;
    public uint PowerLimitMaxMw => _nvmlGpu.GetPowerLimitConstraints().Item3;
    public uint PowerLimitDefaultMw => _nvmlGpu.GetPowerLimitDefault().Item2;
    
    public double PowerLimitCurrentW => _nvmlGpu.GetPowerLimitCurrent().Item2/1000d;
    public double PowerLimitMinW => _nvmlGpu.GetPowerLimitConstraints().Item2/1000d;
    public double PowerLimitMaxW => _nvmlGpu.GetPowerLimitConstraints().Item3/1000d;
    public double PowerLimitDefaultW => _nvmlGpu.GetPowerLimitDefault().Item2/1000d;
    
    public ulong MemoryTotal => _nvmlGpu.GetMemoryUsage().Item2.Total;
    public ulong MemoryFree => _nvmlGpu.GetMemoryUsage().Item2.Free;
    public ulong MemoryUsed => _nvmlGpu.GetMemoryUsage().Item2.Used;

    public double MemoryTotalMB => MemoryTotal / 1000000f;
    public double MemoryFreeMB => MemoryFree / 1000000f;
    public double MemoryUsedMB => MemoryUsed / 1000000f;

    public string MemoryTotalMBFormatted => MemoryTotalMB.ToString("0.00");
    public string MemoryFreeMBFormatted => MemoryFreeMB.ToString("0.00");
    public string MemoryUsedMBFormatted => MemoryUsedMB.ToString("0");
    
    public NvmlUtilization GpuUtilization => _nvmlGpu.GetUtilization().Item2;
    
    public uint UtilizationCore => _nvmlGpu.GetUtilization().Item2.gpu;
    public uint UtilizationMemCtl => _nvmlGpu.GetUtilization().Item2.memory;
    

    public string MemoryUsageString => $"{MemoryUsed/1000000}MB/{MemoryTotal/1000000}MB (Free: {MemoryFree/1000000}MB)";
    

    public uint TemperatureThresholdShutdown => _nvmlGpu.GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SHUTDOWN).Item2;
    public uint TemperatureThresholdSlowdown => _nvmlGpu.GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SLOWDOWN).Item2;
    public uint TemperatureThresholdThrottle => _nvmlGpu.GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_GPU_MAX).Item2;
    
    public bool ApplyAutoSpeedToAllFans()
    {
        try
        {
            var result = RunSudoCliCommand($"-afs");
            return true;
        }catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
    
    public void ApplyFanCurve(FanCurve fanCurve)
    {
        AppliedFanCurve = fanCurve;
        try
        {
            RunFanProcess(fanCurve);
        }catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    public bool ApplySpeedToAllFans(uint speed)
    {
        // bool result = true;
        // foreach (var f in FansList)
        //     result &= f.SetSpeed(speed);
        // return result;

        try
        {
            var result = RunSudoCliCommand($"-fs {speed}");
            return true;
        }catch (SudoPasswordExpiredException)
        {
            throw;
        }
            
            
    }
    
    public NvmlReturnCode SetClockOffset(NvmlClockType clockType, NvmlPStates pState, int clockOffsetMhz)
    {
        // var clockOffset = new NvmlClockOffset_v1()
        // {
        //     Type = clockType,
        //     PState = pState,
        //     ClockOffsetMHz = clockOffsetMhz
        // };

        //return NvmlWrapper.nvmlDeviceSetClockOffsets(_handle, ref clockOffset);

        // Process? result = null;
            
        try
        {
            switch (clockType)
            {
                case NvmlClockType.NVML_CLOCK_GRAPHICS:
                    RunSudoCliCommand($"-c {clockOffsetMhz}");
                    break;
                case NvmlClockType.NVML_CLOCK_MEM:
                    RunSudoCliCommand($"-m {clockOffsetMhz}");
                    break;
            }
                
                
            Console.WriteLine(" set clock offset: ");

            return NvmlReturnCode.NVML_SUCCESS;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
    
    public NvmlReturnCode SetPowerLimit(uint limitMw)
    {
        try
        {
            var result = RunSudoCliCommand($"-p {limitMw}");
            Console.WriteLine(" set power limit: ");

            return NvmlReturnCode.NVML_SUCCESS;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
        // if (result.Item1 != 0)
        // {
        //     
        // }
            
        //return NvmlWrapper.nvmlDeviceSetPowerManagementLimit(_handle,limitMw);
    }
    
    private Process? RunSudoCliCommand(string args, string file="/usr/local/bin/snvctl",bool waitForExit = true)
        {
            if (SudoPasswordManager.CurrentPassword is not null && SudoPasswordManager.CurrentPassword.OperationCanceled)
            {
                SudoPasswordManager.CurrentPassword = null;
                return null;
            }
            
            if (SudoPasswordManager.CurrentPassword?.Password == null || SudoPasswordManager.CurrentPassword.IsExpired || !SudoPasswordManager.CurrentPassword.IsValid )
            {
                throw new SudoPasswordExpiredException("Sudo password is expired or invalid");
            }
            
            
            
            
            var psi = new ProcessStartInfo();
            psi.FileName = "/usr/bin/bash";
            psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" -g "+DeviceIndex+" "+args+"\"";
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
            var process = Process.Start(psi);
            
            
            process.StandardInput.Write(SudoPasswordManager.CurrentPassword.Password+"\n");
            if (waitForExit)
            {
                if (!process.WaitForExit(4000))
                    return null;
            }

            Console.WriteLine(process.Id);
            //var output = process.StandardOutput.ReadToEnd();
            
            return process;
        }

        private void RunFanProcess(FanCurve fanCurve)
        {
            if (Program.FanCurveProcess is not null)
                Program.FanCurveProcess.Kill();
            
            try
            {
                var tempPath = Program.DefaultDataPath + "/temp/fanCurve-" + fanCurve.Name.Replace(" ","_").Replace("/","_").Replace("\\","_").Replace(":","_") +
                               DateTime.Now.ToString("yyyyMMddHHmmss")+".json";
                File.WriteAllText(tempPath,JsonConvert.SerializeObject(fanCurve, Formatting.None));
                
                Program.FanCurveProcess = RunSudoCliCommand($"-fp {tempPath}",waitForExit:false);
            }catch (SudoPasswordExpiredException)
            {
                throw;
            }
        }
        
        private void UpdateProperties()
        {
            foreach (var p in GetType().GetProperties())
            { 
                OnPropertyChanged(p.Name);
            }
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