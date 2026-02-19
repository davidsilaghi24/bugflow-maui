using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class PrioritateToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Prioritate prioritate ? ToRomanian(prioritate) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && TryFromRomanian(text, out var prioritate)
            ? prioritate
            : Prioritate.Medium;

    public static string ToRomanian(Prioritate prioritate) => prioritate switch
    {
        Prioritate.High => "Ridicata",
        Prioritate.Medium => "Medie",
        Prioritate.Low => "Scazuta",
        _ => prioritate.ToString()
    };

    public static bool TryFromRomanian(string text, out Prioritate prioritate)
    {
        prioritate = text switch
        {
            "Ridicata" => Prioritate.High,
            "Medie" => Prioritate.Medium,
            "Scazuta" => Prioritate.Low,
            _ => Prioritate.Medium
        };

        return text is "Ridicata" or "Medie" or "Scazuta";
    }
}
