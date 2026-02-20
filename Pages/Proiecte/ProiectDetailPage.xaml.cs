using BugFlow.Behaviors;
using BugFlow.Data;
using BugFlow.Domain;
using BugFlow.Models;

namespace BugFlow.Pages.Proiecte;

public partial class ProiectDetailPage : ContentPage
{
    private readonly BugFlowDatabase _db;
    private readonly Proiect _proiect;
    private StatusProiect _selectedStatus;

    public ProiectDetailPage(BugFlowDatabase db, Proiect proiect)
    {
        _db = db;
        InitializeComponent();
        _proiect = proiect;

        Title = proiect.Id == 0 ? "Proiect nou" : "Editare proiect";

        _selectedStatus = proiect.Status;
        UpdateStatusButtons();

        numeEntry.Text = proiect.Nume;
        descriereEditor.Text = proiect.Descriere;
        dataStartPicker.Date = proiect.DataStart == default ? DateTime.Today : proiect.DataStart;
        dataDeadlinePicker.Date = proiect.DataDeadline == default ? DateTime.Today.AddDays(Proiect.DefaultDeadlineDays) : proiect.DataDeadline;

        ValidateForm();
    }

    private void OnFormChanged(object? sender, TextChangedEventArgs e) => ValidateForm();

    private void OnStatusClicked(object? sender, EventArgs e)
    {
        if (sender == statusActivBtn) _selectedStatus = StatusProiect.Activ;
        else if (sender == statusInactivBtn) _selectedStatus = StatusProiect.Inactiv;
        else if (sender == statusFinalizatBtn) _selectedStatus = StatusProiect.Finalizat;
        UpdateStatusButtons();
    }

    private void UpdateStatusButtons()
    {
        var active = _selectedStatus switch
        {
            StatusProiect.Activ => statusActivBtn,
            StatusProiect.Inactiv => statusInactivBtn,
            StatusProiect.Finalizat => statusFinalizatBtn,
            _ => statusActivBtn
        };
        SetSegment([statusActivBtn, statusInactivBtn, statusFinalizatBtn], active);
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

        _proiect.Status = _selectedStatus;

        await _db.SaveProiectAsync(_proiect);

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
            await _db.DeleteProiectAsync(_proiect);
            await Navigation.PopAsync();
        }
    }
}
