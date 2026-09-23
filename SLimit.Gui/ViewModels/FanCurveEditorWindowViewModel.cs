using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GpuSSharp.Types;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using SLimit.Gui.Models;

namespace SLimit.Gui.ViewModels;

public partial class FanCurveEditorWindowViewModel : ViewModelBase
{
    [ObservableProperty] private FanCurveViewModel? _currentFanCurve;
    private readonly GpuVendor _selectredGpuVendor;

    public FanCurveEditorWindowViewModel(FanCurveViewModel? fanCurve, GpuVendor selectedGpuVendor)
    {
        _selectredGpuVendor  = selectedGpuVendor;
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
    
    
    public FanCurveEditorWindowViewModel() : this(null,GpuVendor.Nvidia)
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

    public void SaveButtonClicked()
    {
        if (_selectredGpuVendor == GpuVendor.Amd && CurrentFanCurve?.BaseFanCurve.CurvePoints.Count != 5)
        {
            MessageBoxManager.GetMessageBoxStandard("Error!", "AMD GPUs require exactly 5 curve points!",
                ButtonEnum.Ok, Icon.Error
            );
            return;
        }
        SaveCurveCommand.Execute().Subscribe();
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