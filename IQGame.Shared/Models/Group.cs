using System.ComponentModel.DataAnnotations;

namespace IQGame.Shared.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; } // For UI styling (e.g., "#6c3fc5")

        public int DisplayOrder { get; set; } = 0; // For ordering groups

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Category>? Categories { get; set; }
    }
} 