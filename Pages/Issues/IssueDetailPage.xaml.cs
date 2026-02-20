using BugFlow.Behaviors;
using BugFlow.Domain;
using BugFlow.Models;
using BugFlow.Pages.Comentarii;

namespace BugFlow.Pages.Issues;

public partial class IssueDetailPage : ContentPage
{
    private readonly Issue _issue;
    private Prioritate _selectedPrioritate;
    private StatusIssue _selectedStatus;

    public IssueDetailPage(Issue issue)
    {
        InitializeComponent();
        _issue = issue;

        Title = issue.Id == 0 ? "Issue nou" : "Editare issue";

        _selectedPrioritate = issue.Prioritate;
        _selectedStatus = issue.Status;
        UpdatePrioritateButtons();
        UpdateStatusButtons();

        descriereEditor.Text = issue.Descriere;
        titluEntry.Text = issue.Titlu;
        dataEstimataPicker.Date = issue.DataEstimata == default ? DateTime.Today.AddDays(Issue.DefaultDeadlineDays) : issue.DataEstimata;

        comentariiButton.IsVisible = issue.Id != 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
        formContent.IsVisible = false;

        try
        {
            var projects = await App.Database.GetProiecteAsync();
            proiectPicker.ItemsSource = projects;
            if (_issue.ProiectId != 0)
            {
                var selectedProject = projects.FirstOrDefault(p => p.Id == _issue.ProiectId);
                proiectPicker.SelectedItem = selectedProject;
                proiectLabel.Text = selectedProject?.Nume ?? "(neselectat)";
            }

            var members = await App.Database.GetMembriAsync();
            membruPicker.ItemsSource = members;
            if (_issue.MembruEchipaId.HasValue)
            {
                var selectedMember = members.FirstOrDefault(m => m.Id == _issue.MembruEchipaId);
                membruPicker.SelectedItem = selectedMember;
                membruLabel.Text = selectedMember?.NumeComplet ?? "(neasignat)";
            }

            ValidateForm();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nu s-au putut incarca datele issue: {ex}");
            await DisplayAlert("Eroare", $"Nu s-au putut incarca datele: {ex.Message}", "OK");
        }
        finally
        {
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
            formContent.IsVisible = true;
        }
    }

    private void OnFormChanged(object? sender, EventArgs e) => ValidateForm();

    private void OnPrioritateClicked(object? sender, EventArgs e)
    {
        if (sender == prioritateLowBtn) _selectedPrioritate = Prioritate.Low;
        else if (sender == prioritateMediumBtn) _selectedPrioritate = Prioritate.Medium;
        else if (sender == prioritateHighBtn) _selectedPrioritate = Prioritate.High;
        UpdatePrioritateButtons();
    }

    private void OnStatusIssueClicked(object? sender, EventArgs e)
    {
        if (sender == statusToDoBtn) _selectedStatus = StatusIssue.ToDo;
        else if (sender == statusInProgressBtn) _selectedStatus = StatusIssue.InProgress;
        else if (sender == statusReviewBtn) _selectedStatus = StatusIssue.Review;
        else if (sender == statusDoneBtn) _selectedStatus = StatusIssue.Done;
        UpdateStatusButtons();
    }

    private void UpdatePrioritateButtons()
    {
        var active = _selectedPrioritate switch
        {
            Prioritate.Low => prioritateLowBtn,
            Prioritate.Medium => prioritateMediumBtn,
            Prioritate.High => prioritateHighBtn,
            _ => prioritateMediumBtn
        };
        SetSegment([prioritateLowBtn, prioritateMediumBtn, prioritateHighBtn], active);
    }

    private void UpdateStatusButtons()
    {
        var active = _selectedStatus switch
        {
            StatusIssue.ToDo => statusToDoBtn,
            StatusIssue.InProgress => statusInProgressBtn,
            StatusIssue.Review => statusReviewBtn,
            StatusIssue.Done => statusDoneBtn,
            _ => statusToDoBtn
        };
        SetSegment([statusToDoBtn, statusInProgressBtn, statusReviewBtn, statusDoneBtn], active);
    }

    private static void SetSegment(Button[] buttons, Button active)
    {
        var primary = (Color)Application.Current!.Resources["Primary"];
        foreach (var btn in buttons)
        {
            var sel = btn == active;
            btn.BackgroundColor = sel ? primary : Color.FromArgb("#E0E0E0");
            btn.TextColor = sel ? Colors.White : Color.FromArgb("#444444");
        }
    }

    private void OnProiectChanged(object? sender, EventArgs e)
    {
        if (proiectPicker.SelectedItem is Proiect proiect)
        {
            proiectLabel.Text = proiect.Nume;
        }

        ValidateForm();
    }

    private void OnMembruChanged(object? sender, EventArgs e)
    {
        if (membruPicker.SelectedItem is MembruEchipa membru)
        {
            membruLabel.Text = membru.NumeComplet;
        }
    }

    private void ValidateForm()
    {
        saveButton.IsEnabled = ValidationBehavior.GetIsValid(titluEntry)
                               && ValidationBehavior.GetIsValid(descriereEditor)
                               && proiectPicker.SelectedIndex >= 0
                               && ValidationRules.IsIssueDateValid(dataEstimataPicker.Date, DateTime.Today);
    }

    private async void OnComentariiClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ComentariiListPage(App.Database, _issue.Id, _issue.Titlu));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!saveButton.IsEnabled || proiectPicker.SelectedItem is not Proiect proiect)
        {
            await DisplayAlert("Date invalide", "Completeaza toate campurile obligatorii.", "OK");
            return;
        }

        _issue.Titlu = titluEntry.Text?.Trim() ?? string.Empty;
        _issue.Descriere = descriereEditor.Text?.Trim() ?? string.Empty;
        _issue.Prioritate = _selectedPrioritate;
        _issue.Status = _selectedStatus;
        _issue.DataEstimata = dataEstimataPicker.Date;
        _issue.ProiectId = proiect.Id;
        _issue.MembruEchipaId = membruPicker.SelectedItem is MembruEchipa membru ? membru.Id : null;

        await App.Database.SaveIssueAsync(_issue);

        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_issue.Id == 0)
        {
            await Navigation.PopAsync();
            return;
        }

        var confirm = await DisplayAlert(
            "Confirmare stergere",
            $"Sigur vrei sa stergi issue-ul \"{_issue.Titlu}\"?",
            "Da, sterge",
            "Anuleaza");

        if (confirm)
        {
            await App.Database.DeleteIssueAsync(_issue);
            await Navigation.PopAsync();
        }
    }
}
