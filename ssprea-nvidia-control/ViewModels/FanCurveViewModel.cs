
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace ssprea_nvidia_control.ViewModels;

public partial class FanCurveViewModel : ViewModelBase
{

    [ObservableProperty]
    ObservableCollection<ISeries> _curvePointsSeries;
    

    public FanCurve BaseFanCurve { get; private set; }


    public string Name => BaseFanCurve.Name;
    
    
    
    public FanCurveViewModel(FanCurve curve)
    {
        BaseFanCurve = curve;
        CurvePointsSeries= new ObservableCollection<ISeries>(){GetSeries()};
    }

    public void UpdateSeries()
    {
        CurvePointsSeries= new ObservableCollection<ISeries>(){GetSeries()};
    }
    
    private LineSeries<ObservablePoint> GetSeries()
    {
        var series = new ObservableCollection<ObservablePoint>();
        foreach (var p in BaseFanCurve.CurvePoints)
        {
            series.Add(new ObservablePoint(p.Temperature,p.FanSpeed));
            
        }
        return new LineSeries<ObservablePoint>(series);
    }
    
}