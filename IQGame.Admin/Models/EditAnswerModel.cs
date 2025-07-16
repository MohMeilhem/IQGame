using System.ComponentModel.DataAnnotations;

namespace IQGame.Admin.Models
{
    public class EditAnswerModel
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public string? ImageUrl { get; set; }

        // Remove or comment out validation attributes
        // [Required(ErrorMessage = "Please select a question.")]
        public int? QuestionId { get; set; }
    }
} 