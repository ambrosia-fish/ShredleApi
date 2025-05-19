namespace ShredleApi.Models;

public class Solo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string SpotifyId { get; set; } = string.Empty;
    public int SoloStartTimeMs { get; set; }
    public int SoloEndTimeMs { get; set; }
    public string Guitarist { get; set; } = string.Empty;
    public string AiHint { get; set; } = string.Empty;
}