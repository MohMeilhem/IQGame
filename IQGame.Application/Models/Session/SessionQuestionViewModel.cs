namespace IQGame.Application.Models.Session
{
    public class SessionQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public int Difficulty { get; set; }
        public int Points { get; set; }
        public string ImageUrl { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public bool IsScored { get; set; }
        public int CategoryId { get; set; }
        public bool CategoryDisableMCQ { get; set; }
    }
}
