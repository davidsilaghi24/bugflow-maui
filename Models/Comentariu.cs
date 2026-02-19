using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BugFlow.Models;

public class Comentariu
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime Data { get; set; } = DateTime.Now;

    [ForeignKey(typeof(MembruEchipa))]
    public int AutorId { get; set; }

    [ManyToOne]
    public MembruEchipa? Autor { get; set; }

    [ForeignKey(typeof(Issue))]
    public int IssueId { get; set; }

    [ManyToOne]
    public Issue? Issue { get; set; }
}
