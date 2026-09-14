using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using GpuSSharp.Types;
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

        GpuClockTune coreTune;
        GpuClockTune memTune;
        
        switch (SelectedGpu.Capabilities.CoreClockTuningMode)
        {
            case GpuClockTuningMode.ClockRange:
                coreTune = new GpuClockTune.ClockRange(SelectedGpu.ClockCoreMinMhz,GpuClockOffsetSliderValue);
                break;
            
            default:
            case GpuClockTuningMode.Offset:
                coreTune = new GpuClockTune.Offset((int)GpuClockOffsetSliderValue, GpuPState.GpuPstate0);
                break;
        }
        
        switch (SelectedGpu.Capabilities.MemoryClockTuningMode)
        {
            case GpuClockTuningMode.ClockRange:
                memTune = new GpuClockTune.ClockRange(SelectedGpu.ClockMemMinMhz,MemClockOffsetSliderValue);
                break;
            default:
            case GpuClockTuningMode.Offset:
                memTune = new GpuClockTune.Offset((int)MemClockOffsetSliderValue, GpuPState.GpuPstate0);
                break;
        }
        
        CreateProfileCommand = ReactiveCommand.Create(() => new OcProfile(Name ?? "New Profile",coreTune,memTune,PowerLimitSliderValue,0,0,SelectedFanCurve?.BaseFanCurve));
        
    }
    
    public ReactiveCommand<Unit, OcProfile> CreateProfileCommand { get; }
    
    
    public static void CancelButtonCommand()
    {
        
    }
    
    
}