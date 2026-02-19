using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            StatusProiect.Activ => Color.FromArgb("#4CAF50"),
            StatusProiect.Inactiv => Color.FromArgb("#FF9800"),
            StatusProiect.Finalizat => Color.FromArgb("#9E9E9E"),
            _ => Color.FromArgb("#9E9E9E")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
