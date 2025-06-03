namespace ShredleApi.Models
{
    public class GuessRequest
    {
        public int GameId { get; set; }
        public int SoloId { get; set; }
        public string Guess { get; set; } = string.Empty;
        public int Attempt { get; set; }
    }
}
