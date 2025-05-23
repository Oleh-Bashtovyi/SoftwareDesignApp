﻿using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SoftwareDesignApp.UI.Converters;

public class BoolToBlockColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Black);
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}