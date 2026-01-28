using System;
using Newtonsoft.Json;

namespace ssprea_nvidia_control.Models;

public class Settings
{
    public string SelectedGui { get; set; }
    public string SelectedLocale { get; set; }
    public double SelectedUpdateTimeoutSeconds { get; set; }

    [JsonIgnore]
    public TimeSpan SelectedUpdateTimeout => TimeSpan.FromSeconds(SelectedUpdateTimeoutSeconds);

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
        };
    }
}