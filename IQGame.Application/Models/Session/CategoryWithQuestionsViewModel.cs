namespace IQGame.Application.Models.Session
{
    public class CategoryWithQuestionsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImageUrl { get; set; }

        public List<SessionQuestionViewModel> Questions { get; set; }
    }

    public class SessionSummaryDto
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public List<CategorySummaryDto> Categories { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsValid { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
    }

    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImageUrl { get; set; }
    }
}
