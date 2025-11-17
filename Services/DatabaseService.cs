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
        /// <summary>
        /// hent en bruger ud fra deres uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<User?> GetUserByIdAsync(int uid)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Uid == uid && u.IsActive);
        }
        public async Task<Profile?> GetprofileAsync(int userId)
        {
            return await _context.CurrentProfile
                .FirstOrDefaultAsync(p => p.Uid == userId);
        }

        /// <summary>
        /// Create a new session with a random token
        /// </summary>
        public async Task<string> CreateSessionAsync(int uid, int expirationDays = 30)
        {
            // Generate a random 32-character token
            var sessionToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            sessionToken = sessionToken.Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);

            var session = new Session
            {
                Uid = uid,
                SessionToken = sessionToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return sessionToken;
        }

        /// <summary>
        /// Validate a session token and return the user ID if valid
        /// </summary>
        public async Task<int?> ValidateSessionAsync(string sessionToken)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));
            return session?.Uid;
        }

        /// <summary>
        /// Delete all sessions for a user (logout)
        /// </summary>
        public async Task DeleteUserSessionsAsync(int uid)
        {
            var sessions = await _context.Sessions
                .Where(s => s.Uid == uid)
                .ToListAsync();
            
            _context.Sessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();
        }
        public async Task<int?> GetUserIdBySessionTokenAsync(string sessionToken)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));
            return session?.Uid;
        }
    }
}
