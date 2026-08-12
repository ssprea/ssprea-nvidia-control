using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using sspreaNvidiaControl.Models;

namespace sspreaNvidiaControl.ViewModels;

public partial class FanCurveEditorWindowViewModel : ViewModelBase
{
    [ObservableProperty] private FanCurveViewModel? _currentFanCurve;

    public FanCurveEditorWindowViewModel(FanCurveViewModel? fanCurve)
    {
        _currentFanCurve = fanCurve ?? new FanCurveViewModel(FanCurve.DefaultFanCurve());
        
        SaveCurveCommand = ReactiveCommand.Create(() =>
        {
            if (CurrentFanCurve is null)
                return null;
            
            CurrentFanCurve?.BaseFanCurve.SanitizePoints();
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

    [RelayCommand]
    public async Task RemovePoint(FanCurvePoint? selectedPoint)
    {
        if (selectedPoint is null)
        {
            await MessageBoxManager.GetMessageBoxStandard("Warning","Please select a point to remove",ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }
        
        CurrentFanCurve?.BaseFanCurve.CurvePoints.Remove(selectedPoint);
    }
}