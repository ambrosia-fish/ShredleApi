// Models/Game.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ShredleApi.Models
{
    [Table("games")]
    public class Game : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;
        
        [Column("date")]
        public DateTime Date { get; set; }
        
        [Column("solo_id")]
        public int SoloId { get; set; }
    }
}
