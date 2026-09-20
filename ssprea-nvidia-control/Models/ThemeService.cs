using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using LiveChartsCore.Themes;
using Newtonsoft.Json;
using Serilog;

namespace sspreaNvidiaControl.Models;

public class ThemeService
{
    private Application _currentApplication;

    private readonly string _themesFilePath = Path.Combine(Program.DefaultDataPath, "themes.json");
    
    public ObservableCollection<UserTheme> LoadedUserThemes { get; private set; }
    
    private static readonly ThemeVariant CustomDark =
        new("SLimitCustomDark", ThemeVariant.Dark);

    private static readonly ThemeVariant CustomLight =
        new("SLimitCustomLight", ThemeVariant.Light);
    
    public ThemeService()
    {
        if (Application.Current is null)
            throw new Exception("Application.Current is null");
        
        _currentApplication = Application.Current;
        LoadedUserThemes = new();
        
        

    }

    public void Apply(string themeName)
    {
        
        switch (themeName) //handle builtin themes
        {
            case "Light":
                _currentApplication.RequestedThemeVariant = ThemeVariant.Light;
                return;
            case "Dark":
                _currentApplication.RequestedThemeVariant = ThemeVariant.Dark;
                return;
            case "System":
                _currentApplication.RequestedThemeVariant = ThemeVariant.Default;
                return;
        }
        
        
        var theme = LoadedUserThemes.FirstOrDefault(x => x.Name == themeName);
        if (theme is null)
        {
            Log.Error("Could not find theme {themeName}", themeName);
            return;
        }

        var palette = new ResourceDictionary();
        
        foreach (var userColor in theme.Colors)
        {
            if (!_currentApplication.TryGetResource(userColor.Key, theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light, out _))
            {
                Log.Warning("Key '{key}' not found in app theme. Skipping. ",userColor.Key);
                continue;
            }

            // if (!Color.TryParse(userColor.Value, out var color))
            // {
            //     Log.Warning("Invalid value found while loading custom theme at Key: {key}, Value: {value} . Skipping.", userColor.Key, userColor.Value);
            //     continue;
            // }
            
            palette.Add(userColor.Key,userColor.Value);
        }

        _currentApplication.Resources.ThemeDictionaries.Remove(theme.IsDark ? CustomDark : CustomLight);
        
        if (!_currentApplication.Resources.ThemeDictionaries.ContainsKey(theme.IsDark ?  CustomDark : CustomLight))
            _currentApplication.Resources.ThemeDictionaries.Add(theme.IsDark ?  CustomDark : CustomLight, palette);

        _currentApplication.RequestedThemeVariant = theme.IsDark ? CustomDark : CustomLight;

    }

    
    
    public ObservableCollection<UserThemeColor> GetDefaultThemeColors(bool darkTheme)
    {
        var colors = new ObservableCollection<UserThemeColor>();
        var theme =
            (IResourceDictionary)_currentApplication.Resources.ThemeDictionaries[darkTheme ? ThemeVariant.Dark : ThemeVariant.Light];

        foreach (var key in theme.Keys.OfType<string>().ToArray())
        {
            if (theme.TryGetResource(key, ThemeVariant.Dark, out var value)
                && value is Color color)
            {
                colors.Add(new UserThemeColor(){Key = key, Value = color}) ;
                // Console.WriteLine(color + " -- "+$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
            }
        }

        return colors;
    }

    public void LoadUserThemesFromJson()
    {
        if (!File.Exists(_themesFilePath))
        {
            Log.Warning("Could not find UserThemes file at {filePath}, it will be created when you save a new theme. Skipping loading.", _themesFilePath);
            LoadedUserThemes = new ObservableCollection<UserTheme>();
            LoadedUserThemes.Insert(0,new UserTheme() { Name = "Dark", IsDark = true, Colors = new() });
            return;
        }

        var json = File.ReadAllText(_themesFilePath);
        var parsed = JsonConvert.DeserializeObject<ObservableCollection<UserTheme>>(json);

        if (parsed is null)
        {
            Log.Warning("Couldn't load file {filePath}, file is corrupt", _themesFilePath);
            LoadedUserThemes = new ObservableCollection<UserTheme>();
            LoadedUserThemes.Insert(0,new UserTheme() { Name = "Dark", IsDark = true, Colors = new() });
            return;
        }

        parsed.Insert(0,new UserTheme() { Name = "Dark", IsDark = true, Colors = new() });
        LoadedUserThemes = parsed;
    }

    public async Task DeleteUserThemesAndSaveToFileAsync(string themeName)
    {
        var theme = LoadedUserThemes.FirstOrDefault(x => x.Name == themeName);
        if (theme is null)
            return;
        
        LoadedUserThemes.Remove(theme);
        await SaveUserThemesFileAsync();
    }
    
    public async Task AddNewUserThemeAndSaveToFileAsync(UserTheme newTheme)
    {
        //rename if reserved name used
        switch (newTheme.Name)
        {
            case "System":
            case "Dark":
            case "Light":
                newTheme.Name += "-Custom";
                Log.Warning("Trying to save custom theme with reserved name, renaming to {newName}", newTheme.Name);
                break;
        }
        
        //validate unique name
        if (LoadedUserThemes.FirstOrDefault(x => x.Name == newTheme.Name) is UserTheme existingTheme)
        {
            Log.Warning("Theme {name} already exists! Overwriting.", newTheme.Name);
            // Console.WriteLine(LoadedUserThemes.Remove(existingTheme));
            existingTheme.UpdateTheme(newTheme);
        }
        else
            LoadedUserThemes.Add(newTheme);
        
        await SaveUserThemesFileAsync();
    }

    public UserTheme? GetUserTheme(string themeName)
    {
        return  LoadedUserThemes.FirstOrDefault(x => x.Name == themeName);
    }
    
    private async Task SaveUserThemesFileAsync()
    {
        
        var listToSave = new ObservableCollection<UserTheme>();
        foreach (var userTheme in LoadedUserThemes)
        {
            if (userTheme.Name is "Dark" or "Light" or "System")
                continue;
            listToSave.Add(userTheme);
        }
        
        await File.WriteAllTextAsync(_themesFilePath, JsonConvert.SerializeObject(listToSave, Formatting.Indented));
    }
    
}