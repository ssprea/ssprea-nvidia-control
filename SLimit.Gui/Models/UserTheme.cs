using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Serilog;

namespace SLimit.Gui.Models;

public partial class UserTheme : ObservableObject
{
    [ObservableProperty] private string _name;

    [ObservableProperty] private bool _isDark;

    /// <summary>
    /// Contains overriden colors by theme, if IsDark is true, missing colors will be copied from default dark theme, otherwise from default light theme
    /// </summary>
    [ObservableProperty] private ObservableCollection<UserThemeColor> _colors;

    /// <summary>
    /// Loads an existing UserTheme
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isDark"></param>
    /// <param name="colors"></param>
    [JsonConstructor]
    public UserTheme(string name, bool isDark, ObservableCollection<UserThemeColor> colors)
    {
        Name = name;
        IsDark = isDark;
        Colors = colors;
    }
    
    /// <summary>
    /// Creates a new UserTheme instance with colors from default theme
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isDark"></param>
    public UserTheme(string name, bool isDark)
    {
        Name = name;
        IsDark = isDark;

        Colors = new ObservableCollection<UserThemeColor>();

        LoadColorsFromDefaultTheme();
    }

    public void LoadColorsFromDefaultTheme()
    {
        
        if (Program.ThemesService is null)
        {
            Colors = new ObservableCollection<UserThemeColor>();
            Log.Error("Could not read default theme colors, ThemesService is null.");
            return;
        }
        
        Colors = Program.ThemesService.GetDefaultThemeColors(IsDark);
        
    }

    /// <summary>
    /// Creates a new UserTheme instance with default values
    /// </summary>
    public UserTheme() : this("New Theme", true) { }

    public void UpdateTheme(UserTheme newTheme)
    {
        IsDark = newTheme.IsDark;
        Colors = newTheme.Colors;
    }
    
    
}