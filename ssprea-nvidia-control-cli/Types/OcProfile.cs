using System;
using System.Linq;
using GpuSSharp.Types;
using Newtonsoft.Json;
using sspreaNvidiaControl.JsonConverters;

namespace sspreaNvidiaControlCli.Types;

public class OcProfile
{
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune, uint powerLimitMw, FanCurve? fanCurve)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurve != null ? fanCurve.Name : "";
    }

    [JsonConstructor]
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune, uint powerLimitMw, string fanCurveName)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurveName;
    }
    public string Name { get; set; }
    public GpuClockTune GpuClockTune { get; set; }
    public GpuClockTune MemClockTune { get; set; }
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
        return JsonConvert.DeserializeObject<OcProfile>(json,new JsonSerializerSettings(){Converters = [new GpuClockTuneConverter()]});
    }
}