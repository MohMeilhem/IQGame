using System;

namespace IQGame.Shared.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GamesCount { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
} 