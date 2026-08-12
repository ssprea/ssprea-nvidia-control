using System;
using Newtonsoft.Json;

namespace sspreaNvidiaControl.Models;

public class Settings
{
    public string SelectedGui { get; set; } =  "Default";
    public string SelectedLocale { get; set; } =  "System";
    public double SelectedUpdateTimeoutSeconds { get; set; } = 0.5;

    [JsonProperty(PropertyName = "Behaviour_StartGuiAtBoot")]
    public bool BehaviourStartGuiAtBoot { get; set; }
    
    [JsonProperty(PropertyName = "Behaviour_StartGuiInTray")]
    public bool BehaviourStartGuiInTray { get; set; }

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
        BehaviourStartGuiAtBoot = source.BehaviourStartGuiAtBoot;
        BehaviourStartGuiInTray = source.BehaviourStartGuiInTray;
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
            BehaviourStartGuiAtBoot = false,
            BehaviourStartGuiInTray =  false,
        };
    }
}