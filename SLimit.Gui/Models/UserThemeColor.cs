using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SLimit.Gui.Models;

public partial class UserThemeColor : ObservableObject
{
    [ObservableProperty]
    public partial string Key { get; set; }

    [ObservableProperty]
    public partial Color Value { get; set; }

    public UserThemeColor(string key, Color color)
    {
        Key = key;
        Value = color;
    }
}