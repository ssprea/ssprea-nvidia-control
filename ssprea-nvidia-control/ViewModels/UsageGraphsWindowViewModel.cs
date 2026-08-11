using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.Utils;

namespace ssprea_nvidia_control.ViewModels;

public partial class UsageGraphsWindowViewModel : ViewModelBase
{
    public CancellationTokenSource CancelTokenSrc = new();
    private readonly GpuViewModel _targetGpu;
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
    
    public object GpuTempLock    {get;} = new();
    public object PowerUsageLock {get;} = new();
    public object FanSpeedLock   {get;} = new();
    public object GpuClockLock   {get;} = new();
    public object MemClockLock   {get;} = new();
    public object GpuUsageLock   {get;} = new();
    public object MemUsageLock   {get;} = new();
    
    // private static SKTypeface _defaultGraphTypeface =  SKTypeface.FromStream(AssetLoader.Open(new Uri("avares://ssprea-nvidia-control/Assets/Fonts/NotoSans/NotoSans-Light.ttf")));
    //     
    // [ObservableProperty] private SolidColorPaint _graphTooltipTextPaint = new SolidColorPaint(SKColors.Black) {SKTypeface = _defaultGraphTypeface};
    
    #region GraphStyles

    private static readonly SKColor ThemeTextColor = SKColor.Parse("#F1F0F5");

    private static readonly SolidColorPaint GraphsSeparatorsPaint = new SolidColorPaint(ThemeTextColor.WithAlpha(60))
    {
        StrokeThickness = 2,
        PathEffect = new DashEffect([3, 3])
    };
    
    
    // private static string XAxisFormatter(DateTime date)
    // {
    //     var secsAgo = (DateTime.Now - date).TotalSeconds;
    //
    //     return secsAgo < 1
    //         ? "now"
    //         : $"{secsAgo:N0}s ago";
    // }

