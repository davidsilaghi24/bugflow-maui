using BugFlow.Data;
using BugFlow.Models;

namespace BugFlow;

public partial class App : Application
{
    private static readonly Lazy<BugFlowDatabase> DatabaseFactory = new(CreateDatabase, true);
    private static readonly SemaphoreSlim SeedSemaphore = new(1, 1);
    private static bool _isSeeded;

    public static BugFlowDatabase Database => DatabaseFactory.Value;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        _ = InitializeAsync(); // fire-and-forget; errors land in Debug output, not the UI
    }

    private static BugFlowDatabase CreateDatabase()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "bugflow.db3");

        return new BugFlowDatabase(dbPath);
    }

    private static async Task InitializeAsync()
    {
        try
        {
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialization error: {ex}");
        }
    }

    private static async Task SeedDataAsync()
    {
        // TODO: maybe move seed values to a json later
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

            var p1 = new Proiect { Nume = "BugFlow App", Descriere = "Aplicatie de bug tracking pentru echipe agile", DataStart = new DateTime(2025, 1, 10), DataDeadline = new DateTime(2025, 6, 30), Status = StatusProiect.Activ };
            var p2 = new Proiect { Nume = "E-Commerce Platform", Descriere = "Platforma de comert electronic cu plati integrate", DataStart = new DateTime(2025, 2, 1), DataDeadline = new DateTime(2025, 9, 15), Status = StatusProiect.Activ };
            var p3 = new Proiect { Nume = "Website Redesign", Descriere = "Redesign complet al site-ului companiei", DataStart = new DateTime(2024, 6, 1), DataDeadline = new DateTime(2024, 12, 31), Status = StatusProiect.Finalizat };
            await Database.SaveProiectAsync(p1);
            await Database.SaveProiectAsync(p2);
            await Database.SaveProiectAsync(p3);

            var m1 = new MembruEchipa { NumeComplet = "Andrei Popescu", Email = "andrei.popescu@bugflow.ro", Rol = Rol.Developer, Seniority = Seniority.Senior };
            var m2 = new MembruEchipa { NumeComplet = "Maria Ionescu", Email = "maria.ionescu@bugflow.ro", Rol = Rol.Tester, Seniority = Seniority.Mid };
            var m3 = new MembruEchipa { NumeComplet = "Ion Dumitrescu", Email = "ion.dumitrescu@bugflow.ro", Rol = Rol.PM, Seniority = Seniority.Senior };
            var m4 = new MembruEchipa { NumeComplet = "Elena Vasile", Email = "elena.vasile@bugflow.ro", Rol = Rol.Developer, Seniority = Seniority.Junior };
            await Database.SaveMembruAsync(m1);
            await Database.SaveMembruAsync(m2);
            await Database.SaveMembruAsync(m3);
            await Database.SaveMembruAsync(m4);

            var i1 = new Issue { Titlu = "Login crash pe Android", Descriere = "Aplicatia se opreste la autentificare pe Android 14", Prioritate = Prioritate.High, Status = StatusIssue.InProgress, DataEstimata = new DateTime(2025, 3, 15), ProiectId = p1.Id, MembruEchipaId = m1.Id };
            var i2 = new Issue { Titlu = "Buton Submit nu raspunde", Descriere = "Butonul Submit din formularul de contact nu functioneaza", Prioritate = Prioritate.Medium, Status = StatusIssue.ToDo, DataEstimata = new DateTime(2025, 4, 1), ProiectId = p1.Id, MembruEchipaId = m4.Id };
            var i3 = new Issue { Titlu = "Optimizare query-uri SQL", Descriere = "Query-urile pentru rapoarte dureaza peste 10 secunde", Prioritate = Prioritate.High, Status = StatusIssue.Review, DataEstimata = new DateTime(2025, 3, 20), ProiectId = p2.Id, MembruEchipaId = m1.Id };
            var i4 = new Issue { Titlu = "Design responsive navbar", Descriere = "Navbar-ul nu se afiseaza corect pe mobile", Prioritate = Prioritate.Medium, Status = StatusIssue.ToDo, DataEstimata = new DateTime(2025, 5, 1), ProiectId = p2.Id, MembruEchipaId = m4.Id };
            var i5 = new Issue { Titlu = "Integrare gateway plati", Descriere = "Integrarea cu Stripe pentru plati cu cardul", Prioritate = Prioritate.High, Status = StatusIssue.InProgress, DataEstimata = new DateTime(2025, 4, 15), ProiectId = p2.Id, MembruEchipaId = m1.Id };
            var i6 = new Issue { Titlu = "Teste unitare modul auth", Descriere = "Scrierea testelor unitare pentru modulul de autentificare", Prioritate = Prioritate.Low, Status = StatusIssue.Done, DataEstimata = new DateTime(2025, 2, 28), ProiectId = p1.Id, MembruEchipaId = m2.Id };
            await Database.SaveIssueAsync(i1);
            await Database.SaveIssueAsync(i2);
            await Database.SaveIssueAsync(i3);
            await Database.SaveIssueAsync(i4);
            await Database.SaveIssueAsync(i5);
            await Database.SaveIssueAsync(i6);

            var comments = new List<Comentariu>
            {
                new() { Text = "Am reprodus bug-ul pe Pixel 8. Investighez cauza.", Data = new DateTime(2025, 2, 10, 9, 30, 0), AutorId = m1.Id, IssueId = i1.Id },
                new() { Text = "Cred ca e legat de noul API level 34. Verific compatibilitatea.", Data = new DateTime(2025, 2, 10, 14, 15, 0), AutorId = m1.Id, IssueId = i1.Id },
                new() { Text = "Am testat pe 3 dispozitive, bug-ul apare doar pe Android 14+.", Data = new DateTime(2025, 2, 11, 10, 0, 0), AutorId = m2.Id, IssueId = i1.Id },
                new() { Text = "Butonul are event handler-ul dezactivat in XAML. Trebuie verificat.", Data = new DateTime(2025, 2, 12, 11, 0, 0), AutorId = m4.Id, IssueId = i2.Id },
                new() { Text = "Am adaugat indexi pe tabelele de rapoarte. Timpul a scazut la 2s.", Data = new DateTime(2025, 2, 13, 16, 30, 0), AutorId = m1.Id, IssueId = i3.Id },
                new() { Text = "Review-ul arata bine, mai trebuie un test de performanta.", Data = new DateTime(2025, 2, 14, 9, 0, 0), AutorId = m3.Id, IssueId = i3.Id },
                new() { Text = "Am inceput integrarea cu Stripe. Documentatia lor e foarte clara.", Data = new DateTime(2025, 2, 15, 10, 45, 0), AutorId = m1.Id, IssueId = i5.Id },
                new() { Text = "Toate testele trec. Marchez ca Done.", Data = new DateTime(2025, 2, 16, 17, 0, 0), AutorId = m2.Id, IssueId = i6.Id },
            };

            foreach (var comentariu in comments)
            {
                await Database.SaveComentariuAsync(comentariu);
            }

            _isSeeded = true;
        }
        finally
        {
            SeedSemaphore.Release();
        }
    }
}
