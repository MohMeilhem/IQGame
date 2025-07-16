using Microsoft.EntityFrameworkCore;
using IQGame.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IQGame.Infrastructure.Persistence
{
    public class IQGameDbContext : IdentityDbContext<IdentityUser>
    {
        public IQGameDbContext(DbContextOptions<IQGameDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionQuestion> SessionQuestions { get; set; }
        public DbSet<TeamScore> TeamScores { get; set; }
        public DbSet<UsedHelp> UsedHelps { get; set; }
        public DbSet<User> GameUsers { get; set; }
        public DbSet<UserUsedQuestion> UserUsedQuestions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<UserPlan> UserPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity tables
            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(256);
                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(e => e.LoginProvider).HasMaxLength(450);
                entity.Property(e => e.ProviderKey).HasMaxLength(450);
                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.RoleId).HasMaxLength(450);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.LoginProvider).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(450);
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.Property(e => e.RoleId).HasMaxLength(450);
            });

            // Configure Game User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("GameUsers");

                entity.Property(e => e.Username)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.IdentityUserId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.Score)
                    .IsRequired()
                    .HasDefaultValue(0); // ✅ Add this

                entity.Property(e => e.LastLoginDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email);

                entity.HasOne(u => u.IdentityUser)
                    .WithOne()
                    .HasForeignKey<User>(u => u.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.TeamScores)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.UsedHelps)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // Configure Category
            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name);

            // Configure Group
            modelBuilder.Entity<Group>()
                .Property(g => g.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Group>()
                .Property(g => g.Description)
                .HasMaxLength(200);

            modelBuilder.Entity<Group>()
                .Property(g => g.Color)
                .HasMaxLength(20);

            modelBuilder.Entity<Group>()
                .HasIndex(g => g.Name)
                .IsUnique();

            // Configure Category-Group relationship
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Group)
                .WithMany(g => g.Categories)
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.SetNull); // Don't delete categories when group is deleted

            // Configure Question
            modelBuilder.Entity<Question>()
                .Property(q => q.Text)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Answer
            modelBuilder.Entity<Answer>()
                .Property(a => a.Text)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Plan
            modelBuilder.Entity<Plan>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.GamesCount).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure UserPlan
            modelBuilder.Entity<UserPlan>(entity =>
            {
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.Property(e => e.StripeSessionId).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure all string properties to use nvarchar for Unicode support
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    {
                        property.SetColumnType("nvarchar(max)");
                    }
                }
            }
        }
    }
}
