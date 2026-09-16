using Newtonsoft.Json;
using Serilog;
using SLimit.Daemon.Types;
using SLimit.Daemon.Types.Converters;

namespace SLimit.Daemon;

public static class StartupProfiles
{
    private const string ProfilesFolderPath = "/etc/snvctl/startup/";

    static StartupProfiles()
    {
        CreateFolders();
    }
    
    static void CreateFolders()
    {
        if (!Directory.Exists(ProfilesFolderPath))
            Directory.CreateDirectory(ProfilesFolderPath);
    }

    public static OcProfile? GetStartupProfileById(string gpuId)
    {
        var currentGpuFolder = Path.Combine(ProfilesFolderPath, gpuId);

        if (!Directory.Exists(currentGpuFolder))
            return null;
        
        if (!File.Exists(Path.Combine(currentGpuFolder, "profile.json")))
            return null;
        
        var ocProfile = JsonConvert.DeserializeObject<OcProfile>(File.ReadAllText(Path.Combine(currentGpuFolder, "profile.json")),converters: [new GpuClockTuneConverter()]);

        return ocProfile;
    }
    
    public static bool SaveStartupProfile(string gpuId, string ocProfileJson, string? fanCurveJson)
    {
        var currentGpuFolder = Path.Combine(ProfilesFolderPath, gpuId);
        Directory.CreateDirectory(currentGpuFolder);
        
        File.WriteAllText(Path.Combine(currentGpuFolder, "profile.json"), ocProfileJson);
        
        if (fanCurveJson != null)
            File.WriteAllText(Path.Combine(currentGpuFolder, "curve.json"), fanCurveJson);

        return true;
    }

    public static void DeleteStartupProfile(string gpuId)
    {
        var currentGpuFolder = Path.Combine(ProfilesFolderPath, gpuId);
        if (Directory.Exists(currentGpuFolder))
            Directory.Delete(currentGpuFolder, true);
    }

    public static void ApplyAllStartupProfiles()
    {
        if (!Directory.Exists(ProfilesFolderPath))
            return;

        foreach (var gpuFolder in Directory.GetDirectories(ProfilesFolderPath))
        {
            var gpuId =  Path.GetFileName(gpuFolder);
            var gpu = Program.GpuService?.GetGpuByPcieId(gpuId);
            

            if (gpu is null)
            {
                Log.Warning("Unknown GPU id : {gpuId} in startup profiles. Ignoring.", gpuId);
                continue;
            }
            
            if (!File.Exists(Path.Combine(gpuFolder, "profile.json")))
            {
                Log.Warning("GPU id: {gpuId} has startup profile folder but not profile. Ignoring.",gpuId);
                continue;
            }
            
            var profile = JsonConvert.DeserializeObject<OcProfile>(File.ReadAllText(Path.Combine(gpuFolder, "profile.json")), converters: [new GpuClockTuneConverter()]);

            if (profile is null)
            {
                Log.Warning("Invalid profile file for GPU {gpuId} startup profile. Ignoring. ",gpuId);
                continue;
            }

            //apply profile
            var success = true;
            success &= gpu.SetCoreTuning(profile.GpuClockTune);
            success &= gpu.SetMemTuning(profile.MemClockTune);
            success &= gpu.SetGpuPowerLimit(profile.PowerLimitMw);
            success &= gpu.SetCoreVoltageOffset(profile.CoreVoltageOffsetMv);
            
            if (success)
                Log.Information("Successfully applied startup profile to GPU: {gpuId}. \n Core: {coreTune} \n Memory: {memTune} \n PowerLimit mW: {plmw} \n Voltage offset mV: {vOff}",gpuId,profile.GpuClockTune,profile.MemClockTune,profile.PowerLimitMw,profile.CoreVoltageOffsetMv);
            else
                Log.Error("Could not apply startup profile to GPU {gpuId}. Ignoring.",gpuId);
            //check fan curve

            if (!File.Exists(Path.Combine(gpuFolder, "curve.json")))
            {
                Log.Information("Fan curve not found for GPU {gpuId} startup profile. Done with this gpu.",gpuId);
                continue;
            }
            
            var curve = JsonConvert.DeserializeObject<GpuSSharp.Libs.Nvml.FanCurve>(File.ReadAllText(Path.Combine(gpuFolder, "curve.json")));

            if (curve is null)
            {
                Log.Warning("Invalid curve file for GPU {gpuId} startup profile. Ignoring. ",gpuId);
                continue;
            }

            FanCurves.StartNewFanThread(500, gpu, curve);

        }
    }
}