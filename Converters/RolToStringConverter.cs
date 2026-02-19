using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class RolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Rol rol ? ToRomanian(rol) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && TryFromRomanian(text, out var rol)
            ? rol
            : Rol.Developer;

    public static string ToRomanian(Rol rol) => rol switch
    {
        Rol.Developer => "Dezvoltator",
        Rol.Tester => "Tester",
        Rol.PM => "Manager proiect",
        _ => rol.ToString()
    };

    public static bool TryFromRomanian(string text, out Rol rol)
    {
        rol = text switch
        {
            "Dezvoltator" => Rol.Developer,
            "Tester" => Rol.Tester,
            "Manager proiect" => Rol.PM,
            _ => Rol.Developer
        };

        return text is "Dezvoltator" or "Tester" or "Manager proiect";
    }
}
