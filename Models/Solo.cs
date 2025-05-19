using Postgrest.Attributes;
using Postgrest.Models;

namespace ShredleApi.Models
{
    [Table("Solos")]
    public class Solo : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        
        [Column("artist")]
        public string Artist { get; set; } = string.Empty;
        
        [Column("spotify_id")]
        public string SpotifyId { get; set; } = string.Empty;
        
        [Column("solo_start_time_ms")]
        public int SoloStartTimeMs { get; set; }
        
        [Column("solo_end_time_ms")]
        public int SoloEndTimeMs { get; set; }
        
        [Column("guitarist")]
        public string Guitarist { get; set; } = string.Empty;
        
        [Column("ai_hint")]
        public string AiHint { get; set; } = string.Empty;
    }
}