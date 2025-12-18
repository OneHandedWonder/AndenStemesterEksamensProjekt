using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;

namespace AndenStemesterEksamensProjekt.Tests
{
    public class DatabaseServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DatabaseService _databaseService;

        public DatabaseServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _databaseService = new DatabaseService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange - Klargør
            var user = new User
            {
                Email = "testUser@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _databaseService.GetUserByEmailAsync("testUser@example.com");

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.Equal("testUser@example.com", result.Email);
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("User", result.LastName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithInactiveUser_ReturnsNull()
        {
            // Arrange - Klargør
            var user = new User
            {
                Email = "inactive@example.com",
                FirstName = "Inactive",
                LastName = "User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _databaseService.GetUserByEmailAsync("inactive@example.com");

            // Assert - Bekræft
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithNonExistentEmail_ReturnsNull()
        {
            // Act - Udfør
            var result = await _databaseService.GetUserByEmailAsync("nonexistent@example.com");

            // Assert - Bekræft
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateLastLoginAsync_UpdatesTimestamp()
        {
            // Arrange - Klargør
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                LastLogin = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var beforeUpdate = DateTime.UtcNow;

            // Act - Udfør
            await _databaseService.UpdateLastLoginAsync(user.Uid);

            // Assert - Bekræft
            var updatedUser = await _context.Users.FindAsync(user.Uid);
            Assert.NotNull(updatedUser);
            var time = DateTime.UtcNow;
            Assert.True(updatedUser.LastLogin <= time);
            Assert.True(updatedUser.LastLogin >= beforeUpdate);
        }

        [Fact]
        public async Task CreateUserAsync_CreatesNewUser()
        {
            // Act - Udfør
            var userId = await _databaseService.CreateUserAsync("newtestuser@example.com", "hashedpassword");

            // Assert - Bekræft
            Assert.True(userId > 0);
            var user = await _context.Users.FindAsync(userId);
            Assert.NotNull(user);
            Assert.Equal("newtestuser@example.com", user.Email);
            Assert.Equal("hashedpassword", user.PasswordHash);
            Assert.True(user.IsActive);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange - Klargør
            var user = new User
            {
                Email = "testUser@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _databaseService.GetUserByIdAsync(user.Uid);

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.Equal(user.Uid, result.Uid);
            Assert.Equal("testUser@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act - Udfør
            var result = await _databaseService.GetUserByIdAsync(999);

            // Assert - Bekræft
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInactiveUser_ReturnsNull()
        {
            // Arrange - Klargør
            var user = new User
            {
                Email = "inactive@example.com",
                FirstName = "Inactive",
                LastName = "User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _databaseService.GetUserByIdAsync(user.Uid);

            // Assert - Bekræft
            Assert.Null(result);
        }
    }
}
