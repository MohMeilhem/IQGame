namespace IQGame.Application.Models.Session
{
    public class SessionResultViewModel
    {
        public TeamScoreResult Team1 { get; set; }
        public TeamScoreResult Team2 { get; set; }
        public string Winner { get; set; }
        public List<CategoryScoreBreakdown> Categories { get; set; }
    }
}
