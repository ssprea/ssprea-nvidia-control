using System;
using Newtonsoft.Json;

namespace ssprea_nvidia_control.Models;

public class Settings
{
    public string SelectedGui { get; set; } =  "Default";
    public string SelectedLocale { get; set; } =  "System";
    public double SelectedUpdateTimeoutSeconds { get; set; } = 0.5;

    public bool Behaviour_StartGuiAtBoot { get; set; } = false;
    public bool Behaviour_StartGuiInTray { get; set; } = false;

    [JsonIgnore]
    public TimeSpan SelectedUpdateTimeout => TimeSpan.FromSeconds(SelectedUpdateTimeoutSeconds);
    
    
    public Settings() {}
    
    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">Object to clone</param>
    public Settings(Settings source)
    {
        SelectedGui = source.SelectedGui;
        SelectedLocale = source.SelectedLocale;
        SelectedUpdateTimeoutSeconds = source.SelectedUpdateTimeoutSeconds;
        Behaviour_StartGuiAtBoot = source.Behaviour_StartGuiAtBoot;
        Behaviour_StartGuiInTray = source.Behaviour_StartGuiInTray;
    }
    
    
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
    
    public static Settings? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Settings>(json);
    }

    

    public static Settings Default()
    {
        return new Settings
        {
            SelectedGui = "Default",
            SelectedLocale = "System",
            SelectedUpdateTimeoutSeconds = 0.5,
            Behaviour_StartGuiAtBoot = false,
            Behaviour_StartGuiInTray =  false,
        };
    }
}