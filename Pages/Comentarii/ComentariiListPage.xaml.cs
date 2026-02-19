using BugFlow.Models;
using BugFlow.Pages;

namespace BugFlow.Pages.Comentarii;

public partial class ComentariiListPage : ContentPage
{
    private readonly int? _issueId;
    private List<Comentariu> _allComentarii = new();

    public ComentariiListPage()
    {
        InitializeComponent();
        _issueId = null;
    }

    public ComentariiListPage(int issueId, string issueTitlu)
    {
        InitializeComponent();
        _issueId = issueId;
        Title = $"Comentarii - {issueTitlu}";
        emptyLabel.Text = "Nu exista comentarii pentru acest issue";
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
            _allComentarii = _issueId.HasValue
                ? await App.Database.GetComentariiByIssueAsync(_issueId.Value)
                : await App.Database.GetComentariiAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            App.LogError("Nu s-au putut incarca comentariile", ex);
            await DisplayAlert("Eroare", $"Nu s-au putut incarca comentariile: {ex.Message}", "OK");
            _allComentarii.Clear();
            listView.ItemsSource = _allComentarii;
            emptyState.IsVisible = true;
        }
        finally
        {
            ListPageUiState.SetLoading(loadingIndicator, false);
            ListPageUiState.SetEmptyState(emptyState, _allComentarii.Count, loadingIndicator.IsRunning);
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        string query = searchBar.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrEmpty(query)
            ? _allComentarii
            : _allComentarii.Where(c => c.Text.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        listView.ItemsSource = filtered;
        emptyState.IsVisible = filtered.Count == 0 && !loadingIndicator.IsRunning;
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadDataAsync();
        refreshView.IsRefreshing = false;
    }

    private async void OnAdaugaClicked(object? sender, EventArgs e)
    {
        var comentariu = new Comentariu();
        if (_issueId.HasValue)
            comentariu.IssueId = _issueId.Value;

        await Navigation.PushAsync(new ComentariuDetailPage(comentariu));
    }

    private async void OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Comentariu comentariu)
        {
            listView.SelectedItem = null;
            await Navigation.PushAsync(new ComentariuDetailPage(comentariu));
        }
    }
}
