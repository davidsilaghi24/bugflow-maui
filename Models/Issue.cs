using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BugFlow.Models;

public enum Prioritate
{
    Low,
    Medium,
    High
}

public enum StatusIssue
{
    ToDo,
    InProgress,
    Review,
    Done
}

public class Issue
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Titlu { get; set; } = string.Empty;

    public string Descriere { get; set; } = string.Empty;

    public Prioritate Prioritate { get; set; } = Prioritate.Medium;

    public StatusIssue Status { get; set; } = StatusIssue.ToDo;

    public const int DefaultDeadlineDays = 7;
    public DateTime DataEstimata { get; set; } = DateTime.Now.AddDays(DefaultDeadlineDays);

    [ForeignKey(typeof(Proiect))]
    public int ProiectId { get; set; }

    [ManyToOne]
    public Proiect? Proiect { get; set; }

    [ForeignKey(typeof(MembruEchipa))]
    public int MembruEchipaId { get; set; }

    [ManyToOne]
    public MembruEchipa? MembruEchipa { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Comentariu> Comentarii { get; set; } = new();

    public override string ToString() => Titlu;
}
