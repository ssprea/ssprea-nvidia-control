using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace sspreaNvidiaControl.Converters;

public class WattsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not uint mWatts) 
            return value;
        
        // if (parameter is not string targetUnit)
        //     targetUnit = "W";
        //
        // switch (targetUnit)
        // {
        //     default:
        //     case "W":
        //         return watts / 1000d;
        //     
        // }
        
        return mWatts / 1000d;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not uint watts) 
            return value;

        return watts * 1000d;
    }
}