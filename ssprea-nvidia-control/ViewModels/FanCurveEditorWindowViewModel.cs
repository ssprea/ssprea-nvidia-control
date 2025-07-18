using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using ReactiveUI;

namespace ssprea_nvidia_control.ViewModels;

public partial class FanCurveEditorWindowViewModel : ViewModelBase
{
    [ObservableProperty] private FanCurveViewModel _currentFanCurve;

    public FanCurveEditorWindowViewModel(FanCurveViewModel? fanCurve)
    {
        _currentFanCurve = fanCurve ?? new FanCurveViewModel(FanCurve.DefaultFanCurve());
        SaveCurveCommand = ReactiveCommand.Create(() =>
        {
            CurrentFanCurve.BaseFanCurve.GenerateGpuTempToFanSpeedMap();
            return CurrentFanCurve;
        });
    }
    
    
    public FanCurveEditorWindowViewModel() : this(new FanCurveViewModel(FanCurve.DefaultFanCurve()))
    {
        
    }


    public ReactiveCommand<Unit, FanCurveViewModel> SaveCurveCommand { get; }

    public void CancelCommand()
    {
        
    }

    public void AddPointCommand()
    {
        CurrentFanCurve.BaseFanCurve.CurvePoints.Add(new FanCurvePoint());
    }

    public void RemovePointCommand(FanCurvePoint selectedPoint)
    {
        CurrentFanCurve.BaseFanCurve.CurvePoints.Remove(selectedPoint);
    }
}