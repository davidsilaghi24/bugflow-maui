using BugFlow.Data;
using BugFlow.Models;
using BugFlow.Pages;

namespace BugFlow.Pages.Comentarii;

public partial class ComentariiListPage : ContentPage
{
    private readonly BugFlowDatabase _db;
    private readonly int? _issueId;
    private List<Comentariu> _allComentarii = new();

    public ComentariiListPage(BugFlowDatabase db)
    {
        _db = db;
        _issueId = null;
        InitializeComponent();
    }

    public ComentariiListPage(BugFlowDatabase db, int issueId, string issueTitlu)
    {
        _db = db;
        _issueId = issueId;
        InitializeComponent();
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
                ? await _db.GetComentariiByIssueAsync(_issueId.Value)
                : await _db.GetComentariiAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nu s-au putut incarca comentariile: {ex}");
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
