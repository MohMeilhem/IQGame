using Microsoft.AspNetCore.Identity;

namespace IQGame.Shared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string IdentityUserId { get; set; } // Foreign key to AspNetUsers table
        public int Score { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }

        // Navigation properties
        public virtual IdentityUser IdentityUser { get; set; }
        public virtual ICollection<UserUsedQuestion> UsedQuestions { get; set; }
        public virtual ICollection<TeamScore> TeamScores { get; set; }
        public virtual ICollection<UsedHelp> UsedHelps { get; set; }

        public User()
        {
            UsedQuestions = new HashSet<UserUsedQuestion>();
            TeamScores = new HashSet<TeamScore>();
            UsedHelps = new HashSet<UsedHelp>();
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
