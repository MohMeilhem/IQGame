namespace IQGame.Shared.Models
{
    public class SessionQuestion
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int QuestionId { get; set; }

        public Session Session { get; set; }
        public Question Question { get; set; }
        public bool IsScored { get; set; } = false;
        public string? TeamAnswered { get; set; }


    }
}
