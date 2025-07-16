namespace IQGame.Shared.Models
{
    public class UserUsedQuestion
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuestionId { get; set; }

        public User User { get; set; }
        public Question Question { get; set; }
    }
}
