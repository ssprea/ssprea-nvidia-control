using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GpuSSharp.Types;
using Newtonsoft.Json;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Models;

public class MonitoredGpu : INotifyPropertyChanged
{
    private IGpu _baseGpu;

    public GpuVendor Vendor => _baseGpu.Vendor;

    public string PciAddress => _baseGpu.DevicePciAddress;
    
    public uint DeviceIndex => _baseGpu.DeviceIndex;
    public string Name => _baseGpu.Name;
    public uint GpuTemperature => (uint)_baseGpu.GpuTemperature;
    public uint GpuPowerUsage => _baseGpu.GpuPowerUsage;
    public double GpuPowerUsageW => _baseGpu.GpuPowerUsage / 1000f;
    public string GpuPowerUsageWFormatted=> GpuPowerUsageW.ToString("0.00");

    public GpuPState GpuPState => _baseGpu.GpuPState;

    public uint GpuClockCurrent => _baseGpu.GpuClockCurrent;
    public uint MemClockCurrent => _baseGpu.MemClockCurrent;
    public uint SmClockCurrent => _baseGpu.SmClockCurrent;
    public uint VideoClockCurrent=> _baseGpu.VideoClockCurrent;

    // public IImmutableSolidColorBrush TemperatureIndicatorColorBrush=> _baseGpu.
    //     GpuTemperature < TemperatureThresholdThrottle ? Brushes.White : (GpuTemperature < TemperatureThresholdSlowdown ? Brushes.Orange : Brushes.Red  );
    
    
    public uint PowerLimitCurrentMw=> _baseGpu.PowerLimitCurrentMw;
    public uint PowerLimitMinMw=> _baseGpu.PowerLimitMinMw;
    public uint PowerLimitMaxMw=> _baseGpu.PowerLimitMaxMw;
    public uint PowerLimitDefaultMw=> _baseGpu.PowerLimitDefaultMw;
    
    public double PowerLimitCurrentW=> _baseGpu.PowerLimitCurrentW;
    public double PowerLimitMinW=> _baseGpu.PowerLimitMinW;
    public double PowerLimitMaxW=> _baseGpu.PowerLimitMaxW;
    public double PowerLimitDefaultW => _baseGpu.PowerLimitDefaultW;
    
    public ulong MemoryTotal=> _baseGpu.MemoryTotal;
    public ulong MemoryFree=> _baseGpu.MemoryFree;
    public ulong MemoryUsed=> _baseGpu.MemoryUsed;

    public double MemoryTotalMB=> _baseGpu.MemoryTotalMB;
    public double MemoryFreeMB=> _baseGpu.MemoryFreeMB;
    public double MemoryUsedMB=> _baseGpu.MemoryUsedMB;

    public string MemoryTotalMBFormatted => MemoryTotalMB.ToString("0.00");
    public string MemoryFreeMBFormatted => MemoryFreeMB.ToString("0.00");
    public string MemoryUsedMBFormatted => MemoryUsedMB.ToString("0");


    public uint UtilizationCore => _baseGpu.UtilizationCore;
    public uint UtilizationMemCtl => _baseGpu.UtilizationMemCtl;
    

    public string MemoryUsageString => $"{MemoryUsed/1000000}MB/{MemoryTotal/1000000}MB (Free: {MemoryFree/1000000}MB)";
    

    public uint TemperatureThresholdShutdown=> _baseGpu.TemperatureThresholdShutdown;
    public uint TemperatureThresholdSlowdown=> _baseGpu.TemperatureThresholdSlowdown;
    public uint TemperatureThresholdThrottle=> _baseGpu.TemperatureThresholdThrottle;

    public uint Fan0SpeedPercent => _baseGpu.Fan0SpeedPercent;


    public FanCurve? AppliedFanCurve { get; private set; }

    

    
    
    public List<uint> GetFansIds()  => _baseGpu.GetFansIds();

    public MonitoredGpu(IGpu baseGpu, TimeSpan updateDelay)
    {
        _baseGpu = baseGpu;
        
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep((int)updateDelay.TotalMilliseconds);
                UpdateProperties();
            }
        });
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
        try
        {
            var result = RunSudoCliCommand($"-fs {speed}");
            return true;
        }catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

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

    public bool SetPowerLimit(uint limitMw)
    {
        try
        {
            var result = RunSudoCliCommand($"-p {limitMw}");
            Console.WriteLine(" set power limit: ");

            return true;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    public bool SetMemOffset(GpuPState pState, uint clockOffsetMhz)
    {
        try
        {
            RunSudoCliCommand($"-m {clockOffsetMhz}");
                
                
            Console.WriteLine(" set mem clock offset: ");
            return true;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
    
    public bool SetCoreOffset(GpuPState pState, uint clockOffsetMhz)
    {
        try
        {
            RunSudoCliCommand($"-c {clockOffsetMhz}");
                
                
            Console.WriteLine(" set core clock offset: ");
            return true;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
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
    
        private Process? RunSudoCliCommand(string args, string file="snvctl",bool waitForExit = true)
        {
            if (file == "snvctl" && Environment.GetEnvironmentVariable("SNVCTLCLITOOLPATH") is not null)
                file = Environment.GetEnvironmentVariable("SNVCTLCLITOOLPATH")!;
                
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
            psi.Arguments = $"-c \"/usr/bin/sudo -S "+file+" -g "+PciAddress+" "+args+"\"";
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
}