using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace sspreaNvidiaControl.Converters;

public class LiveStatsGridResponsiveColumnConverter : IValueConverter
{
    
    private const double Threshold = 745; //same breakpoint as container query "queryLiveStatsGrid"

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return new GridLength(0);
        
        var width = (double)value;
        return width < Threshold
            ? new GridLength(6, GridUnitType.Star)
            : new GridLength(12, GridUnitType.Star);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}