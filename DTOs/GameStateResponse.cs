namespace ShredleApi.DTOs
{
    public class GameStateResponse
    {
        public DateTime Date { get; set; }
        public SoloResponse? CurrentSolo { get; set; }
        public bool IsComplete { get; set; }
        public int AttemptsRemaining { get; set; }
    }
}