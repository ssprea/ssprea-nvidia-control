using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.NVML.NvmlTypes;
using ssprea_nvidia_control.ViewModels;
using Newtonsoft.Json;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Models;

[ObservableObject]
public partial class OcProfile
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

    public bool Apply(NvmlGpu targetGpu)
    {
        try
        {
            var r1 = targetGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0,
                (int)GpuClockOffset);
            var r2 = targetGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0,
                (int)MemClockOffset);
            var r3 = targetGpu.SetPowerLimit(PowerLimitMw);

            if (FanCurve != null)
                targetGpu.ApplyFanCurve(FanCurve);

            Console.WriteLine(r1.ToString() + r2 + r3);
            return r1 == NvmlReturnCode.NVML_SUCCESS && r2 == NvmlReturnCode.NVML_SUCCESS &&
                   r3 == NvmlReturnCode.NVML_SUCCESS;
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