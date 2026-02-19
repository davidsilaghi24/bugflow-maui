using BugFlow.Models;

namespace BugFlow.Domain;

public record RaportStat<TEnum>(TEnum Key, int Count, int Procent) where TEnum : struct, Enum;

public static class RaportCalculator
{
    public static List<RaportStat<StatusIssue>> BuildStatusStats(IReadOnlyCollection<Issue> issues)
    {
        var total = issues.Count;
        var stats = new List<RaportStat<StatusIssue>>();

        foreach (var value in Enum.GetValues<StatusIssue>())
        {
            var count = issues.Count(i => i.Status == value);
            var percent = total > 0 ? (int)Math.Round(100.0 * count / total) : 0;
            stats.Add(new RaportStat<StatusIssue>(value, count, percent));
        }

        return stats;
    }

    public static List<RaportStat<Prioritate>> BuildPrioritateStats(IReadOnlyCollection<Issue> issues)
    {
        var total = issues.Count;
        var stats = new List<RaportStat<Prioritate>>();

        foreach (var value in Enum.GetValues<Prioritate>())
        {
            var count = issues.Count(i => i.Prioritate == value);
            var percent = total > 0 ? (int)Math.Round(100.0 * count / total) : 0;
            stats.Add(new RaportStat<Prioritate>(value, count, percent));
        }

        return stats;
    }
}
