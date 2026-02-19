using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class PriorityColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Prioritate.High => Color.FromArgb("#F44336"),
            Prioritate.Medium => Color.FromArgb("#FF9800"),
            Prioritate.Low => Color.FromArgb("#4CAF50"),
            _ => Color.FromArgb("#9E9E9E")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
