using System;
using System.Linq;
using Newtonsoft.Json;
using GpuSSharp;

namespace ssprea_nvidia_control_cli.Types;

public class OcProfile
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
    public string Name { get; set; }
    public uint GpuClockOffset { get; set; }
    public uint MemClockOffset { get; set; }
    //public uint SmClockOffset { get; set; }  = 0;
    public uint PowerLimitMw { get; set; }
    
    // [JsonIgnore]
    // public FanCurve? FanCurve => String.IsNullOrEmpty(_fanCurveName) ? null : MainWindowViewModel.FanCurvesList.First(x => x.Name == _fanCurveName).BaseFanCurve;


    [JsonProperty("fanCurveName")]
    private string _fanCurveName;

    // public bool Apply(NvmlGpu targetGpu)
    // {
    //     try
    //     {
    //         var r1 = targetGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0,
    //             (int)GpuClockOffset);
    //         var r2 = targetGpu.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0,
    //             (int)MemClockOffset);
    //         var r3 = targetGpu.SetPowerLimit(PowerLimitMw);
    //
    //         if (FanCurve != null)
    //             targetGpu.ApplyFanCurve(FanCurve);
    //
    //         Console.WriteLine(r1.ToString() + r2 + r3);
    //         return r1 == NvmlReturnCode.NVML_SUCCESS && r2 == NvmlReturnCode.NVML_SUCCESS &&
    //                r3 == NvmlReturnCode.NVML_SUCCESS;
    //     }
    //     catch (SudoPasswordExpiredException)
    //     {
    //         throw;
    //     }
    // }
    

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static OcProfile? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<OcProfile>(json);
    }
}