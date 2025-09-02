using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.Utils;

namespace ssprea_nvidia_control.ViewModels;

public partial class UsageGraphsWindowViewModel : ViewModelBase
{
    public CancellationTokenSource CancelTokenSrc = new();
    private readonly NvmlGpu _targetGpu;
    private static int _graphLength = 300; //seconds of data in graph
    
    [ObservableProperty] private ISeries[] _gpuTempSeries = [new LineSeries<int>()]  ;
    [ObservableProperty] private ISeries[] _powerUsageSeries = [new LineSeries<int>()];
    [ObservableProperty] private ISeries[] _gpuUsageSeries = [new LineSeries<int>()] ;
    [ObservableProperty] private ISeries[] _memUsageSeries = [new LineSeries<int>()] ;
    [ObservableProperty] private ISeries[] _gpuClockSeries = [new LineSeries<int>()] ;
    [ObservableProperty] private ISeries[] _memClockSeries = [new LineSeries<int>()] ;
    [ObservableProperty] private ISeries[] _fanSpeedSeries = [new LineSeries<int>()] ;

    private readonly MaxSizeObservableCollection<int> _gpuTempValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _powerUsageValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _fanSpeedValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _gpuClockValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _memClockValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _gpuUsageValues = new(_graphLength);
    private readonly MaxSizeObservableCollection<int> _memUsageValues = new(_graphLength);
    
    private static SKTypeface _defaultGraphTypeface =  SKTypeface.FromFamilyName("Noto Sans",SKFontStyleWeight.Normal,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright);
    
    [ObservableProperty] private SolidColorPaint _graphTooltipTextPaint = new SolidColorPaint(SKColors.Black) {SKTypeface = _defaultGraphTypeface};
    
    #region GraphStyles

    

    //Axes styles for graphs 
    public Axis[] GraphXAxes { get; set; } =
    [
        new Axis
        {
            LabelsPaint = null, 

            // SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 1 }  
            SeparatorsPaint = null
        }
    ];

    public Axis[] GpuTempGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreTemp,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
            
            
        }
    ];
    
    public Axis[] PowerUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsGpuPower,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    public Axis[] GpuClockGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreClock,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    public Axis[] MemClockGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsMemClock,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    public Axis[] GpuUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreUsage,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    public Axis[] MemUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsMemUsage,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    public Axis[] FanSpeedGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsFanSpeed,
            NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
            { 
                StrokeThickness = 2, 
                PathEffect = new DashEffect([ 3, 3 ]) 
            } 
        }
    ];
    
    #endregion GraphStyles
    
    
    public UsageGraphsWindowViewModel() : this(MainWindowViewModel.NvmlService.GpuList[0]) {}
    public UsageGraphsWindowViewModel(NvmlGpu targetGpu)
    {
        GpuTempSeries[0] = new LineSeries<int>()
        {
            Values = _gpuTempValues,
            Name = Lang.Resources.GraphsCoreTemp,
            Fill = new SolidColorPaint(SKColors.Green.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Green) {StrokeThickness = 1},
            GeometryStroke = null,//new SolidColorPaint(SKColors.Green) {StrokeThickness = 4},
            GeometryFill = null,
            
            LineSmoothness = 0
            // GeometrySize = 8
        };
        
        PowerUsageSeries[0] = new LineSeries<int>()
        {
            Values = _powerUsageValues,
            Name = Lang.Resources.GraphsGpuPower,
            Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.MediumPurple) {StrokeThickness = 1},
            //GeometryStroke = new SolidColorPaint(SKColors.MediumPurple) {StrokeThickness = 4},
            GeometryStroke = null,
            GeometryFill = null,
            GeometrySize = 8,
            LineSmoothness = 0
            
            
        };
        
        GpuUsageSeries[0] = new LineSeries<int>()
        {
            Values = _gpuUsageValues,
            Name = Lang.Resources.GraphsCoreUsage,
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            Fill = new SolidColorPaint(SKColors.Aqua.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Aqua) {StrokeThickness = 1},
            LineSmoothness = 0
            
            
        };
        
        MemUsageSeries[0] = new LineSeries<int>()
        {
            Values = _memUsageValues,
            Name = Lang.Resources.GraphsMemUsage,
            Fill = new SolidColorPaint(SKColors.Goldenrod.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Goldenrod) {StrokeThickness = 1},
            // GeometryStroke = new SolidColorPaint(SKColors.Goldenrod) {StrokeThickness = 4},
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0
            
            
        };
        
        GpuClockSeries[0] = new LineSeries<int>()
        {
            Values = _gpuClockValues,
            Name = Lang.Resources.GraphsCoreClock,
            Fill = new SolidColorPaint(SKColors.DeepPink.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.DeepPink) {StrokeThickness = 1},
            // GeometryStroke = new SolidColorPaint(SKColors.DeepPink) {StrokeThickness = 4},
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0
            
            
        };
        
        MemClockSeries[0] = new LineSeries<int>()
        {
            Values = _memClockValues,
            Name = Lang.Resources.GraphsMemClock,
            Fill = new SolidColorPaint(SKColors.Chocolate.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Chocolate) {StrokeThickness = 1},
            GeometryStroke = null,
            GeometryFill = null,
            //GeometryStroke = new SolidColorPaint(SKColors.Chocolate) {StrokeThickness = 4},
            GeometrySize = 8,
            LineSmoothness = 0
            
        };
        
        FanSpeedSeries[0] = new LineSeries<int>()
        {
            Values = _fanSpeedValues,
            Name = Lang.Resources.GraphsFanSpeed,
            Fill = new SolidColorPaint(SKColors.IndianRed.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.IndianRed) {StrokeThickness = 1},
            GeometryStroke = null,
            GeometryFill = null,
            //GeometryStroke = new SolidColorPaint(SKColors.IndianRed) {StrokeThickness = 4},
            GeometrySize = 8,
            LineSmoothness = 0
            
            
        };
        
        _targetGpu=targetGpu;

        Task.Run(async () =>
        {
            while (CancelTokenSrc.Token.IsCancellationRequested == false)
            {
                UpdateGraphs();
                await Task.Delay(1000);
            }
        });
    }

    private void UpdateGraphs()
    {
        _gpuClockValues.Add((int)_targetGpu.GpuClockCurrent);
        _memClockValues.Add((int)_targetGpu.MemClockCurrent);
        _gpuUsageValues.Add((int)_targetGpu.GpuUtilization.gpu);
        _memUsageValues.Add((int)_targetGpu.GpuUtilization.memory);
        _powerUsageValues.Add((int)_targetGpu.GpuPowerUsageW);
        _fanSpeedValues.Add((int)_targetGpu.FansList[0].CurrentSpeed);
        _gpuTempValues.Add((int)_targetGpu.GpuTemperature);

    }
    
}