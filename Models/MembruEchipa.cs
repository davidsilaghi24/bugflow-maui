using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BugFlow.Models;

public enum Rol
{
    Developer,
    Tester,
    PM
}

public enum Seniority
{
    Junior,
    Mid,
    Senior
}

public class MembruEchipa
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string NumeComplet { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Rol Rol { get; set; } = Rol.Developer;

    public Seniority Seniority { get; set; } = Seniority.Junior;

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Issue> Issues { get; set; } = new();

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Comentariu> Comentarii { get; set; } = new();

    public override string ToString() => NumeComplet;
}
