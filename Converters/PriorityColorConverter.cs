using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class PriorityColorConverter : IValueConverter
{
    public static Color GetColor(Prioritate prioritate) => prioritate switch
    {
        Prioritate.High => Color.FromArgb("#F44336"),
        Prioritate.Medium => Color.FromArgb("#FF9800"),
        Prioritate.Low => Color.FromArgb("#4CAF50"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Prioritate p ? GetColor(p) : Color.FromArgb("#9E9E9E");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
