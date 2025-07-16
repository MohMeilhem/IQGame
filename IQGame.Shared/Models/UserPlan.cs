using System;

namespace IQGame.Shared.Models
{
    public class UserPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public int GamesRemaining { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PaymentStatus { get; set; } // e.g. Pending, Paid, Failed
        public string StripeSessionId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
} 