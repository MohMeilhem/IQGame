using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IQGame.Admin.Models
{
    public class EditCategoryModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? GroupId { get; set; }

        public string? ImageUrl { get; set; }

        public bool DisableMCQ { get; set; } = false;

        public IFormFile? ImageFile { get; set; }
    }
} 