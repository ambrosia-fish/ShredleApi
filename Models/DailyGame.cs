namespace ShredleApi.Models;

public class DailyGame
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int SoloId { get; set; }
    public Solo? Solo { get; set; }
}