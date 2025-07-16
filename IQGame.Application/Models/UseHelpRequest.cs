namespace IQGame.Application.Models
{
    public class UseHelpRequest
    {
        public int SessionId { get; set; }
        public string TeamName { get; set; }
        public string HelpType { get; set; }
        public int? QuestionId { get; set; } // only needed for خيارات
    }
}
