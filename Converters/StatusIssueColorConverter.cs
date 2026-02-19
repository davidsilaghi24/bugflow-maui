using System.Globalization;
using BugFlow.Models;

namespace BugFlow.Converters;

public class StatusIssueColorConverter : IValueConverter
{
    public static Color GetColor(StatusIssue status) => status switch
    {
        StatusIssue.ToDo => Color.FromArgb("#9E9E9E"),
        StatusIssue.InProgress => Color.FromArgb("#2196F3"),
        StatusIssue.Review => Color.FromArgb("#FF9800"),
        StatusIssue.Done => Color.FromArgb("#4CAF50"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StatusIssue s ? GetColor(s) : Color.FromArgb("#9E9E9E");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
