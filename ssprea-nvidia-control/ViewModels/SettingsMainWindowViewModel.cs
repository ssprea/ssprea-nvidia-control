using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ReactiveUI;
using ssprea_nvidia_control.Models;

namespace ssprea_nvidia_control.ViewModels;

public partial class SettingsMainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, object?> CloseSettingsCommand { get; }

    public ObservableCollection<string> SettingCategories { get; private set; }
    public ObservableCollection<string> AvailableGuiSettings { get; private set; }
    
    [ObservableProperty] public string selectedSettingCategory;
    [ObservableProperty] public string selectedGuiSetting;
    
    public SettingsMainWindowViewModel()
    {
        CloseSettingsCommand = ReactiveCommand.Create((() => new object()));
        
        SettingCategories = new ObservableCollection<string>() {"GUI"};
        
        
        AvailableGuiSettings = new ObservableCollection<string>();
        AvailableGuiSettings.AddRange(Directory.GetDirectories($"{Program.DefaultDataPath}/Guis").Select(Path.GetFileName));


        SelectedGuiSetting = File.ReadAllText($"{Program.DefaultDataPath}/SelectedGui.txt").Trim();
        
    }

    public void SaveGuiSettings()
    {
        File.WriteAllText($"{Program.DefaultDataPath}/SelectedGui.txt", SelectedGuiSetting);
    }
    
    partial void OnSelectedGuiSettingChanged(string oldValue, string newValue)
    {
        
    }
}