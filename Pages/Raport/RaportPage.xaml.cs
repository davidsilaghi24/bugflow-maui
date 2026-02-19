using BugFlow.Converters;
using BugFlow.Domain;
using BugFlow.Models;

namespace BugFlow.Pages.Raport;

public partial class RaportPage : ContentPage
{
    private static readonly StatusIssueToStringConverter StatusConverter = new();
    private static readonly PrioritateToStringConverter PrioritateConverter = new();

    public RaportPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
        content.IsVisible = false;

        try
        {
            var issues = await App.Database.GetIssuesAsync();
            var total = issues.Count;
            totalLabel.Text = total.ToString();

            var ui = System.Globalization.CultureInfo.CurrentUICulture;

            var statusGroups = new List<RaportItem>();
            foreach (var stat in RaportCalculator.BuildStatusStats(issues))
            {
                statusGroups.Add(new RaportItem
                {
                    StatusNume = (string)StatusConverter.Convert(stat.Key, typeof(string), null, ui),
                    Count = stat.Count,
                    CountText = stat.Count == 1 ? "1 issue" : $"{stat.Count} issue-uri",
                    Procent = stat.Procent,
                    Culoare = StatusIssueColorConverter.GetColor(stat.Key)
                });
            }
            statusCollection.ItemsSource = statusGroups;

            var priorityGroups = new List<RaportItem>();
            foreach (var stat in RaportCalculator.BuildPrioritateStats(issues))
            {
                priorityGroups.Add(new RaportItem
                {
                    StatusNume = (string)PrioritateConverter.Convert(stat.Key, typeof(string), null, ui),
                    Count = stat.Count,
                    CountText = stat.Count == 1 ? "1 issue" : $"{stat.Count} issue-uri",
                    Procent = stat.Procent,
                    Culoare = PriorityColorConverter.GetColor(stat.Key)
                });
            }
            priorityCollection.ItemsSource = priorityGroups;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nu s-a putut incarca raportul: {ex}");
            await DisplayAlert("Eroare", $"Nu s-a putut incarca raportul: {ex.Message}", "OK");
        }
        finally
        {
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
            content.IsVisible = true;
        }
    }

}
