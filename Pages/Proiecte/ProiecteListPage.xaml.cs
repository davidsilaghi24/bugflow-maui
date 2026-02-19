using BugFlow.Models;
using BugFlow.Pages;

namespace BugFlow.Pages.Proiecte;

public partial class ProiecteListPage : ContentPage
{
    private List<Proiect> _allProiecte = new();

    public ProiecteListPage()
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
            _allProiecte = await App.Database.GetProiecteAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nu s-au putut incarca proiectele: {ex}");
            await DisplayAlert("Eroare", $"Nu s-au putut incarca proiectele: {ex.Message}", "OK");
            _allProiecte.Clear();
            listView.ItemsSource = _allProiecte;
            emptyState.IsVisible = true;
        }
        finally
        {
            ListPageUiState.SetLoading(loadingIndicator, false);
            ListPageUiState.SetEmptyState(emptyState, _allProiecte.Count, loadingIndicator.IsRunning);
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadDataAsync();
        refreshView.IsRefreshing = false;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = searchBar.Text?.Trim() ?? string.Empty;
        List<Proiect> filtered = string.IsNullOrEmpty(query)
            ? _allProiecte
            : _allProiecte.Where(p => p.Nume.Contains(query, StringComparison.OrdinalIgnoreCase)
                                      || p.Descriere.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        listView.ItemsSource = filtered;
        emptyState.IsVisible = filtered.Count == 0 && !loadingIndicator.IsRunning;
    }

    private async void OnAdaugaClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProiectDetailPage(new Proiect()));
    }

    private async void OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Proiect proiect)
        {
            listView.SelectedItem = null;
            await Navigation.PushAsync(new ProiectDetailPage(proiect));
        }
    }
}
