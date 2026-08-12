using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace sspreaNvidiaControl.Converters;

public class BytesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ulong bytes) 
            return value;
        
        if (parameter is not string targetUnit)
            targetUnit = "MB";

        switch (targetUnit)
        {
            case "KB":
                return bytes / 1000d;
            
            default:
            case "MB":
                return bytes / 1000000d;
            
            case "GB":
                return bytes / 1000000000d;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}