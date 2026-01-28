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
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Utils;
using ssprea_nvidia_control.Views.SettingsPages;

namespace ssprea_nvidia_control.ViewModels;

public partial class SettingsMainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, object?> CloseSettingsCommand { get; }

    public ObservableCollection<string> AvailableGuiSettings { get; private set; }
    public static ObservableCollection<string> AvailableLocales => new(["it-IT", "en-US", "System"]);
    
    // [ObservableProperty] public string selectedSettingCategory;
    [ObservableProperty] private int _selectedSettingCategoryIndex = 0;
    [ObservableProperty] private Control _currentSettingCategoryContent;
    [ObservableProperty] private Settings _currentEditingSettings;
    
    public SettingsMainWindowViewModel()
    {
        CurrentEditingSettings = new Settings()
        {
            SelectedGui = Program.LoadedSettings.SelectedGui, 
            SelectedLocale = Program.LoadedSettings.SelectedLocale,
            SelectedUpdateTimeoutSeconds = Program.LoadedSettings.SelectedUpdateTimeoutSeconds
        };
        CurrentSettingCategoryContent = new GuiSettingsPage();
        
        
        CloseSettingsCommand = ReactiveCommand.Create((() => new object()));

        
        AvailableGuiSettings = new ObservableCollection<string>();

        AvailableGuiSettings.Add("Default");
        
        //read all directories in subfolder Guis, which are the available guis
        AvailableGuiSettings.AddRange(Directory.GetDirectories($"{Program.DefaultDataPath}/Guis").Select(Path.GetFileName).Where(x => x is not null).Select(x => x!));
        
        //same thing but with embedded assets
        AvailableGuiSettings.Add(AvaloniaAssetsUtils.GetAvailableEmbeddedAssetsGuis());
        
    }

    public async Task SaveSettingsAsync()
    {
        Program.LoadedSettings = CurrentEditingSettings;
        await File.WriteAllTextAsync(Program.SettingsFilePath,Program.LoadedSettings.ToJson());
        Lang.Resources.Culture = new CultureInfo(Program.LoadedSettings.SelectedLocale);
        WindowsManager.ApplyMainWindowCustomGui();
    }
    
    partial void OnSelectedSettingCategoryIndexChanged(int value)
    {
        Log.Debug("Loading settings page: "+value);
        
        switch (value)
        {
            case 0:
                CurrentSettingCategoryContent = new GuiSettingsPage();
                break;
            case 1:
                CurrentSettingCategoryContent = new ValuesSettingsPage();
                break;
            default:
                CurrentSettingCategoryContent = new TextBlock() { Text = "Invalid category." };
                break;
        }
    }

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