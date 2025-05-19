using System.Text.Json.Serialization;

namespace ShredleApi.Models
{
    public class DailyGame
    {
        public int Id { get; set; }
        
        public DateTime Date { get; set; }
        
        public int? SoloId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Solo? Solo { get; set; }
    }
}