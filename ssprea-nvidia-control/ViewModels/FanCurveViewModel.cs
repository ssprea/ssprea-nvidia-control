
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ssprea_nvidia_control.Utils;

namespace ssprea_nvidia_control.ViewModels;

public partial class FanCurveViewModel : ViewModelBase
{

    [ObservableProperty] ObservableCollection<ISeries> _curvePointsSeries;
    [ObservableProperty] MaxSizeObservableCollection<ObservablePoint> _currentFanSpeedPoints;
    
    

    public FanCurve BaseFanCurve { get; private set; }


    public string Name => BaseFanCurve.Name;
    
    
    
    public FanCurveViewModel(FanCurve curve)
    {
        BaseFanCurve = curve;
        CurvePointsSeries= new ObservableCollection<ISeries>(){GetSeries()};

        CurrentFanSpeedPoints = new(1);
        
        

    }

    public void UpdateSeries()
    {
        CurvePointsSeries= new ObservableCollection<ISeries>(){GetSeries()};
        CurvePointsSeries.Add(new LineSeries<ObservablePoint>()
        {
            Values = CurrentFanSpeedPoints,
            Name=Lang.Resources.TextCurrentFanSpeed,
            YToolTipLabelFormatter = point => $"{point.Model?.Y}%",
            XToolTipLabelFormatter = point => $"{Lang.Resources.TextCurrentTemp} {point.Model?.X}°C",
            GeometryStroke=new SolidColorPaint(SKColors.DarkRed) {StrokeThickness = 3},
            LineSmoothness = 0,
        });
    }
    
    private LineSeries<ObservablePoint> GetSeries()
    {
        var seriesValues = new ObservableCollection<ObservablePoint>();
        foreach (var p in BaseFanCurve.CurvePoints)
        {
            seriesValues.Add(new ObservablePoint(p.Temperature,p.FanSpeed));
            
        }
        return new LineSeries<ObservablePoint>(seriesValues)
        {
            GeometryStroke=new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness = 3},
            Stroke= new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness = 3},
            Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(50)),
            YToolTipLabelFormatter = point => $"{point.Model?.Y}%",
            XToolTipLabelFormatter = point => $"Temp: {point.Model?.X}°C",
            LineSmoothness = 0
        };
    }
    
}