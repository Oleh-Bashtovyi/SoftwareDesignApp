using System.Globalization;
using System.Windows.Data;

namespace SoftwareDesignApp.UI.Converters;

public class TextTrimConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        string text = value.ToString();
        int maxLength = int.Parse(parameter.ToString());

        if (text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}