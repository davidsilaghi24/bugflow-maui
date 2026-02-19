using BugFlow.Data;
using BugFlow.Domain;
using BugFlow.Models;

namespace BugFlow.Tests;

public class DatabaseTests : IAsyncLifetime
{
    private static readonly DateTime FixedNow = new(2030, 1, 15, 10, 30, 0);
    private static readonly DateTime FixedStart = new(2030, 1, 10);
    private static readonly DateTime FixedDeadline = new(2030, 2, 10);
    private static readonly DateTime FixedEstimate = new(2030, 1, 25);

    private string _tempDirectory = string.Empty;
    private string _dbPath = string.Empty;
    private BugFlowDatabase _database = null!;

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"bugflow-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        _dbPath = Path.Combine(_tempDirectory, "bugflow.db3");
        _database = new BugFlowDatabase(_dbPath);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_tempDirectory) && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Test_DbInitialization_CreatesTables_AndEnablesForeignKeys()
    {
        _ = await _database.GetProiecteAsync();

        var fkPragma = await _database.GetForeignKeysPragmaAsync();
        var tableNames = await _database.GetUserTableNamesAsync();

        Assert.Equal(1, fkPragma);
        Assert.Contains("Proiect", tableNames);
        Assert.Contains("MembruEchipa", tableNames);
        Assert.Contains("Issue", tableNames);
        Assert.Contains("Comentariu", tableNames);
    }

    [Fact]
    public async Task Test_DbInitialization_IsIdempotent()
    {
        var first = await _database.GetUserTableNamesAsync();
        var second = await _database.GetUserTableNamesAsync();

        Assert.Equal(first.OrderBy(x => x), second.OrderBy(x => x));
        Assert.Equal(1, await _database.GetForeignKeysPragmaAsync());
    }

    [Fact]
    public async Task Test_ForeignKeyPragma_StillOn_AfterWrites()
    {
        var proiect = await CreateProiectAsync("P-FK-Pragma");
        var membru = await CreateMembruAsync("FK User", "fk@bugflow.ro");
        _ = await CreateIssueAsync(proiect.Id, membru.Id, "FK check issue");

        var fkPragma = await _database.GetForeignKeysPragmaAsync();
        Assert.Equal(1, fkPragma);
    }

    [Fact]
    public async Task Test_RelationshipIntegrity_IssueRejectsInvalidReferences()
    {
        var proiect = await CreateProiectAsync("P-Valid");

        var invalidProjectIssue = new Issue
        {
            Titlu = "Issue invalid proiect",
            Descriere = "invalid",
            Prioritate = Prioritate.Medium,
            Status = StatusIssue.ToDo,
            DataEstimata = FixedEstimate,
            ProiectId = 999999,
            MembruEchipaId = 0
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _database.SaveIssueAsync(invalidProjectIssue));

        var invalidMembruIssue = new Issue
        {
            Titlu = "Issue invalid membru",
            Descriere = "invalid",
            Prioritate = Prioritate.Medium,
            Status = StatusIssue.ToDo,
            DataEstimata = FixedEstimate,
            ProiectId = proiect.Id,
            MembruEchipaId = 999999
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _database.SaveIssueAsync(invalidMembruIssue));
    }

    [Fact]
    public async Task Test_RelationshipIntegrity_ComentariuRejectsInvalidReferences()
    {
        var proiect = await CreateProiectAsync("P-Comentariu-Ref");
        var membru = await CreateMembruAsync("Autor valid", "autor.valid@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue valid");

        var invalidAutor = new Comentariu
        {
            Text = "invalid autor",
            Data = FixedNow,
            AutorId = 999999,
            IssueId = issue.Id
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => _database.SaveComentariuAsync(invalidAutor));

        var invalidIssue = new Comentariu
        {
            Text = "invalid issue",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = 999999
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => _database.SaveComentariuAsync(invalidIssue));
    }

    [Fact]
    public async Task Test_ProiectCrud_CreateReadUpdateDelete()
    {
        var proiect = new Proiect
        {
            Nume = "Proiect CRUD",
            Descriere = "Descriere CRUD",
            DataStart = FixedStart,
            DataDeadline = FixedDeadline,
            Status = StatusProiect.Activ
        };

        await _database.SaveProiectAsync(proiect);

        var loaded = await _database.GetProiectAsync(proiect.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Proiect CRUD", loaded.Nume);

        proiect.Nume = "Proiect Actualizat";
        proiect.Status = StatusProiect.Finalizat;
        await _database.SaveProiectAsync(proiect);

        var updated = await _database.GetProiectAsync(proiect.Id);
        Assert.NotNull(updated);
        Assert.Equal("Proiect Actualizat", updated.Nume);
        Assert.Equal(StatusProiect.Finalizat, updated.Status);

        await _database.DeleteProiectAsync(proiect);

        var afterDelete = await _database.GetProiectAsync(proiect.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Test_MembruCrud_CreateReadUpdateDelete()
    {
        var membru = await CreateMembruAsync("Membru CRUD", "crud.membru@bugflow.ro");

        var loaded = await _database.GetMembruAsync(membru.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Membru CRUD", loaded.NumeComplet);

        membru.Email = "actualizat.membru@bugflow.ro";
        membru.Seniority = Seniority.Senior;
        await _database.SaveMembruAsync(membru);

        var updated = await _database.GetMembruAsync(membru.Id);
        Assert.NotNull(updated);
        Assert.Equal("actualizat.membru@bugflow.ro", updated.Email);
        Assert.Equal(Seniority.Senior, updated.Seniority);

        await _database.DeleteMembruAsync(membru);

        var afterDelete = await _database.GetMembruAsync(membru.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Test_IssueCrud_CreateReadUpdateDelete()
    {
        var proiect = await CreateProiectAsync("P-Issue");
        var membru = await CreateMembruAsync("Issue Owner", "issue.owner@bugflow.ro");

        var issue = new Issue
        {
            Titlu = "Issue CRUD",
            Descriere = "Descriere issue",
            Prioritate = Prioritate.High,
            Status = StatusIssue.InProgress,
            DataEstimata = FixedEstimate,
            ProiectId = proiect.Id,
            MembruEchipaId = membru.Id
        };

        await _database.SaveIssueAsync(issue);

        var loaded = await _database.GetIssueAsync(issue.Id);
        Assert.NotNull(loaded);
        Assert.Equal(StatusIssue.InProgress, loaded.Status);

        issue.Status = StatusIssue.Done;
        issue.Prioritate = Prioritate.Low;
        await _database.SaveIssueAsync(issue);

        var updated = await _database.GetIssueAsync(issue.Id);
        Assert.NotNull(updated);
        Assert.Equal(StatusIssue.Done, updated.Status);
        Assert.Equal(Prioritate.Low, updated.Prioritate);

        await _database.DeleteIssueAsync(issue);

        var afterDelete = await _database.GetIssueAsync(issue.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Test_ComentariuCrud_CreateReadUpdateDelete()
    {
        var proiect = await CreateProiectAsync("P-Comentariu");
        var membru = await CreateMembruAsync("Comentariu Owner", "comment.owner@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue pentru comentariu");

        var comentariu = new Comentariu
        {
            Text = "Comentariu initial",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        };

        await _database.SaveComentariuAsync(comentariu);

        var loaded = await _database.GetComentariuAsync(comentariu.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Comentariu initial", loaded.Text);

        comentariu.Text = "Comentariu actualizat";
        await _database.SaveComentariuAsync(comentariu);

        var updated = await _database.GetComentariuAsync(comentariu.Id);
        Assert.NotNull(updated);
        Assert.Equal("Comentariu actualizat", updated.Text);

        await _database.DeleteComentariuAsync(comentariu);

        var afterDelete = await _database.GetComentariuAsync(comentariu.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Test_GetIssueWithChildren_ReturnsRelatedComments()
    {
        var proiect = await CreateProiectAsync("P-Children-Issue");
        var membru = await CreateMembruAsync("Children User", "children@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue cu copii");

        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "Comentariu #1",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        });
        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "Comentariu #2",
            Data = FixedNow.AddMinutes(1),
            AutorId = membru.Id,
            IssueId = issue.Id
        });

        var issueWithChildren = await _database.GetIssueWithChildrenAsync(issue.Id);

        Assert.NotNull(issueWithChildren);
        Assert.Equal(issue.Id, issueWithChildren.Id);
        Assert.Equal(2, issueWithChildren.Comentarii.Count);
    }

    [Fact]
    public async Task Test_GetProiectWithChildren_ReturnsRelatedIssuesAndComments()
    {
        var proiect = await CreateProiectAsync("P-Children-Proiect");
        var membru = await CreateMembruAsync("Children Owner", "children.owner@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue legat proiect");

        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "Comentariu proiect",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        });

        var proiectWithChildren = await _database.GetProiectWithChildrenAsync(proiect.Id);

        Assert.NotNull(proiectWithChildren);
        Assert.Equal(proiect.Id, proiectWithChildren.Id);
        Assert.Single(proiectWithChildren.Issues);
        Assert.Single(proiectWithChildren.Issues[0].Comentarii);
    }

    [Fact]
    public async Task Test_DeleteProiect_DeletesChildIssuesAndComments()
    {
        var proiect = await CreateProiectAsync("P-Cascade");
        var membru = await CreateMembruAsync("Cascade User", "cascade@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue cascade");

        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "Comentariu dependent",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        });

        await _database.DeleteProiectAsync(proiect);

        Assert.Equal(0, await _database.CountAsync<Issue>());
        Assert.Equal(0, await _database.CountAsync<Comentariu>());
    }

    [Fact]
    public async Task Test_DeleteIssue_DeletesChildComments()
    {
        var proiect = await CreateProiectAsync("P-IssueDelete");
        var membru = await CreateMembruAsync("IssueDelete User", "issue.delete@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue cu comentarii");

        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "comentariu 1",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        });
        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "comentariu 2",
            Data = FixedNow.AddMinutes(1),
            AutorId = membru.Id,
            IssueId = issue.Id
        });

        await _database.DeleteIssueAsync(issue);

        Assert.Equal(0, await _database.CountAsync<Comentariu>());
        Assert.Null(await _database.GetIssueAsync(issue.Id));
    }

    [Fact]
    public async Task Test_DeleteMembru_UnassignsIssues_AndDeletesOwnComments()
    {
        var proiect = await CreateProiectAsync("P-MembruDelete");
        var membru = await CreateMembruAsync("Membru Sters", "delete@bugflow.ro");
        var issue = await CreateIssueAsync(proiect.Id, membru.Id, "Issue legat de membru");

        await _database.SaveComentariuAsync(new Comentariu
        {
            Text = "Comentariu autor",
            Data = FixedNow,
            AutorId = membru.Id,
            IssueId = issue.Id
        });

        await _database.DeleteMembruAsync(membru);

        var remainingIssue = await _database.GetIssueAsync(issue.Id);
        var remainingComments = await _database.GetComentariiByIssueAsync(issue.Id);

        Assert.NotNull(remainingIssue);
        Assert.Equal(0, remainingIssue.MembruEchipaId);
        Assert.Empty(remainingComments);
    }

    [Fact]
    public async Task Test_GetIssuesByProiect_ReturnsOnlyMatchingIssues()
    {
        var proiectA = await CreateProiectAsync("Proiect A");
        var proiectB = await CreateProiectAsync("Proiect B");
        var membru = await CreateMembruAsync("Filter User", "filter@bugflow.ro");

        await CreateIssueAsync(proiectA.Id, membru.Id, "Issue A1");
        await CreateIssueAsync(proiectA.Id, membru.Id, "Issue A2");
        await CreateIssueAsync(proiectB.Id, membru.Id, "Issue B1");

        var issuesForA = await _database.GetIssuesByProiectAsync(proiectA.Id);

        Assert.Equal(2, issuesForA.Count);
        Assert.All(issuesForA, issue => Assert.Equal(proiectA.Id, issue.ProiectId));
    }

    [Fact]
    public async Task Test_GetComentariiByIssue_ReturnsOnlyIssueComments()
    {
        var proiect = await CreateProiectAsync("Proiect comentarii");
        var membru = await CreateMembruAsync("Comentarii User", "comentarii@bugflow.ro");
        var issue1 = await CreateIssueAsync(proiect.Id, membru.Id, "Issue 1");
        var issue2 = await CreateIssueAsync(proiect.Id, membru.Id, "Issue 2");

        await _database.SaveComentariuAsync(new Comentariu { Text = "c1", AutorId = membru.Id, IssueId = issue1.Id, Data = FixedNow });
        await _database.SaveComentariuAsync(new Comentariu { Text = "c2", AutorId = membru.Id, IssueId = issue1.Id, Data = FixedNow.AddMinutes(1) });
        await _database.SaveComentariuAsync(new Comentariu { Text = "c3", AutorId = membru.Id, IssueId = issue2.Id, Data = FixedNow.AddMinutes(2) });

        var commentsForIssue1 = await _database.GetComentariiByIssueAsync(issue1.Id);

        Assert.Equal(2, commentsForIssue1.Count);
        Assert.All(commentsForIssue1, c => Assert.Equal(issue1.Id, c.IssueId));
    }

    [Fact]
    public async Task Test_Durability_DataPersistsAfterReopeningDatabase()
    {
        var proiect = await CreateProiectAsync("P-Durability");
        var membru = await CreateMembruAsync("Durability User", "durability@bugflow.ro");
        _ = await CreateIssueAsync(proiect.Id, membru.Id, "Issue durability");

        var reopened = new BugFlowDatabase(_dbPath);
        var proiecte = await reopened.GetProiecteAsync();
        var membri = await reopened.GetMembriAsync();
        var issues = await reopened.GetIssuesAsync();

        Assert.Contains(proiecte, p => p.Id == proiect.Id);
        Assert.Contains(membri, m => m.Id == membru.Id);
        Assert.Contains(issues, i => i.ProiectId == proiect.Id);
    }

    [Fact]
    public async Task Test_ReportStats_FromDatabase_AfterMutations()
    {
        var proiect = await CreateProiectAsync("P-Raport");
        var membru = await CreateMembruAsync("Raport User", "raport@bugflow.ro");

        var issueTodo = await CreateIssueAsync(proiect.Id, membru.Id, "Issue ToDo");
        issueTodo.Status = StatusIssue.ToDo;
        issueTodo.Prioritate = Prioritate.Low;
        await _database.SaveIssueAsync(issueTodo);

        var issueProgress = await CreateIssueAsync(proiect.Id, membru.Id, "Issue Progress");
        issueProgress.Status = StatusIssue.InProgress;
        issueProgress.Prioritate = Prioritate.High;
        await _database.SaveIssueAsync(issueProgress);

        var statusStats = RaportCalculator.BuildStatusStats(await _database.GetIssuesAsync());
        Assert.Equal(1, statusStats.Single(s => s.Key == StatusIssue.ToDo).Count);
        Assert.Equal(1, statusStats.Single(s => s.Key == StatusIssue.InProgress).Count);

        await _database.DeleteIssueAsync(issueTodo);

        statusStats = RaportCalculator.BuildStatusStats(await _database.GetIssuesAsync());
        var prioritateStats = RaportCalculator.BuildPrioritateStats(await _database.GetIssuesAsync());

        Assert.Equal(0, statusStats.Single(s => s.Key == StatusIssue.ToDo).Count);
        Assert.Equal(1, statusStats.Single(s => s.Key == StatusIssue.InProgress).Count);
        Assert.Equal(1, prioritateStats.Single(s => s.Key == Prioritate.High).Count);
        Assert.Equal(0, prioritateStats.Single(s => s.Key == Prioritate.Low).Count);
    }

    [Fact]
    public async Task Test_ReportStats_EmptyDatabase_ReturnsZeros()
    {
        var statusStats = RaportCalculator.BuildStatusStats(await _database.GetIssuesAsync());
        var prioritateStats = RaportCalculator.BuildPrioritateStats(await _database.GetIssuesAsync());

        Assert.All(statusStats, s =>
        {
            Assert.Equal(0, s.Count);
            Assert.Equal(0, s.Procent);
        });
        Assert.All(prioritateStats, s =>
        {
            Assert.Equal(0, s.Count);
            Assert.Equal(0, s.Procent);
        });
    }

    [Fact]
    public async Task Test_ShouldHandle_MultipleCreates_WithoutThrowing()
    {
        var proiect = await CreateProiectAsync("P-Stress");
        var membru = await CreateMembruAsync("Stress User", "stress@bugflow.ro");

        for (var i = 1; i <= 20; i++)
        {
            var issue = await CreateIssueAsync(proiect.Id, membru.Id, $"Issue {i:D2}");
            await _database.SaveComentariuAsync(new Comentariu
            {
                Text = $"Comentariu {i:D2}",
                Data = FixedNow.AddMinutes(i),
                AutorId = membru.Id,
                IssueId = issue.Id
            });
        }

        Assert.Equal(20, await _database.CountAsync<Issue>());
        Assert.Equal(20, await _database.CountAsync<Comentariu>());
    }

    [Fact]
    public async Task Test_Initialize_CalledConcurrently_ShouldNotThrow()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => i % 2 == 0
                ? _database.GetProiecteAsync().ContinueWith(_ => { })
                : _database.GetMembriAsync().ContinueWith(_ => { }));

        await Task.WhenAll(tasks);

        Assert.Equal(1, await _database.GetForeignKeysPragmaAsync());
    }

    [Fact]
    public async Task Test_SaveIssue_WithNoMemberAssigned_IsAllowed()
    {
        // MembruEchipaId = 0 means unassigned — this is the state left by DeleteMembruAsync.
        // If this contract breaks, the delete-member logic silently corrupts data.
        var proiect = await CreateProiectAsync("P-Unassigned");

        var issue = new Issue
        {
            Titlu = "Issue neasignat",
            Descriere = "Fara membru asignat",
            Prioritate = Prioritate.Low,
            Status = StatusIssue.ToDo,
            DataEstimata = FixedEstimate,
            ProiectId = proiect.Id,
            MembruEchipaId = 0
        };

        await _database.SaveIssueAsync(issue);

        var loaded = await _database.GetIssueAsync(issue.Id);
        Assert.NotNull(loaded);
        Assert.Equal(0, loaded.MembruEchipaId);

        // Also verify it can be updated while still unassigned
        issue.Status = StatusIssue.InProgress;
        await _database.SaveIssueAsync(issue);

        var updated = await _database.GetIssueAsync(issue.Id);
        Assert.NotNull(updated);
        Assert.Equal(StatusIssue.InProgress, updated.Status);
        Assert.Equal(0, updated.MembruEchipaId);
    }

    private async Task<Proiect> CreateProiectAsync(string nume)
    {
        var proiect = new Proiect
        {
            Nume = nume,
            Descriere = $"Descriere {nume}",
            DataStart = FixedStart,
            DataDeadline = FixedDeadline,
            Status = StatusProiect.Activ
        };

        await _database.SaveProiectAsync(proiect);
        return proiect;
    }

    private async Task<MembruEchipa> CreateMembruAsync(string numeComplet, string email)
    {
        var membru = new MembruEchipa
        {
            NumeComplet = numeComplet,
            Email = email,
            Rol = Rol.Developer,
            Seniority = Seniority.Mid
        };

        await _database.SaveMembruAsync(membru);
        return membru;
    }

    private async Task<Issue> CreateIssueAsync(int proiectId, int membruId, string titlu)
    {
        var issue = new Issue
        {
            Titlu = titlu,
            Descriere = $"Descriere {titlu}",
            Prioritate = Prioritate.Medium,
            Status = StatusIssue.ToDo,
            DataEstimata = FixedEstimate,
            ProiectId = proiectId,
            MembruEchipaId = membruId
        };

        await _database.SaveIssueAsync(issue);
        return issue;
    }
}
