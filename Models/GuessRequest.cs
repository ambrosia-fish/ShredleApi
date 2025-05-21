namespace ShredleApi.Models
{
    public class GuessRequest
    {
        public string GameId { get; set; } = string.Empty;
        public int SoloId { get; set; }
        public string Guess { get; set; } = string.Empty;
        public int Attempt { get; set; }
    }
}