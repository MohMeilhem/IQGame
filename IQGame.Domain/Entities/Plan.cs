using System;

namespace IQGame.Domain.Entities
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