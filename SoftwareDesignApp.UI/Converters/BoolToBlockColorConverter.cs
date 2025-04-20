using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SoftwareDesignApp.UI.Converters;

public class BoolToBlockColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(Colors.White);
        }

        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}