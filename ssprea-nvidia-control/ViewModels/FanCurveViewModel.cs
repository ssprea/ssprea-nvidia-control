
using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ssprea_nvidia_control.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ssprea_nvidia_control.Utils;

namespace ssprea_nvidia_control.ViewModels;

public partial class FanCurveViewModel : ViewModelBase
{

    [ObservableProperty] ObservableCollection<ISeries> _curvePointsSeries;
    
    [ObservableProperty] MaxSizeObservableCollection<ObservablePoint> _currentFanSpeedPoints;
    [ObservableProperty] ObservableCollection<ObservablePoint> _fanCurveGraphPoints;
    

    public FanCurve BaseFanCurve { get; private set; }


    public string Name => BaseFanCurve.Name;
    
    
    
    
    public FanCurveViewModel(FanCurve curve)
    {
        BaseFanCurve = curve;
        // CurvePointsSeries= new ObservableCollection<ISeries>(){GetSeries()};

        CurrentFanSpeedPoints = new(1);
        
        //convert points from base curve to observablepoints

        FanCurveGraphPoints = new();
        CurvePointsSeries = new();
        CurvePointsSeries.Add(GetSeries());
        UpdateSeries();        


    }

    public void UpdateSeries()
    {
        FanCurveGraphPoints.Clear();
        FanCurveGraphPoints.AddRange(BaseFanCurve.CurvePoints.Select(x => new ObservablePoint(x.Temperature, x.FanSpeed)).ToArray());
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