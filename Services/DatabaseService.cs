using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;

namespace AndenStemesterEksamensProjekt.Services
{
    public class DatabaseService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a user by email address
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        /// <summary>
        /// Update last login timestamp for a user
        /// </summary>
        public async Task UpdateLastLoginAsync(int uid)
        {
            var user = await _context.Users.FindAsync(uid);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create a new user with hashed password
        /// </summary>
        public async Task<int> CreateUserAsync(string email, string passwordHash)
        {
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user.Uid;
        }
    }
}
