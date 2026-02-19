using BugFlow.Models;
using BugFlow.Pages;

namespace BugFlow.Pages.Issues;

public partial class IssuesListPage : ContentPage
{
    private List<Issue> _allIssues = new();

    public IssuesListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        ListPageUiState.SetLoading(loadingIndicator, true);
        emptyState.IsVisible = false;

        try
        {
            _allIssues = await App.Database.GetIssuesAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nu s-au putut incarca issue-urile: {ex}");
            await DisplayAlert("Eroare", $"Nu s-au putut incarca issue-urile: {ex.Message}", "OK");
            _allIssues.Clear();
            listView.ItemsSource = _allIssues;
            emptyState.IsVisible = true;
        }
        finally
        {
            ListPageUiState.SetLoading(loadingIndicator, false);
            ListPageUiState.SetEmptyState(emptyState, _allIssues.Count, loadingIndicator.IsRunning);
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadDataAsync();
        refreshView.IsRefreshing = false;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = searchBar.Text?.Trim() ?? string.Empty;
        List<Issue> filteredIssues = string.IsNullOrEmpty(query)
            ? _allIssues
            : _allIssues.Where(i => i.Titlu.Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || i.Descriere.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        listView.ItemsSource = filteredIssues;
        emptyState.IsVisible = filteredIssues.Count == 0 && !loadingIndicator.IsRunning;
    }

    private async void OnAdaugaClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new IssueDetailPage(new Issue()));
    }

    private async void OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Issue issue)
        {
            listView.SelectedItem = null;
            await Navigation.PushAsync(new IssueDetailPage(issue));
        }
    }
}
