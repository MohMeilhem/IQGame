using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IQGame.Shared.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public string? ImageUrl { get; set; }

        [Range(1, 3)]
        public int Difficulty { get; set; } // 1 = Easy, 2 = Medium, 3 = Hard

        [Range(1, 10000)]
        public int Points { get; set; }     // 250, 500, 1000

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Answer>? Answers { get; set; }

    }
}
