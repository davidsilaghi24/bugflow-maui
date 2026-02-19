using BugFlow.Behaviors;
using BugFlow.Models;

namespace BugFlow.Pages.Comentarii;

public partial class ComentariuDetailPage : ContentPage
{
    private readonly Comentariu _comentariu;

    public ComentariuDetailPage(Comentariu comentariu)
    {
        InitializeComponent();
        _comentariu = comentariu;

        Title = comentariu.Id == 0 ? "Comentariu nou" : "Editare comentariu";
        textEditor.Text = comentariu.Text;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        loadingIndicator.IsVisible = true;
        loadingIndicator.IsRunning = true;
        formContent.IsVisible = false;

        try
        {
            var membri = await App.Database.GetMembriAsync();
            autorPicker.ItemsSource = membri;
            if (_comentariu.AutorId != 0)
            {
                autorPicker.SelectedItem = membri.FirstOrDefault(m => m.Id == _comentariu.AutorId);
            }

            var issues = await App.Database.GetIssuesAsync();
            issuePicker.ItemsSource = issues;
            if (_comentariu.IssueId != 0)
            {
                issuePicker.SelectedItem = issues.FirstOrDefault(i => i.Id == _comentariu.IssueId);
            }

            ValidateForm();
        }
        catch (Exception ex)
        {
            App.LogError("Nu s-au putut incarca datele comentariului", ex);
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

    private void ValidateForm()
    {
        saveButton.IsEnabled = ValidationBehavior.GetIsValid(textEditor)
                               && autorPicker.SelectedIndex >= 0
                               && issuePicker.SelectedIndex >= 0;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // var dbgNow = DateTime.Now;
        if (!saveButton.IsEnabled
            || autorPicker.SelectedItem is not MembruEchipa autor
            || issuePicker.SelectedItem is not Issue issue)
        {
            await DisplayAlert("Date invalide", "Completeaza toate campurile obligatorii.", "OK");
            return;
        }

        _comentariu.Text = textEditor.Text?.Trim() ?? string.Empty;
        _comentariu.Data = _comentariu.Id == 0 ? DateTime.Now : _comentariu.Data; // la editare pastrez data originala
        _comentariu.AutorId = autor.Id;
        _comentariu.IssueId = issue.Id;

        await App.Database.SaveComentariuAsync(_comentariu);
        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_comentariu.Id == 0)
        {
            await Navigation.PopAsync();
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirmare stergere",
            "Sigur vrei sa stergi acest comentariu?",
            "Da, sterge",
            "Anuleaza");

        if (confirm)
        {
            await App.Database.DeleteComentariuAsync(_comentariu);
            await Navigation.PopAsync();
        }
    }
}
