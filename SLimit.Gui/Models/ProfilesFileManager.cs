using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using GpuSSharp.Types;
using Newtonsoft.Json.Linq;
using Serilog;
using SLimit.Gui.JsonConverters;

namespace SLimit.Gui.Models;

public class ProfilesFileManager
{
    public ObservableCollection<OcProfile> LoadedProfiles { get; set; } = [];
    private readonly string _path;

    public ProfilesFileManager(string path)
    {
        _path = path;
        LoadProfiles();
    }

    void LoadProfiles()
    {
        if (!File.Exists(_path))
        {
            Log.Information("Profiles file not found at: {profileFilePath}, it will be created when you save a new profile.",_path);
            return;
        }

        try
        {
            var deserialized = JsonConvert.DeserializeObject<ObservableCollection<OcProfile>>(File.ReadAllText(_path),new JsonSerializerSettings(){Converters = { new GpuClockTuneConverter()}});
            if (deserialized == null)
            {
                Log.Warning("Error loading file {profileFilePath}" , _path);
                return;
            }
            
            //disabled because this can only be null if the users opens the new version with an old settings file.
            
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (deserialized.First().GpuClockTune is null || deserialized.First().MemClockTune is null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            {
                Log.Warning("Old Profiles file version detected, starting migration.");
                

                var backupPath = Path.GetDirectoryName(_path) + "/profiles.old";
                
                File.Copy(_path, backupPath);
                Log.Information("Created profiles file backup at {backupPath}", backupPath);
                
                LoadedProfiles = new();
                
                JArray profiles = JArray.Parse(File.ReadAllText(_path));
                foreach (JToken profile in profiles)
                {
                    var name =  (string?)profile["Name"];
                    var coreOffset = (int?)profile["GpuClockOffset"];
                    var memOffset = (int?)profile["MemClockOffset"];
                    var powerLimit = (uint?)profile["PowerLimitMw"];
                    var voltOffCore = (int?)profile["CoreVoltageOffsetMv"];
                    var voltOffMem = (int?)profile["MemVoltageOffsetMv"];
                    var fanCurve = (string?)profile["FanCurveName"];

                    
                    
                    if (name is null || coreOffset is null || memOffset is null || powerLimit is null || fanCurve is null)
                    {
                        Log.Warning("Migration failed! Profiles file is corrupt. Ignoring.");
                        return;
                    }
                        
                    LoadedProfiles.Add(new OcProfile(name,new GpuClockTune.Offset((int)coreOffset,GpuPState.GpuPstate0),
                                                    new GpuClockTune.Offset((int)memOffset,GpuPState.GpuPstate0),(uint)powerLimit,voltOffCore ?? 0, voltOffMem ?? 0,fanCurve));

                }
                UpdateProfilesFile();
                Log.Information("Profiles file successfully migrated.");
                return;
            }
            
            LoadedProfiles = deserialized;
            Log.Information("Successfully loaded {profileFilePath}",_path);
        }
        catch (ArgumentNullException ex)
        {
            Log.Warning("Error loading file {profileFilePath} :\n {exceptionMsg}",_path, ex);
        }
        catch (JsonException ex)
        {
            Log.Warning("Invalid {profileFilePath} file:\n" , ex);
            
        }
    }
    
    // private void CheckAndConvertLegacyOcProfilesJson()
    // {
    //     var migrationRequired = false;
    //     foreach (var p in OcProfilesList)
    //     {
    //         
    //         //disabled because this can only be null if the users opens the new version with an old settings file.
    //         
    //         
    //         // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    //         if (p.GpuClockTune is null || p.MemClockTune is null)
    //             // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    //         {
    //             migrationRequired = true;
    //             break;
    //         }
    //     }
    //
    //     if (!migrationRequired)
    //         return;
    //     
    //     //try migration
    //     
    // }
    
    // public async Task LoadProfilesAsync()
    // {
    //     if (!File.Exists(_path))
    //     {
    //         Console.WriteLine("File not found: " + _path + ", it will be created when you save a new profile.");
    //         return;
    //     }
    //
    //     try
    //     {
    //         var deserialized = JsonConvert.DeserializeObject<ObservableCollection<OcProfile>>(await File.ReadAllTextAsync(_path));
    //         if (deserialized == null)
    //         {
    //             Console.WriteLine("Error loading file " + _path);
    //             return;
    //         }
    //         
    //         LoadedProfiles = deserialized;
    //         Console.WriteLine("Successfully loaded "+_path);
    //     }
    //     catch (ArgumentNullException ex)
    //     {
    //         Console.WriteLine("Error loading file " + _path + ":\n" + ex);
    //     }
    //     catch (JsonException ex)
    //     {
    //         Console.WriteLine("Invalid "+ _path + " file:\n" + ex);
    //         
    //     }
    // }
    
    public async Task UpdateProfilesFileAsync()
    {
        await File.WriteAllTextAsync(_path, JsonConvert.SerializeObject(LoadedProfiles, Formatting.Indented));
    }
    
    private void UpdateProfilesFile()
    {
        File.WriteAllText(_path, JsonConvert.SerializeObject(LoadedProfiles, Formatting.Indented));
    }
}