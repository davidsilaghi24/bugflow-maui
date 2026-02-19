using BugFlow.Models;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

namespace BugFlow.Data;

public class BugFlowDatabase
{
    private readonly SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _isInitialized;

    public BugFlowDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initSemaphore.WaitAsync();
        try
        {
            await _database.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await _database.CreateTableAsync<Proiect>();
            await _database.CreateTableAsync<MembruEchipa>();
            await _database.CreateTableAsync<Issue>();
            await _database.CreateTableAsync<Comentariu>();

            _isInitialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    // Proiect
    public async Task<List<Proiect>> GetProiecteAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<Proiect>().ToListAsync();
    }

    public async Task<Proiect?> GetProiectAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Proiect>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveProiectAsync(Proiect proiect)
    {
        await EnsureInitializedAsync();
        return proiect.Id != 0 ? await _database.UpdateAsync(proiect) : await _database.InsertAsync(proiect);
    }

    public async Task<int> DeleteProiectAsync(Proiect proiect)
    {
        await EnsureInitializedAsync();
        var issues = await _database.Table<Issue>().Where(i => i.ProiectId == proiect.Id).ToListAsync();
        foreach (var issue in issues)
        {
            await DeleteIssueAsync(issue);
        }

        return await _database.DeleteAsync(proiect);
    }

    // MembruEchipa
    public async Task<List<MembruEchipa>> GetMembriAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<MembruEchipa>().ToListAsync();
    }

    public async Task<MembruEchipa?> GetMembruAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<MembruEchipa>().Where(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveMembruAsync(MembruEchipa membru)
    {
        await EnsureInitializedAsync();
        return membru.Id != 0 ? await _database.UpdateAsync(membru) : await _database.InsertAsync(membru);
    }

    public async Task<int> DeleteMembruAsync(MembruEchipa membru)
    {
        await EnsureInitializedAsync();
        var comments = await _database.Table<Comentariu>().Where(c => c.AutorId == membru.Id).ToListAsync();
        foreach (var comment in comments)
        {
            await _database.DeleteAsync(comment);
        }

        // issue-urile nu le sterg, le las dar scot asignarea ca sa nu ramana cu un id mort
        var assignedIssues = await _database.Table<Issue>().Where(i => i.MembruEchipaId == membru.Id).ToListAsync();
        foreach (var issue in assignedIssues)
        {
            issue.MembruEchipaId = 0;
            await _database.UpdateAsync(issue);
        }

        return await _database.DeleteAsync(membru);
    }

    // Issue
    public async Task<List<Issue>> GetIssuesAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<Issue>().ToListAsync();
    }

    public async Task<Issue?> GetIssueAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Issue>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Issue>> GetIssuesByProiectAsync(int proiectId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Issue>().Where(i => i.ProiectId == proiectId).ToListAsync();
    }

    public async Task<int> SaveIssueAsync(Issue issue)
    {
        await EnsureInitializedAsync();
        await ValidateIssueReferencesAsync(issue);
        return issue.Id != 0 ? await _database.UpdateAsync(issue) : await _database.InsertAsync(issue);
    }

    public async Task<Issue?> GetIssueWithChildrenAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.GetWithChildrenAsync<Issue>(id, recursive: true);
    }

    public async Task<int> DeleteIssueAsync(Issue issue)
    {
        await EnsureInitializedAsync();
        var comments = await _database.Table<Comentariu>().Where(c => c.IssueId == issue.Id).ToListAsync();
        foreach (var comment in comments)
        {
            await _database.DeleteAsync(comment);
        }

        return await _database.DeleteAsync(issue);
    }

    // Comentariu
    public async Task<List<Comentariu>> GetComentariiAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<Comentariu>().ToListAsync();
    }

    public async Task<Comentariu?> GetComentariuAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Comentariu>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Comentariu>> GetComentariiByIssueAsync(int issueId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Comentariu>().Where(c => c.IssueId == issueId).ToListAsync();
    }

    public async Task<int> SaveComentariuAsync(Comentariu comentariu)
    {
        await EnsureInitializedAsync();
        await ValidateComentariuReferencesAsync(comentariu);
        return comentariu.Id != 0
            ? await _database.UpdateAsync(comentariu)
            : await _database.InsertAsync(comentariu);
    }

    public async Task<Proiect?> GetProiectWithChildrenAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.GetWithChildrenAsync<Proiect>(id, recursive: true);
    }

    public async Task<int> DeleteComentariuAsync(Comentariu comentariu)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(comentariu);
    }

    // helpers used by tests / seed check
    public async Task<int> CountAsync<T>() where T : new()
    {
        await EnsureInitializedAsync();
        return await _database.Table<T>().CountAsync();
    }

    public async Task<int> GetForeignKeysPragmaAsync()
    {
        await EnsureInitializedAsync();
        return await _database.ExecuteScalarAsync<int>("PRAGMA foreign_keys;");
    }

    public async Task<List<string>> GetUserTableNamesAsync()
    {
        await EnsureInitializedAsync();
        return await _database.QueryScalarsAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
    }

    private async Task ValidateIssueReferencesAsync(Issue issue)
    {
        if (issue.ProiectId <= 0)
        {
            throw new InvalidOperationException("Issue trebuie sa aiba un proiect valid.");
        }

        var proiectExists = await _database.Table<Proiect>().Where(p => p.Id == issue.ProiectId).CountAsync() > 0;
        if (!proiectExists)
        {
            throw new InvalidOperationException("Proiect inexistent pentru issue.");
        }

        if (issue.MembruEchipaId > 0)
        {
            var membruExists = await _database.Table<MembruEchipa>().Where(m => m.Id == issue.MembruEchipaId).CountAsync() > 0;
            if (!membruExists)
            {
                throw new InvalidOperationException("Membru inexistent pentru issue.");
            }
        }
    }

    private async Task ValidateComentariuReferencesAsync(Comentariu comentariu)
    {
        if (comentariu.AutorId <= 0)
        {
            throw new InvalidOperationException("Comentariu trebuie sa aiba autor valid.");
        }

        if (comentariu.IssueId <= 0)
        {
            throw new InvalidOperationException("Comentariu trebuie sa aiba issue valid.");
        }

        var autorExists = await _database.Table<MembruEchipa>().Where(m => m.Id == comentariu.AutorId).CountAsync() > 0;
        if (!autorExists)
        {
            throw new InvalidOperationException("Autor inexistent pentru comentariu.");
        }

        var issueExists = await _database.Table<Issue>().Where(i => i.Id == comentariu.IssueId).CountAsync() > 0;
        if (!issueExists)
        {
            throw new InvalidOperationException("Issue inexistent pentru comentariu.");
        }
    }
}
