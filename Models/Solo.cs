using System.Text.Json.Serialization;

namespace ShredleApi.Models
{
    public class Solo
    {
        // Using standard properties without Entity Framework or Postgrest dependencies
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        
        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("Artist")]
        public string Artist { get; set; } = string.Empty;
        
        [JsonPropertyName("SpotifyId")]
        public string SpotifyId { get; set; } = string.Empty;
        
        [JsonPropertyName("SoloStartTimeMs")]
        public int SoloStartTimeMs { get; set; }
        
        [JsonPropertyName("SoloEndTimeMs")]
        public int SoloEndTimeMs { get; set; }
        
        [JsonPropertyName("Guitarist")]
        public string Guitarist { get; set; } = string.Empty;
        
        [JsonPropertyName("AiHint")]
        public string AiHint { get; set; } = string.Empty;
    }
}