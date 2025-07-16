using IQGame.Shared.Models;

namespace IQGame.Client.Models
{
    public class FavoriteCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;

        public static FavoriteCategory FromCategory(Category category)
        {
            return new FavoriteCategory
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                GroupId = category.GroupId,
                GroupName = category.Group?.Name,
                AddedDate = DateTime.Now
            };
        }

        public static FavoriteCategory FromSelectableCategory(SelectableCategory category)
        {
            return new FavoriteCategory
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                GroupId = category.GroupId,
                GroupName = category.GroupName,
                AddedDate = DateTime.Now
            };
        }
    }
} 