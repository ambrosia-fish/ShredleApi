namespace ShredleApi.DTOs
{
    public class SoloResponse
    {
        // Basic solo information
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string SpotifyId { get; set; } = string.Empty;
        public int SoloStartTimeMs { get; set; }
        public int SoloEndTimeMs { get; set; }
        public string Guitarist { get; set; } = string.Empty;
        public string AiHint { get; set; } = string.Empty;
        
        // Properties for controlling what to reveal to the user
        public int ClipDurationMs { get; set; }
        public bool IsCorrect { get; set; }
        public int GuessCount { get; set; }
        public bool RevealGuitarist { get; set; }
        public bool RevealHint { get; set; }
    }
}