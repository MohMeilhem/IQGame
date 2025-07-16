namespace IQGame.Shared.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<SessionQuestion> SessionQuestions { get; set; }
        public ICollection<TeamScore> TeamScores { get; set; }
        public ICollection<UsedHelp> UsedHelps { get; set; }
        public string? CurrentTurnTeam { get; set; }

    }
}
