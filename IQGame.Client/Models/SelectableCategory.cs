using IQGame.Shared.Models;

namespace IQGame.Client.Models
{
    public class SelectableCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsFavorite { get; set; } = false;
        
        // Availability information
        public int AvailableGames { get; set; }
        public int TotalQuestions { get; set; }
        public int UsedQuestions { get; set; }
        public int EasyQuestionsAvailable { get; set; }
        public int MediumQuestionsAvailable { get; set; }
        public int HardQuestionsAvailable { get; set; }

        public static SelectableCategory FromCategory(Category category)
        {
            return new SelectableCategory
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                GroupId = category.GroupId,
                GroupName = category.Group?.Name,
                IsSelected = false,
                IsFavorite = false,
                AvailableGames = 0,
                TotalQuestions = 0,
                UsedQuestions = 0,
                EasyQuestionsAvailable = 0,
                MediumQuestionsAvailable = 0,
                HardQuestionsAvailable = 0
            };
        }

        public static SelectableCategory FromCategoryAvailability(CategoryAvailability availability)
        {
            return new SelectableCategory
            {
                Id = availability.CategoryId,
                Name = availability.CategoryName,
                Description = null, // Not available in CategoryAvailability
                ImageUrl = availability.CategoryImageUrl,
                GroupId = availability.GroupId,
                GroupName = availability.GroupName,
                IsSelected = false,
                IsFavorite = false,
                AvailableGames = availability.AvailableGames,
                TotalQuestions = availability.TotalQuestions,
                UsedQuestions = availability.UsedQuestions,
                EasyQuestionsAvailable = availability.EasyQuestionsAvailable,
                MediumQuestionsAvailable = availability.MediumQuestionsAvailable,
                HardQuestionsAvailable = availability.HardQuestionsAvailable
            };
        }
    }
}
