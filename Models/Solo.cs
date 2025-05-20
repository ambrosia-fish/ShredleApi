using System.Text.Json.Serialization;

namespace ShredleApi.Models
{
    public class Solo
    {
        // Using standard properties without Entity Framework or Postgrest dependencies
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;
        
        [JsonPropertyName("spotify_id")]
        public string SpotifyId { get; set; } = string.Empty;
        
        [JsonPropertyName("solo_start_time_ms")]
        public int SoloStartTimeMs { get; set; }
        
        [JsonPropertyName("solo_end_time_ms")]
        public int SoloEndTimeMs { get; set; }
        
        [JsonPropertyName("guitarist")]
        public string Guitarist { get; set; } = string.Empty;
        
        [JsonPropertyName("ai_hint")]
        public string AiHint { get; set; } = string.Empty;
    }
}