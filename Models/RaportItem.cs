namespace BugFlow.Models;

public class RaportItem
{
    public string StatusNume { get; set; } = string.Empty;
    public int Count { get; set; }
    public string CountText { get; set; } = string.Empty;
    public int Procent { get; set; }
    public Color Culoare { get; set; } = Colors.Grey;
}
