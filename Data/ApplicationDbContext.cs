using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Models;

namespace AndenStemesterEksamensProjekt.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Additional configuration can be added here if needed
            // For example, setting default values, constraints, etc.
        }
    }
}
