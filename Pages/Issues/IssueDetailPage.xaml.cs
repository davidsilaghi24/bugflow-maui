using BugFlow.Behaviors;
using BugFlow.Domain;
using BugFlow.Models;
using BugFlow.Pages.Comentarii;

namespace BugFlow.Pages.Issues;

public partial class IssueDetailPage : ContentPage
{
    private readonly Issue _issue;

    public IssueDetailPage(Issue issue)
    {
        InitializeComponent();
        _issue = issue;

        Title = issue.Id == 0 ? "Issue nou" : "Editare issue";

        prioritatePicker.ItemsSource = Enum.GetValues<Prioritate>().Cast<object>().ToList();
        prioritatePicker.SelectedItem = issue.Prioritate;

        statusPicker.ItemsSource = Enum.GetValues<StatusIssue>().Cast<object>().ToList();
        statusPicker.SelectedItem = issue.Status;

        descriereEditor.Text = issue.Descriere;
        titluEntry.Text = issue.Titlu;
        dataEstimataPicker.Date = issue.DataEstimata == default ? DateTime.Today.AddDays(7) : issue.DataEstimata;

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
            if (_issue.MembruEchipaId != 0)
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
        await Navigation.PushAsync(new ComentariiListPage(_issue.Id, _issue.Titlu));
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
        _issue.Prioritate = prioritatePicker.SelectedItem is Prioritate prioritate ? prioritate : Prioritate.Medium;
        _issue.Status = statusPicker.SelectedItem is StatusIssue status ? status : StatusIssue.ToDo;
        _issue.DataEstimata = dataEstimataPicker.Date;
        _issue.ProiectId = proiect.Id;
        _issue.MembruEchipaId = membruPicker.SelectedItem is MembruEchipa membru ? membru.Id : 0;

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
