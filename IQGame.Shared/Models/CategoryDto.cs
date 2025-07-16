namespace IQGame.Client.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
    }
}
