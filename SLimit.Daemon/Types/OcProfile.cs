using GpuSSharp.Types;
using Newtonsoft.Json;
using SLimit.Daemon.Types.Converters;

namespace SLimit.Daemon.Types;

public class OcProfile
{
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune,int memVoltageOffsetMv,int coreVoltageOffsetMv, uint powerLimitMw, FanCurve? fanCurve)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurve != null ? fanCurve.Name : "";
    }

    [JsonConstructor]
    public OcProfile(string name,GpuClockTune gpuClockTune, GpuClockTune memClockTune,int memVoltageOffsetMv,int coreVoltageOffsetMv, uint powerLimitMw, string fanCurveName)
    {
        Name = name;
        GpuClockTune = gpuClockTune;
        MemClockTune = memClockTune;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurveName;
        CoreVoltageOffsetMv = coreVoltageOffsetMv;
        MemVoltageOffsetMv = memVoltageOffsetMv;
    }
    public string Name { get; set; }
    public GpuClockTune GpuClockTune { get; set; }
    public GpuClockTune MemClockTune { get; set; }
    //public uint SmClockOffset { get; set; }  = 0;
    public uint PowerLimitMw { get; set; }
    public int CoreVoltageOffsetMv { get; set; }
    public int MemVoltageOffsetMv  { get; set; }
    
    // [JsonIgnore]
    // public FanCurve? FanCurve => String.IsNullOrEmpty(_fanCurveName) ? null : MainWindowViewModel.FanCurvesList.First(x => x.Name == _fanCurveName).BaseFanCurve;


    [JsonProperty("fanCurveName")]
    private string _fanCurveName;
    
    

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static OcProfile? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<OcProfile>(json,new JsonSerializerSettings(){Converters = [new GpuClockTuneConverter()]});
    }
}