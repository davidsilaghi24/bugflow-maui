using BugFlow.Domain;
using BugFlow.Models;

namespace BugFlow.Tests;

public class DomainRulesTests
{
    [Fact]
    public void Test_ValidationRules_ProjectTimeline()
    {
        var start = new DateTime(2030, 1, 10);

        Assert.True(ValidationRules.IsProiectTimelineValid(start, start));
        Assert.True(ValidationRules.IsProiectTimelineValid(start, start.AddDays(1)));
        Assert.False(ValidationRules.IsProiectTimelineValid(start, start.AddDays(-1)));
    }

    [Fact]
    public void Test_ValidationRules_IssueDate()
    {
        var today = new DateTime(2030, 1, 15);

        Assert.True(ValidationRules.IsIssueDateValid(today, today));
        Assert.True(ValidationRules.IsIssueDateValid(today.AddDays(1), today));
        Assert.False(ValidationRules.IsIssueDateValid(today.AddDays(-1), today));
    }

    [Fact]
    public void Test_ValidationRules_Email()
    {
        Assert.True(ValidationRules.IsValidEmail("user@example.com"));
        Assert.True(ValidationRules.IsValidEmail("prenume.nume@sub.domain.ro"));
        Assert.False(ValidationRules.IsValidEmail("invalid-email"));
        Assert.False(ValidationRules.IsValidEmail("user@"));
        Assert.False(ValidationRules.IsValidEmail(" "));
    }

    [Fact]
    public void Test_RaportCalculator_StatusStats_AreCorrect()
    {
        var issues = new List<Issue>
        {
            new() { Status = StatusIssue.ToDo, Prioritate = Prioritate.Low },
            new() { Status = StatusIssue.InProgress, Prioritate = Prioritate.Medium },
            new() { Status = StatusIssue.InProgress, Prioritate = Prioritate.High },
            new() { Status = StatusIssue.Done, Prioritate = Prioritate.High }
        };

        var stats = RaportCalculator.BuildStatusStats(issues);

        Assert.Equal(4, stats.Count);
        Assert.Equal(1, stats.Single(s => s.Key == StatusIssue.ToDo).Count);
        Assert.Equal(2, stats.Single(s => s.Key == StatusIssue.InProgress).Count);
        Assert.Equal(0, stats.Single(s => s.Key == StatusIssue.Review).Count);
        Assert.Equal(1, stats.Single(s => s.Key == StatusIssue.Done).Count);
        Assert.Equal(50, stats.Single(s => s.Key == StatusIssue.InProgress).Procent);
    }

    [Fact]
    public void Test_RaportCalculator_SingleItem_IsHundredPercent()
    {
        var issues = new List<Issue>
        {
            new() { Status = StatusIssue.Done, Prioritate = Prioritate.High }
        };

        var statusStats = RaportCalculator.BuildStatusStats(issues);
        var prioritateStats = RaportCalculator.BuildPrioritateStats(issues);

        Assert.Equal(100, statusStats.Single(s => s.Key == StatusIssue.Done).Procent);
        Assert.Equal(0, statusStats.Single(s => s.Key == StatusIssue.ToDo).Procent);
        Assert.Equal(100, prioritateStats.Single(s => s.Key == Prioritate.High).Procent);
        Assert.Equal(0, prioritateStats.Single(s => s.Key == Prioritate.Low).Procent);
    }

    [Fact]
    public void Test_RaportCalculator_EmptyInput_ReturnsZeroStats()
    {
        var empty = new List<Issue>();

        var statusStats = RaportCalculator.BuildStatusStats(empty);
        var prioritateStats = RaportCalculator.BuildPrioritateStats(empty);

        Assert.Equal(4, statusStats.Count);
        Assert.All(statusStats, s =>
        {
            Assert.Equal(0, s.Count);
            Assert.Equal(0, s.Procent);
        });

        Assert.Equal(3, prioritateStats.Count);
        Assert.All(prioritateStats, s =>
        {
            Assert.Equal(0, s.Count);
            Assert.Equal(0, s.Procent);
        });
    }

    [Fact]
    public void Test_ValidationRules_HasRequiredText()
    {
        Assert.True(ValidationRules.HasRequiredText("anything"));
        Assert.True(ValidationRules.HasRequiredText("  x  "));
        Assert.False(ValidationRules.HasRequiredText(string.Empty));
        Assert.False(ValidationRules.HasRequiredText("   "));
        Assert.False(ValidationRules.HasRequiredText(null));
    }

    [Fact]
    public void Test_RaportCalculator_PrioritateStats_AreCorrect()
    {
        var issues = new List<Issue>
        {
            new() { Prioritate = Prioritate.Low, Status = StatusIssue.ToDo },
            new() { Prioritate = Prioritate.High, Status = StatusIssue.Done },
            new() { Prioritate = Prioritate.High, Status = StatusIssue.InProgress }
        };

        var stats = RaportCalculator.BuildPrioritateStats(issues);

        Assert.Equal(3, stats.Count);
        Assert.Equal(1, stats.Single(s => s.Key == Prioritate.Low).Count);
        Assert.Equal(0, stats.Single(s => s.Key == Prioritate.Medium).Count);
        Assert.Equal(2, stats.Single(s => s.Key == Prioritate.High).Count);
        Assert.Equal(67, stats.Single(s => s.Key == Prioritate.High).Procent);
    }
}
