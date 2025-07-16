using IQGame.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IQGame.Shared.Models
{
    public class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }  // ✅ Match with other models

        public int? GroupId { get; set; }  // Foreign key to Group

        public bool DisableMCQ { get; set; } = false;  // Disable MCQ for questions from this category

        [JsonIgnore]
        public Group? Group { get; set; }  // Navigation property

        [JsonIgnore]
        public ICollection<Question>? Questions { get; set; }
    }
}