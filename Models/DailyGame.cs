using System.Text.Json.Serialization;

namespace ShredleApi.Models
{
    public class DailyGame
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        
        [JsonPropertyName("Date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("SoloId")]
        public int? SoloId { get; set; }
        
        // This property is not stored in the database, but used for navigation
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Solo? Solo { get; set; }
    }
}