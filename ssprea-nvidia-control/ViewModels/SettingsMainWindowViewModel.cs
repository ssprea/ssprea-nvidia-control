using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ReactiveUI;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Utils;

namespace ssprea_nvidia_control.ViewModels;

public partial class SettingsMainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, object?> CloseSettingsCommand { get; }

    public ObservableCollection<string> SettingCategories { get; private set; }
    public ObservableCollection<string> AvailableGuiSettings { get; private set; }
    public static ObservableCollection<string> AvailableLocales => new(["it-IT", "en-US", "System"]);
    
    // [ObservableProperty] public string selectedSettingCategory;
    [ObservableProperty] private string _selectedGuiSetting;
    [ObservableProperty] private string _selectedLocale = Program.SelectedLocale;
    
    public SettingsMainWindowViewModel()
    {
        CloseSettingsCommand = ReactiveCommand.Create((() => new object()));
        
        SettingCategories = new ObservableCollection<string>() {"GUI"};

        
        AvailableGuiSettings = new ObservableCollection<string>();

        AvailableGuiSettings.Add("Default");
        
        //read all directories in subfolder Guis, which are the available guis
        AvailableGuiSettings.AddRange(Directory.GetDirectories($"{Program.DefaultDataPath}/Guis").Select(Path.GetFileName).Where(x => x is not null).Select(x => x!));
        
        //same thing but with embedded assets
        AvailableGuiSettings.Add(AvaloniaAssetsUtils.GetAvailableEmbeddedAssetsGuis());
        

        SelectedGuiSetting = File.ReadAllText($"{Program.DefaultDataPath}/SelectedGui.txt").Trim();
        
    }

    public async Task SaveGuiSettingsAsync()
    {
        await File.WriteAllTextAsync($"{Program.DefaultDataPath}/SelectedGui.txt", SelectedGuiSetting);
        WindowsManager.ApplyMainWindowCustomGui();
    }
    
    public async Task SaveLocaleSettingsAsync()
    {
        await File.WriteAllTextAsync($"{Program.DefaultDataPath}/SelectedLocale.txt", SelectedLocale);
        Lang.Resources.Culture = new CultureInfo(SelectedLocale);
    }

    // public async Task ApplyMainWindowGui()
    // {
    //     await SaveGuiSettingsAsync();
    // }


}