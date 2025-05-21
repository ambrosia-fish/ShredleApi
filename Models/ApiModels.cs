// Models/ApiResponses.cs
namespace ShredleApi.Models
{
    public class GameResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int SoloId { get; set; }
    }

    public class SoloResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string SpotifyId { get; set; } = string.Empty;
        public double StartTimeClip1 { get; set; }
        public double EndTimeClip1 { get; set; }
        public double StartTimeClip2 { get; set; }
        public double EndTimeClip2 { get; set; }
        public double StartTimeClip3 { get; set; }
        public double EndTimeClip3 { get; set; }
        public double StartTimeClip4 { get; set; }
        public double EndTimeClip4 { get; set; }
        public string Guitarist { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
    }
}