using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class StatusIssueToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StatusIssue status ? ToRomanian(status) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && TryFromRomanian(text, out var status)
            ? status
            : StatusIssue.ToDo;

    public static string ToRomanian(StatusIssue status) => status switch
    {
        StatusIssue.ToDo => "De facut",
        StatusIssue.InProgress => "In lucru",
        StatusIssue.Review => "In revizuire",
        StatusIssue.Done => "Finalizat",
        _ => status.ToString()
    };

    public static bool TryFromRomanian(string text, out StatusIssue status)
    {
        status = text switch
        {
            "De facut" => StatusIssue.ToDo,
            "In lucru" => StatusIssue.InProgress,
            "In revizuire" => StatusIssue.Review,
            "Finalizat" => StatusIssue.Done,
            _ => StatusIssue.ToDo
        };

        return text is "De facut" or "In lucru" or "In revizuire" or "Finalizat";
    }
}
