namespace IQGame.Application.Models.Session
{
    public class ScoreQuestionRequest
    {
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public string? TeamName { get; set; } // null = no one answered
    }
}
