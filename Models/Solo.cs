// Models/Solo.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ShredleApi.Models
{
    [Table("solos")]
    public class Solo : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        
        [Column("artist")]
        public string Artist { get; set; } = string.Empty;
        
        [Column("spotify_id")]
        public string SpotifyId { get; set; } = string.Empty;
        
        [Column("start_time_clip1")]
        public double StartTimeClip1 { get; set; }
        
        [Column("end_time_clip1")]
        public double EndTimeClip1 { get; set; }
        
        [Column("start_time_clip2")]
        public double StartTimeClip2 { get; set; }
        
        [Column("end_time_clip2")]
        public double EndTimeClip2 { get; set; }
        
        [Column("start_time_clip3")]
        public double StartTimeClip3 { get; set; }
        
        [Column("end_time_clip3")]
        public double EndTimeClip3 { get; set; }
        
        [Column("start_time_clip4")]
        public double StartTimeClip4 { get; set; }
        
        [Column("end_time_clip4")]
        public double EndTimeClip4 { get; set; }
        
        [Column("guitarist")]
        public string Guitarist { get; set; } = string.Empty;
        
        [Column("hint")]
        public string Hint { get; set; } = string.Empty;
    }
}