using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using sspreaNvidiaControl.Models;

namespace sspreaNvidiaControl.ViewModels;

public partial class NewOcProfileWindowViewModel : ViewModelBase
{
    

    [ObservableProperty] private uint _powerLimitSliderValue;
    [ObservableProperty] private uint _gpuClockOffsetSliderValue;
    [ObservableProperty] private uint _memClockOffsetSliderValue;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;
    
    
    public GpuViewModel? SelectedGpu { get; private set; }
    public static ObservableCollection<FanCurveViewModel>? FanCurvesList => MainWindowViewModel.FanCurvesList;


    public NewOcProfileWindowViewModel(GpuViewModel targetGpu)
    {

        SelectedGpu = targetGpu;
        
        CreateProfileCommand = ReactiveCommand.Create(() => new OcProfile(Name ?? "New Profile",GpuClockOffsetSliderValue,MemClockOffsetSliderValue,PowerLimitSliderValue,SelectedFanCurve?.BaseFanCurve));
        
    }
    
    public ReactiveCommand<Unit, OcProfile> CreateProfileCommand { get; }
    
    
    public static void CancelButtonCommand()
    {
        
    }
    
    
}