namespace IQGame.Application.Models.Session
{
    public class GameStatusViewModel
    {
        public int TotalQuestionsAnswered { get; set; }
        public bool GameFinished { get; set; }
        public string Team1Name { get; set; }
        public int Team1Score { get; set; }
        public TeamHelpStatus Team1Help { get; set; }
        public string Team2Name { get; set; }
        public int Team2Score { get; set; }
        public TeamHelpStatus Team2Help { get; set; }
        public string CurrentTurn { get; set; }
        public string SessionName { get; set; }
    }

    public class TeamHelpStatus
    {
        public string TeamName { get; set; }
        public bool KhiyaratUsed { get; set; }
        public bool DoublePointsUsed { get; set; }
        public bool DoublePointsActive { get; set; }
        public bool TwoAnswersUsed { get; set; }
    }
}
