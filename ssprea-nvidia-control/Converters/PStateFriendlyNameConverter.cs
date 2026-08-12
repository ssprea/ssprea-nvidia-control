using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GpuSSharp.Types;

namespace sspreaNvidiaControl.Converters;

public class PStateFriendlyNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GpuPState pstate)
            return null;

        return (int)pstate;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
