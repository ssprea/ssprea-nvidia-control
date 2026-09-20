using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ReactiveUI;
using Serilog;
using sspreaNvidiaControl.Models;
using sspreaNvidiaControl.Utils;
using sspreaNvidiaControl.Views.SettingsPages;

namespace sspreaNvidiaControl.ViewModels;

public partial class SettingsMainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, object> CloseSettingsCommand { get; }

    public ObservableCollection<string> AvailableGuiSettings { get; private set; }
    public static ObservableCollection<string> AvailableLocales => new(["it-IT", "en-US", "System"]);
    [ObservableProperty] private ObservableCollection<string> _availableThemes = new(["Dark"]);
    
    // [ObservableProperty] public string selectedSettingCategory;
    [ObservableProperty] private int _selectedSettingCategoryIndex = 0;
    [ObservableProperty] private Control _currentSettingCategoryContent;
    [ObservableProperty] private Settings _currentEditingSettings;
    [ObservableProperty] private UserTheme _currentEditingUserTheme;

    [ObservableProperty] private int _themeSelectorIndex;
    [ObservableProperty] private bool _selectedThemeIsCustom;
    
   
    
    public SettingsMainWindowViewModel()
    {
        CurrentEditingSettings = new Settings(Program.LoadedSettings);
        
        CurrentSettingCategoryContent = new GuiSettingsPage();

        CurrentEditingUserTheme = new UserTheme();
        
        CloseSettingsCommand = ReactiveCommand.Create((() => new object()));

        
        AvailableGuiSettings = new ObservableCollection<string>();

        AvailableGuiSettings.Add("Default");
        
        //read all directories in subfolder Guis, which are the available guis
        AvailableGuiSettings.AddRange(Directory.GetDirectories($"{Program.DefaultDataPath}/Guis").Select(Path.GetFileName).Where(x => x is not null).Select(x => x!));
        
        //same thing but with embedded assets
        AvailableGuiSettings.Add(AvaloniaAssetsUtils.GetAvailableEmbeddedAssetsGuis());
        
        //read available custom themes
        if (Program.ThemesService is not null)
            AvailableThemes.AddRange(Program.ThemesService.LoadedUserThemes.Select(theme => theme.Name));
        
        UpdateCustomThemeCheck(CurrentEditingSettings.SelectedTheme);
        
        
    }

    public async Task SaveSettingsAsync()
    {
        Program.LoadedSettings = CurrentEditingSettings;
        await File.WriteAllTextAsync(Program.SettingsFilePath,Program.LoadedSettings.ToJson());
        Lang.Resources.Culture = new CultureInfo(Program.LoadedSettings.SelectedLocale);
        WindowsManager.ApplyMainWindowCustomGui();
        
        //enable gui service if option is set
        if (CurrentEditingSettings.BehaviourStartGuiAtBoot)
            Systemd.EnableUserService("snvctl-gui.service");
        else
        {
            Systemd.DisableUserService("snvctl-gui.service");
        }
        
        //apply theme
        Program.ThemesService?.Apply(Program.LoadedSettings.SelectedTheme);

    }

    private void UpdateLocalThemeList()
    {
        var selectedBefore = ThemeSelectorIndex;
        AvailableThemes.Clear();
        AvailableThemes.AddRange(["Dark"]);

        if (Program.ThemesService is null)
            return;
        
        AvailableThemes.AddRange(Program.ThemesService.LoadedUserThemes.Select(theme => theme.Name));
        
        ThemeSelectorIndex = selectedBefore;
    }
    
    partial void OnSelectedSettingCategoryIndexChanged(int value)
    {
        Log.Debug("Loading settings page: {pageIdx}",value);
        
        switch (value)
        {
            case 0:
                CurrentSettingCategoryContent = new GuiSettingsPage();
                break;
            case 1:
                CurrentSettingCategoryContent = new ThemesSettingsPage();
                break;
            case 2:
                CurrentSettingCategoryContent = new ValuesSettingsPage();
                break;
            case 3:
                CurrentSettingCategoryContent = new BehaviourSettingsPage();
                break;
            default:
                CurrentSettingCategoryContent = new TextBlock() { Text = "Invalid category." };
                break;
        }
    }


    partial void OnThemeSelectorIndexChanged(int value)
    {
        
        var theme = AvailableThemes[value];
        UpdateCustomThemeCheck(theme);
        
    }

    private void UpdateCustomThemeCheck(string theme)
    {
        
        
        Console.WriteLine(theme);
        switch (theme)
        {
            case "System":
            case "Light":
            case "Dark":
                SelectedThemeIsCustom = false;
                break;
            
            default:
                SelectedThemeIsCustom = true;
                break;
        }
    }

    public async Task DeleteSelectedUserTheme()
    {
        if (Program.ThemesService is null)
            return;

        await Program.ThemesService.DeleteUserThemesAndSaveToFileAsync(CurrentEditingUserTheme.Name);
        UpdateLocalThemeList();
    }
    
    public async Task SaveNewUserThemeAsync()
    {
        if (Program.ThemesService is null)
            return;

        await Program.ThemesService.AddNewUserThemeAndSaveToFileAsync(CurrentEditingUserTheme);
        UpdateLocalThemeList();
        
    }

    public void EditSelectedTheme()
    {
        if (Program.ThemesService is null)
            return;

        var theme = Program.ThemesService.GetUserTheme(CurrentEditingSettings.SelectedTheme);

        if (theme is null)
        {
            Log.Error("Could not find theme {themeName}", theme.Name);
            return;
        }
        
        CurrentEditingUserTheme = theme;
        
    }
    // partial void OnCurrentEditingUserThemeChanged(UserTheme value)
    // {
    //     //update UserThemeColor collection
    //     CurrentUserThemeEditingColors =
    //         new ObservableCollection<UserThemeColor>(value.Colors.Select(x => new UserThemeColor()
    //             { Key = x.Key, Value = x.Value }));
    // }
    
    

    // private void UpdateSettingsContent(Control content)
    // {
    //     SettingsCategoryChildren.Clear();
    //     SettingsCategoryChildren.Add(content);
    // }
    
    // public async Task SaveGuiSettingsAsync()
    // {
    //     await File.WriteAllTextAsync($"{Program.DefaultDataPath}/SelectedGui.txt", SelectedGuiSetting);
    //     WindowsManager.ApplyMainWindowCustomGui();
    // }
    //
    // public async Task SaveLocaleSettingsAsync()
    // {
    //     await File.WriteAllTextAsync($"{Program.DefaultDataPath}/SelectedLocale.txt", SelectedLocale);
    //     Lang.Resources.Culture = new CultureInfo(SelectedLocale);
    // }

    // public async Task ApplyMainWindowGui()
    // {
    //     await SaveGuiSettingsAsync();
    // }


}