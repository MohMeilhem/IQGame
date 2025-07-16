namespace IQGame.Shared.Models
{
    public class CategoryAvailability
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? CategoryImageUrl { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int AvailableGames { get; set; }
        public int TotalQuestions { get; set; }
        public int UsedQuestions { get; set; }
        public int EasyQuestionsAvailable { get; set; }
        public int MediumQuestionsAvailable { get; set; }
        public int HardQuestionsAvailable { get; set; }
        public int EasyQuestionsTotal { get; set; }
        public int MediumQuestionsTotal { get; set; }
        public int HardQuestionsTotal { get; set; }
    }
} 