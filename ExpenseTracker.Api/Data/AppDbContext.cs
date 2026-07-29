using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                FullName = "Mario Yaoyao II",
                Username = "mario.yaoyao",
                CreatedAt = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = null
            });
        }
    }
}
