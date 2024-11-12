using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ssprea_nvidia_control.Models;

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
            Console.WriteLine("File not found: " + _path + ", it will be created when you save a new profile.");
            return;
        }

        try
        {
            var deserialized = JsonConvert.DeserializeObject<ObservableCollection<OcProfile>>(File.ReadAllText(_path));
            if (deserialized == null)
            {
                Console.WriteLine("Error loading file " + _path);
                return;
            }
            
            LoadedProfiles = deserialized;
            Console.WriteLine("Successfully loaded "+_path);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine("Error loading file " + _path + ":\n" + ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Invalid "+ _path + " file:\n" + ex);
            
        }
    }
    
    public async Task LoadProfilesAsync()
    {
        if (!File.Exists(_path))
        {
            Console.WriteLine("File not found: " + _path + ", it will be created when you save a new profile.");
            return;
        }

        try
        {
            var deserialized = JsonConvert.DeserializeObject<ObservableCollection<OcProfile>>(await File.ReadAllTextAsync(_path));
            if (deserialized == null)
            {
                Console.WriteLine("Error loading file " + _path);
                return;
            }
            
            LoadedProfiles = deserialized;
            Console.WriteLine("Successfully loaded "+_path);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine("Error loading file " + _path + ":\n" + ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Invalid "+ _path + " file:\n" + ex);
            
        }
    }
    
    public async Task UpdateProfilesFileAsync()
    {
        await File.WriteAllTextAsync(_path, JsonConvert.SerializeObject(LoadedProfiles, Formatting.Indented));
    }
}