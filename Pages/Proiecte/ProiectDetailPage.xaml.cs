using BugFlow.Behaviors;
using BugFlow.Domain;
using BugFlow.Models;

namespace BugFlow.Pages.Proiecte;

public partial class ProiectDetailPage : ContentPage
{
    private readonly Proiect _proiect;

    public ProiectDetailPage(Proiect proiect)
    {
        InitializeComponent();
        _proiect = proiect;

        Title = proiect.Id == 0 ? "Proiect nou" : "Editare proiect";

        statusPicker.ItemsSource = Enum.GetNames(typeof(StatusProiect));
        statusPicker.SelectedItem = proiect.Status.ToString();

        numeEntry.Text = proiect.Nume;
        descriereEditor.Text = proiect.Descriere;
        dataStartPicker.Date = proiect.DataStart == default ? DateTime.Today : proiect.DataStart;
        dataDeadlinePicker.Date = proiect.DataDeadline == default ? DateTime.Today.AddDays(Proiect.DefaultDeadlineDays) : proiect.DataDeadline;

        ValidateForm();
    }

    private void OnFormChanged(object? sender, TextChangedEventArgs e) => ValidateForm();

    private void ValidateForm()
    {
        saveButton.IsEnabled = ValidationBehavior.GetIsValid(numeEntry)
                               && ValidationBehavior.GetIsValid(descriereEditor)
                               && ValidationRules.IsProiectTimelineValid(dataStartPicker.Date, dataDeadlinePicker.Date);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!saveButton.IsEnabled)
        {
            await DisplayAlert("Date invalide", "Verifica datele introduse si incearca din nou.", "OK");
            return;
        }

        _proiect.Nume = numeEntry.Text?.Trim() ?? string.Empty;
        _proiect.Descriere = descriereEditor.Text?.Trim() ?? string.Empty;
        _proiect.DataStart = dataStartPicker.Date;
        _proiect.DataDeadline = dataDeadlinePicker.Date;

        var statusText = statusPicker.SelectedItem?.ToString();
        _proiect.Status = Enum.TryParse(statusText, out StatusProiect status)
            ? status
            : StatusProiect.Activ;

        await App.Database.SaveProiectAsync(_proiect);

        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_proiect.Id == 0)
        {
            await Navigation.PopAsync();
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirmare stergere",
            $"Sigur vrei sa stergi proiectul \"{_proiect.Nume}\"?",
            "Da, sterge",
            "Anuleaza");

        if (confirm)
        {
            await App.Database.DeleteProiectAsync(_proiect);
            await Navigation.PopAsync();
        }
    }
}
