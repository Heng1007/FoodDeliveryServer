using FoodDeliveryServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Data
{
    // Inherits DbContext, making this the EF Core database manager
    public class AppDbContext : DbContext
    {
        // Constructor: receives configuration (like database connection string) and passes it to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // These are your "tables".
        // DbSet<FoodItem> means there will be a table named "FoodItems" in the database
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>(); // 👈 This line is a lifesaver!
                                          // Its purpose:
                                          // When saving: converts Enum.Pending to "Pending"
                                          // When retrieving: converts "Pending" to Enum.Pending
        }
    }
}