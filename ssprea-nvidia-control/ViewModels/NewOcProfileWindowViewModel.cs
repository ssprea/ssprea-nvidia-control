using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using ReactiveUI;

namespace ssprea_nvidia_control.ViewModels;

public partial class NewOcProfileWindowViewModel : ViewModelBase
{
    MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty] private uint _powerLimitSliderValue;
    [ObservableProperty] private uint _gpuClockOffsetSliderValue;
    [ObservableProperty] private uint _memClockOffsetSliderValue;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;
    
    
    public MonitoredGpu? SelectedGpu => _mainWindowViewModel.SelectedGpu;
    public ObservableCollection<FanCurveViewModel>? FanCurvesList => MainWindowViewModel.FanCurvesList;


    public NewOcProfileWindowViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        
        CreateProfileCommand = ReactiveCommand.Create(() => new OcProfile(Name ?? "New Profile",GpuClockOffsetSliderValue,MemClockOffsetSliderValue,PowerLimitSliderValue,SelectedFanCurve?.BaseFanCurve));
        
    }
    
    public ReactiveCommand<Unit, OcProfile> CreateProfileCommand { get; }
    
    
    public void CancelButtonCommand()
    {
        
    }
    
    
}