namespace IQGame.Shared.Models
{
    public class TeamScore
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string TeamName { get; set; }
        public int Score { get; set; }

        public Session Session { get; set; }
    }
}
