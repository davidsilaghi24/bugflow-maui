using BugFlow.Behaviors;
using BugFlow.Models;

namespace BugFlow.Pages.Membri;

public partial class MembruDetailPage : ContentPage
{
    private readonly MembruEchipa _membru;

    public MembruDetailPage(MembruEchipa membru)
    {
        InitializeComponent();
        _membru = membru;

        Title = membru.Id == 0 ? "Membru nou" : "Editare membru";

        rolPicker.ItemsSource = Enum.GetValues<Rol>().Cast<object>().ToList();
        rolPicker.SelectedItem = membru.Rol;

        seniorityPicker.ItemsSource = Enum.GetValues<Seniority>().Cast<object>().ToList();
        seniorityPicker.SelectedItem = membru.Seniority;

        emailEntry.Text = membru.Email;
        numeEntry.Text = membru.NumeComplet;

        ValidateForm();
    }

    private void OnFormChanged(object? sender, TextChangedEventArgs e) => ValidateForm();

    private void ValidateForm()
    {
        saveButton.IsEnabled = ValidationBehavior.GetIsValid(numeEntry)
                               && ValidationBehavior.GetIsValid(emailEntry);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!saveButton.IsEnabled)
        {
            await DisplayAlert("Date invalide", "Verifica numele si adresa de e-mail.", "OK");
            return;
        }

        _membru.NumeComplet = numeEntry.Text?.Trim() ?? string.Empty;
        _membru.Email = emailEntry.Text?.Trim() ?? string.Empty;
        _membru.Rol = rolPicker.SelectedItem is Rol rol ? rol : Rol.Developer;

        _membru.Seniority = seniorityPicker.SelectedItem is Seniority seniority ? seniority : Seniority.Junior;

        await App.Database.SaveMembruAsync(_membru);
        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_membru.Id == 0)
        {
            await Navigation.PopAsync();
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirmare stergere",
            $"Sigur vrei sa stergi membrul \"{_membru.NumeComplet}\"?",
            "Da, sterge",
            "Anuleaza");

        if (confirm)
        {
            await App.Database.DeleteMembruAsync(_membru);
            await Navigation.PopAsync();
        }
    }
}
