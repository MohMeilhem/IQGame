using System.ComponentModel.DataAnnotations.Schema;

namespace IQGame.Shared.Models
{
    public class UsedHelp
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string TeamName { get; set; }
        public string HelpType { get; set; } // "خيارات", "دبل", "إجابتين"
        public int? QuestionId { get; set; }
        public bool IsConsumed { get; set; } = false; // Track if double points has been consumed for a specific question

        public Session Session { get; set; }
    }
}
