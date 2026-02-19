using BugFlow.Models;
using BugFlow.Pages;

namespace BugFlow.Pages.Membri;

public partial class MembriListPage : ContentPage
{
    private List<MembruEchipa> _allMembers = new();

    public MembriListPage()
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
            _allMembers = await App.Database.GetMembriAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            App.LogError("Nu s-au putut incarca membrii", ex);
            await DisplayAlert("Eroare", $"Nu s-au putut incarca membrii: {ex.Message}", "OK");
            _allMembers.Clear();
            listView.ItemsSource = _allMembers;
            emptyState.IsVisible = true;
        }
        finally
        {
            ListPageUiState.SetLoading(loadingIndicator, false);
            ListPageUiState.SetEmptyState(emptyState, _allMembers.Count, loadingIndicator.IsRunning);
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
        List<MembruEchipa> filtered = string.IsNullOrEmpty(query)
            ? _allMembers
            : _allMembers.Where(m => m.NumeComplet.Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || m.Email.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        listView.ItemsSource = filtered;
        emptyState.IsVisible = filtered.Count == 0 && !loadingIndicator.IsRunning;
    }

    private async void OnAdaugaClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MembruDetailPage(new MembruEchipa()));
    }

    private async void OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not MembruEchipa membru)
            return;

        listView.SelectedItem = null;
        await Navigation.PushAsync(new MembruDetailPage(membru));
    }
}
