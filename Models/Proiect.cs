using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BugFlow.Models;

public enum StatusProiect
{
    Activ,
    Inactiv,
    Finalizat
}

public class Proiect
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nume { get; set; } = string.Empty;

    public string Descriere { get; set; } = string.Empty;

    public DateTime DataStart { get; set; } = DateTime.Now;

    public const int DefaultDeadlineDays = 30;
    public DateTime DataDeadline { get; set; } = DateTime.Now.AddDays(DefaultDeadlineDays);

    public StatusProiect Status { get; set; } = StatusProiect.Activ;

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Issue> Issues { get; set; } = new();

    public override string ToString() => Nume;
}
