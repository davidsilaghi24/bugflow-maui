using BugFlow.Data;
using BugFlow.Models;
using System.Text.Json;

namespace BugFlow;

public partial class App : Application
{
    private static readonly SemaphoreSlim SeedSemaphore = new(1, 1);
    private static bool _isSeeded;
    private static BugFlowDatabase _database = null!;
    public static BugFlowDatabase Database => _database;

    public App(BugFlowDatabase database)
    {
        _database = database;

        InitializeComponent();
        MainPage = new AppShell();

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialization error: {ex}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (MainPage != null)
                    await MainPage.DisplayAlert("Eroare la pornire", "Datele initiale nu au putut fi incarcate.", "OK");
            });
        }
    }

    private static async Task SeedDataAsync()
    {
        if (_isSeeded)
        {
            return;
        }

        await SeedSemaphore.WaitAsync();
        try
        {
            if (_isSeeded)
            {
                return;
            }

            if (await Database.CountAsync<Proiect>() > 0)
            {
                _isSeeded = true;
                return;
            }

            var seed = await LoadSeedDataAsync();
            var proiecteByKey = new Dictionary<string, Proiect>(StringComparer.OrdinalIgnoreCase);
            foreach (var proiectSeed in seed.Proiecte)
            {
                var proiect = new Proiect
                {
                    Nume = proiectSeed.Nume,
                    Descriere = proiectSeed.Descriere,
                    DataStart = proiectSeed.DataStart,
                    DataDeadline = proiectSeed.DataDeadline,
                    Status = Enum.Parse<StatusProiect>(proiectSeed.Status, ignoreCase: true)
                };

                await Database.SaveProiectAsync(proiect);
                proiecteByKey[proiectSeed.Key] = proiect;
            }

            var membriByKey = new Dictionary<string, MembruEchipa>(StringComparer.OrdinalIgnoreCase);
            foreach (var membruSeed in seed.Membri)
            {
                var membru = new MembruEchipa
                {
                    NumeComplet = membruSeed.NumeComplet,
                    Email = membruSeed.Email,
                    Rol = Enum.Parse<Rol>(membruSeed.Rol, ignoreCase: true),
                    Seniority = Enum.Parse<Seniority>(membruSeed.Seniority, ignoreCase: true)
                };

                await Database.SaveMembruAsync(membru);
                membriByKey[membruSeed.Key] = membru;
            }

            var seededIssues = new List<Issue>();
            foreach (var issueSeed in seed.Issues)
            {
                if (!proiecteByKey.TryGetValue(issueSeed.ProiectKey, out var proiect))
                {
                    throw new InvalidOperationException($"Proiect inexistent in seed: {issueSeed.ProiectKey}");
                }

                var issue = new Issue
                {
                    Titlu = issueSeed.Titlu,
                    Descriere = issueSeed.Descriere,
                    Prioritate = Enum.Parse<Prioritate>(issueSeed.Prioritate, ignoreCase: true),
                    Status = Enum.Parse<StatusIssue>(issueSeed.Status, ignoreCase: true),
                    DataEstimata = issueSeed.DataEstimata,
                    ProiectId = proiect.Id,
                    MembruEchipaId = membriByKey.TryGetValue(issueSeed.MembruKey, out var membru) ? membru.Id : null
                };

                await Database.SaveIssueAsync(issue);
                seededIssues.Add(issue);
            }

            foreach (var comentariuSeed in seed.Comentarii)
            {
                if (!membriByKey.TryGetValue(comentariuSeed.AutorKey, out var autor))
                {
                    throw new InvalidOperationException($"Autor inexistent in seed: {comentariuSeed.AutorKey}");
                }

                if (comentariuSeed.IssueIndex < 0 || comentariuSeed.IssueIndex >= seededIssues.Count)
                {
                    throw new InvalidOperationException($"IssueIndex invalid in seed: {comentariuSeed.IssueIndex}");
                }

                var comentariu = new Comentariu
                {
                    Text = comentariuSeed.Text,
                    Data = comentariuSeed.Data,
                    AutorId = autor.Id,
                    IssueId = seededIssues[comentariuSeed.IssueIndex].Id
                };

                await Database.SaveComentariuAsync(comentariu);
            }

            _isSeeded = true;
        }
        finally
        {
            SeedSemaphore.Release();
        }
    }

    private static async Task<InitialSeedData> LoadSeedDataAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("initial-data.json");
        var seedData = await JsonSerializer.DeserializeAsync<InitialSeedData>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return seedData ?? throw new InvalidOperationException("Fisierul initial-data.json este gol sau invalid.");
    }

    private sealed class InitialSeedData
    {
        public List<ProiectSeed> Proiecte { get; set; } = [];
        public List<MembruSeed> Membri { get; set; } = [];
        public List<IssueSeed> Issues { get; set; } = [];
        public List<ComentariuSeed> Comentarii { get; set; } = [];
    }

    private sealed class ProiectSeed
    {
        public string Key { get; set; } = string.Empty;
        public string Nume { get; set; } = string.Empty;
        public string Descriere { get; set; } = string.Empty;
        public DateTime DataStart { get; set; }
        public DateTime DataDeadline { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class MembruSeed
    {
        public string Key { get; set; } = string.Empty;
        public string NumeComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
    }

    private sealed class IssueSeed
    {
        public string Titlu { get; set; } = string.Empty;
        public string Descriere { get; set; } = string.Empty;
        public string Prioritate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataEstimata { get; set; }
        public string ProiectKey { get; set; } = string.Empty;
        public string MembruKey { get; set; } = string.Empty;
    }

    private sealed class ComentariuSeed
    {
        public string Text { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string AutorKey { get; set; } = string.Empty;
        public int IssueIndex { get; set; }
    }
}
