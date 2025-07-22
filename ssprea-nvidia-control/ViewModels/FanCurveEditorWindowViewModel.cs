using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ssprea_nvidia_control.Models;
using ReactiveUI;

namespace ssprea_nvidia_control.ViewModels;

public partial class FanCurveEditorWindowViewModel : ViewModelBase
{
    [ObservableProperty] private FanCurveViewModel? _currentFanCurve;

    public FanCurveEditorWindowViewModel(FanCurveViewModel? fanCurve)
    {
        _currentFanCurve = fanCurve ?? new FanCurveViewModel(FanCurve.DefaultFanCurve());
        
        SaveCurveCommand = ReactiveCommand.Create(() =>
        {
            CurrentFanCurve?.BaseFanCurve.GenerateGpuTempToFanSpeedMap();
            return CurrentFanCurve;
        });
    }
    
    
    public FanCurveEditorWindowViewModel() : this(null)
    {
        
    }


    public ReactiveCommand<Unit, FanCurveViewModel?> SaveCurveCommand { get; }

    public void CancelCommand()
    {
        CurrentFanCurve = null;
        SaveCurveCommand.Execute().Subscribe();
    }

    public void AddPointCommand()
    {
        CurrentFanCurve?.BaseFanCurve.CurvePoints.Add(new FanCurvePoint());
    }

    public async Task RemovePointCommand(FanCurvePoint? selectedPoint)
    {
        if (selectedPoint is null)
        {
            await MessageBoxManager.GetMessageBoxStandard("Warning","Please select a point to remove",ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }
        
        CurrentFanCurve?.BaseFanCurve.CurvePoints.Remove(selectedPoint);
    }
}