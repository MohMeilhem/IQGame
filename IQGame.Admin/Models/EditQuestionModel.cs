using System.ComponentModel.DataAnnotations;

namespace IQGame.Admin.Models
{
    public class EditQuestionModel
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Range(1, 3)]
        public int Difficulty { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<EditAnswerModel> Answers { get; set; } = new List<EditAnswerModel>();
    }
} 