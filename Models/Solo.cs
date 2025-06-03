using System.ComponentModel.DataAnnotations;

namespace ShredleApi.Models
{
    public class Solo
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Artist { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string SpotifyId { get; set; } = string.Empty;
        
        public double StartTimeClip1 { get; set; }
        public double EndTimeClip1 { get; set; }
        public double StartTimeClip2 { get; set; }
        public double EndTimeClip2 { get; set; }
        public double StartTimeClip3 { get; set; }
        public double EndTimeClip3 { get; set; }
        public double StartTimeClip4 { get; set; }
        public double EndTimeClip4 { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Guitarist { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Hint { get; set; } = string.Empty;
        
        // Navigation property
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
