using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Serilog;
using sspreaNvidiaControl.Models.Exceptions;
using sspreaNvidiaControl.ViewModels;

namespace sspreaNvidiaControl.Models;


public partial class OcProfile : ObservableObject
{
    public OcProfile(string name,uint gpuClockOffset, uint memClockOffset, uint powerLimitMw, FanCurve? fanCurve)
    {
        Name = name;
        GpuClockOffset = gpuClockOffset;
        MemClockOffset = memClockOffset;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurve != null ? fanCurve.Name : "";
    }

    [JsonConstructor]
    public OcProfile(string name,uint gpuClockOffset, uint memClockOffset, uint powerLimitMw, string fanCurveName)
    {
        Name = name;
        GpuClockOffset = gpuClockOffset;
        MemClockOffset = memClockOffset;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurveName;
    }

    [ObservableProperty] private string _name;
    [ObservableProperty] private uint _gpuClockOffset;

    [ObservableProperty] private uint _memClockOffset;
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
            
            if (GpuClockOffset > 0)
                success &= targetGpu.SetCoreClockOffset((int)GpuClockOffset);
            
            if (MemClockOffset > 0)
                success &= targetGpu.SetMemoryClockOffset((int)GpuClockOffset);

            if (PowerLimitMw > 0)
                success &= targetGpu.SetPowerLimit((int)PowerLimitMw);
                    

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