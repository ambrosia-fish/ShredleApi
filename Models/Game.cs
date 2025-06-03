using System.ComponentModel.DataAnnotations;

namespace ShredleApi.Models
{
    public class Game
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public int SoloId { get; set; }
        
        // Navigation property
        public Solo Solo { get; set; } = null!;
    }
}
