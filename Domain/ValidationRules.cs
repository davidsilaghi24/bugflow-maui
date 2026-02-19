using System.Text.RegularExpressions;

namespace BugFlow.Domain;

public static partial class ValidationRules
{
    private static readonly Regex EmailRegex = EmailPattern();

    public static bool HasRequiredText(string? value) => !string.IsNullOrWhiteSpace(value);

    public static bool IsValidEmail(string? value)
        => !string.IsNullOrWhiteSpace(value) && EmailRegex.IsMatch(value);

    public static bool IsProiectTimelineValid(DateTime dataStart, DateTime dataDeadline)
        => dataDeadline.Date >= dataStart.Date;

    public static bool IsIssueDateValid(DateTime dataEstimata, DateTime today)
        => dataEstimata.Date >= today.Date;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();
}
