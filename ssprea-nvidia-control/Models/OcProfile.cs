using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GpuSSharp.Types;
using Newtonsoft.Json;
using Serilog;
using sspreaNvidiaControl.Models.Exceptions;
using sspreaNvidiaControl.ViewModels;

namespace sspreaNvidiaControl.Models;


public partial class OcProfile : ObservableObject
{
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune, uint powerLimitMw, int coreVoltageOffsetMv,int memVoltageOffsetMv, FanCurve? fanCurve)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurve != null ? fanCurve.Name : "";
        CoreVoltageOffsetMv = coreVoltageOffsetMv;
        MemVoltageOffsetMv = memVoltageOffsetMv;
    }

    [JsonConstructor]
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune, uint powerLimitMw,int coreVoltageOffsetMv,int memVoltageOffsetMv, string fanCurveName)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurveName;
        CoreVoltageOffsetMv = coreVoltageOffsetMv;
        MemVoltageOffsetMv = memVoltageOffsetMv;
        
    }

    [ObservableProperty] private string _name;
    [ObservableProperty] private GpuClockTune _gpuClockTune;

    [ObservableProperty] private GpuClockTune _memClockTune;
    [ObservableProperty] private int _coreVoltageOffsetMv;
    [ObservableProperty] private int _memVoltageOffsetMv;
    //public uint SmClockOffset { get; set; }  = 0;
    [ObservableProperty] private uint _powerLimitMw;
    // [ObservableProperty] private double _powerLimitW = 0;
    //
    // partial void OnPowerLimitMwChanged(uint oldValue, uint newValue)
    // {
    //     PowerLimitW = PowerLimitMw / 1000f;
    // }
    
    [JsonIgnore]
    public FanCurve? FanCurve => String.IsNullOrEmpty(FanCurveName) ? null : MainWindowViewModel.FanCurvesList.First(x => x.Name == FanCurveName).BaseFanCurve;

    // partial void OnFanCurveNameChanged(string? oldValue, string? newValue)
    // {
    //     OnPropertyChanged(nameof(FanCurve));
    // }
    //
    [ObservableProperty]
    [JsonProperty("fanCurveName")]
    [JsonIgnore]
    private string _fanCurveName;

    public bool Apply(GpuViewModel targetGpu)
    {
        try
        {
            bool success = true;
            
            
            success &= targetGpu.ApplyCoreClockTune(GpuClockTune);
            
            
            success &= targetGpu.ApplyMemClockTune(MemClockTune);

            
            success &= targetGpu.SetPowerLimit((int)PowerLimitMw);
            
            if (targetGpu.Capabilities.GpuVoltageOffset)
                success &= targetGpu.SetCoreVoltageOffset(CoreVoltageOffsetMv);
                    

            if (FanCurve != null)
                targetGpu.ApplyFanCurve(FanCurve);

            Log.Debug("Applying profile: \" {profileName} \" , {status}!",Name,success ? "Success" : "Failure");
            return success;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
    

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static OcProfile? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<OcProfile>(json);
    }
}