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
                FullName = "Super Admin",
                Username = "superadmin",
                ContactNumber = "09876543210",
                Role = UserRole.SuperAdmin,
                HashedPassword = "AQAAAAIAAYagAAAAEMnD4tgZER76L6K/MjnkwaRF6fDDLAx5KW3zFPWKP+94uO/lQ3FrpfpQMAOd6RtrbA==",
                IsActive = true,
                CreatedAt = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = null
            });
        }
    }
}
