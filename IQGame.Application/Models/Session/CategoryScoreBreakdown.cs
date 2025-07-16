namespace IQGame.Application.Models.Session
{
    public class CategoryScoreBreakdown
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Team1Points { get; set; }
        public int Team2Points { get; set; }
    }
}