    //Axes styles for graphs 
    public Axis[] GraphXAxes { get; set; } =
    [
        // new DateTimeAxis(TimeSpan.FromSeconds(1),XAxisFormatter)
        // {
        //     // CustomSeparators = GetSeparators(),
        //     // AnimationsSpeed = TimeSpan.FromMilliseconds(0),
        //     // SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100)),
        //     // LabelsPaint = new SolidColorPaint(SKColors.White) {SKTypeface = _defaultGraphTypeface},
        //     LabelsPaint = null,
        //     
        // }
        new Axis()
        {
            LabelsPaint = null
        }
    ];

    
    // private static double[] GetSeparators()
    // {
    //     var now = DateTime.Now;
    //
    //     return
    //     [
    //         now.AddSeconds(-300).Ticks, //5m
    //         now.AddSeconds(-270).Ticks,
    //         now.AddSeconds(-240).Ticks, //4m
    //         now.AddSeconds(-210).Ticks,
    //         now.AddSeconds(-180).Ticks, //3m
    //         now.AddSeconds(-150).Ticks,
    //         now.AddSeconds(-120).Ticks, //2m
    //         now.AddSeconds(-90).Ticks,
    //         now.AddSeconds(-60).Ticks, //1m
    //         now.AddSeconds(-30).Ticks,
    //         now.Ticks
    //     ];
    // }
    public Axis[] GpuTempGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreTemp,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
            
            
        }
    ];
    
    public Axis[] PowerUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsGpuPower,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    public Axis[] GpuClockGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreClock,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,
            //
            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    public Axis[] MemClockGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsMemClock,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    public Axis[] GpuUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsCoreUsage,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    public Axis[] MemUsageGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsMemUsage,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    public Axis[] FanSpeedGraphYAxes { get; set; } =
    [
        new Axis
        {
            Name = Lang.Resources.GraphsFanSpeed,
            // NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            NameTextSize = 10,

            // LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite) {SKTypeface = _defaultGraphTypeface}, 
            TextSize = 10,

            SeparatorsPaint = GraphsSeparatorsPaint
        }
    ];
    
    #endregion GraphStyles
    
    
    // public UsageGraphsWindowViewModel() : this(MainWindowViewModel.NvmlService.GpuList[0]) {}
    public UsageGraphsWindowViewModel(GpuViewModel targetGpu)
    {
        GpuTempSeries[0] = new LineSeries<int>()
        {
            Values = _gpuTempValues,
            Fill = new SolidColorPaint(SKColors.Green.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Green) {StrokeThickness = 1},
            GeometryStroke = null,//new SolidColorPaint(SKColors.Green) {StrokeThickness = 4},
            GeometryFill = null,
            YToolTipLabelFormatter = point => $"{_gpuTempValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model}°C",
            LineSmoothness = 0,
            
            // GeometrySize = 8
        };
        
        PowerUsageSeries[0] = new LineSeries<int>()
        {
            Values = _powerUsageValues,
            Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.MediumPurple) {StrokeThickness = 1},
            YToolTipLabelFormatter = point => $"{_powerUsageValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model} W",
            
            //GeometryStroke = new SolidColorPaint(SKColors.MediumPurple) {StrokeThickness = 4},
            GeometryStroke = null,
            GeometryFill = null,
            GeometrySize = 8,
            LineSmoothness = 0
            
            
        };
        
        GpuUsageSeries[0] = new LineSeries<int>()
        {
            Values = _gpuUsageValues,
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            Fill = new SolidColorPaint(SKColors.Aqua.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Aqua) {StrokeThickness = 1},
            LineSmoothness = 0,
            YToolTipLabelFormatter = point => $"{_gpuUsageValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model}%",
            
            
            
        };
        
        MemUsageSeries[0] = new LineSeries<int>()
        {
            Values = _memUsageValues,
            Fill = new SolidColorPaint(SKColors.Goldenrod.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Goldenrod) {StrokeThickness = 1},
            // GeometryStroke = new SolidColorPaint(SKColors.Goldenrod) {StrokeThickness = 4},
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0,
            YToolTipLabelFormatter = point => $"{_memUsageValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model}%",
            
            
            
        };
        
        GpuClockSeries[0] = new LineSeries<int>()
        {
            Values = _gpuClockValues,
            Fill = new SolidColorPaint(SKColors.DeepPink.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.DeepPink) {StrokeThickness = 1},
            // GeometryStroke = new SolidColorPaint(SKColors.DeepPink) {StrokeThickness = 4},
            GeometrySize = 8,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0,
            YToolTipLabelFormatter = point => $"{_gpuClockValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model} MHz",
            
            
            
        };
        
        MemClockSeries[0] = new LineSeries<int>()
        {
            Values = _memClockValues,
            Fill = new SolidColorPaint(SKColors.Chocolate.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.Chocolate) {StrokeThickness = 1},
            GeometryStroke = null,
            GeometryFill = null,
            //GeometryStroke = new SolidColorPaint(SKColors.Chocolate) {StrokeThickness = 4},
            GeometrySize = 8,
            LineSmoothness = 0,
            YToolTipLabelFormatter = point => $"{_memClockValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model} MHz",
            
            
        };
        
        FanSpeedSeries[0] = new LineSeries<int>()
        {
            Values = _fanSpeedValues,
            Fill = new SolidColorPaint(SKColors.IndianRed.WithAlpha(50)),
            Stroke = new SolidColorPaint(SKColors.IndianRed) {StrokeThickness = 1},
            GeometryStroke = null,
            GeometryFill = null,
            //GeometryStroke = new SolidColorPaint(SKColors.IndianRed) {StrokeThickness = 4},
            GeometrySize = 8,
            LineSmoothness = 0,
            YToolTipLabelFormatter = point => $"{_fanSpeedValues.Count - point.Index}{Lang.Resources.GraphsTooltipSecondsAgo}: {point.Model}%",
            
            
            
        };
        
        _targetGpu=targetGpu;

        Task.Run(async () =>
        {
            while (!CancelTokenSrc.Token.IsCancellationRequested)
            {
                Dispatcher.UIThread.Post(UpdateGraphs);
                await Task.Delay(1000);
            }
        });
    }

    private void UpdateGraphs()
    {
        if (_targetGpu.LatestGpuMetrics is null) return;
        
        lock (GpuClockLock)
        {
            _gpuClockValues.Add((int)_targetGpu.LatestGpuMetrics.GpuClockCurrent);
        }

        lock (MemClockLock)
        {
            _memClockValues.Add((int)_targetGpu.LatestGpuMetrics.MemClockCurrent);
        }

        lock (GpuUsageLock)
        {
            _gpuUsageValues.Add((int)_targetGpu.LatestGpuMetrics.UtilizationCore);
        }

        lock (MemUsageLock)
        {
            _memUsageValues.Add((int)_targetGpu.LatestGpuMetrics.UtilizationMemCtl);
        }

        lock (PowerUsageLock)
        {
            _powerUsageValues.Add((int)_targetGpu.LatestGpuMetrics.GpuPowerUsageMilliW/1000);
        }
        // _fanSpeedValues.Add(new DateTimePoint(DateTime.Now,(int)_targetGpu.FansList[0].CurrentSpeed));

        lock (FanSpeedLock)
        {
            _fanSpeedValues.Add((int)_targetGpu.LatestGpuMetrics.FansSpeedPercent.Fan0Speed);
        }

        lock (GpuTempLock)
        {
            _gpuTempValues.Add((int)_targetGpu.LatestGpuMetrics.GpuTemperature);
        }
        
        // GraphXAxes[0].CustomSeparators = GetSeparators();

    }
    
}