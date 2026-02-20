using BugFlow.Behaviors;
using BugFlow.Models;

namespace BugFlow.Pages.Membri;

public partial class MembruDetailPage : ContentPage
{
    private readonly MembruEchipa _membru;
    private Rol _selectedRol;
    private Seniority _selectedSeniority;

    public MembruDetailPage(MembruEchipa membru)
    {
        InitializeComponent();
        _membru = membru;

        Title = membru.Id == 0 ? "Membru nou" : "Editare membru";

        _selectedRol = membru.Rol;
        _selectedSeniority = membru.Seniority;
        UpdateRolButtons();
        UpdateSeniorityButtons();

        emailEntry.Text = membru.Email;
        numeEntry.Text = membru.NumeComplet;

        ValidateForm();
    }

    private void OnFormChanged(object? sender, TextChangedEventArgs e) => ValidateForm();

    private void OnRolClicked(object? sender, EventArgs e)
    {
        if (sender == rolDeveloperBtn) _selectedRol = Rol.Developer;
        else if (sender == rolTesterBtn) _selectedRol = Rol.Tester;
        else if (sender == rolPMBtn) _selectedRol = Rol.PM;
        UpdateRolButtons();
    }

    private void OnSeniorityClicked(object? sender, EventArgs e)
    {
        if (sender == seniorityJuniorBtn) _selectedSeniority = Seniority.Junior;
        else if (sender == seniorityMidBtn) _selectedSeniority = Seniority.Mid;
        else if (sender == senioritySeniorBtn) _selectedSeniority = Seniority.Senior;
        UpdateSeniorityButtons();
    }

    private void UpdateRolButtons()
    {
        var active = _selectedRol switch
        {
            Rol.Developer => rolDeveloperBtn,
            Rol.Tester => rolTesterBtn,
            Rol.PM => rolPMBtn,
            _ => rolDeveloperBtn
        };
        SetSegment([rolDeveloperBtn, rolTesterBtn, rolPMBtn], active);
    }

    private void UpdateSeniorityButtons()
    {
        var active = _selectedSeniority switch
        {
            Seniority.Junior => seniorityJuniorBtn,
            Seniority.Mid => seniorityMidBtn,
            Seniority.Senior => senioritySeniorBtn,
            _ => seniorityJuniorBtn
        };
        SetSegment([seniorityJuniorBtn, seniorityMidBtn, senioritySeniorBtn], active);
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
        _membru.Rol = _selectedRol;
        _membru.Seniority = _selectedSeniority;

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
