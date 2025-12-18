using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;
// Lavet af Emil
namespace AndenStemesterEksamensProjekt.Tests
{
    public class TeamServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _teamService = new TeamService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllTeamsAsync_ReturnsAllTeams()
        {
            // Arrange - Klargør
            var team1 = new Team { Name = "Team A", Description = "First team", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var team2 = new Team { Name = "Team B", Description = "Second team", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Teams.AddRange(team1, team2);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _teamService.GetAllTeamsAsync();

            // Assert - Bekræft
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Name == "Team A");
            Assert.Contains(result, t => t.Name == "Team B");
        }

        [Fact]
        public async Task GetTeamByIdAsync_WithValidId_ReturnsTeam()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Test Team", Description = "Test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _teamService.GetTeamByIdAsync(team.Id);

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.Equal("Test Team", result.Name);
            Assert.Equal("Test", result.Description);
        }

        [Fact]
        public async Task GetTeamByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act - Udfør
            var result = await _teamService.GetTeamByIdAsync(999);

            // Assert - Bekræft
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTeamAsync_CreatesNewTeam()
        {
            // Act - Udfør
            var result = await _teamService.CreateTeamAsync("New Team", "Team Description");

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.Equal("New Team", result.Name);
            Assert.Equal("Team Description", result.Description);
            Assert.True(result.Id > 0);

            // Verificer i databasen
            var teamInDb = await _context.Teams.FindAsync(result.Id);
            Assert.NotNull(teamInDb);
            Assert.Equal("New Team", teamInDb.Name);
        }

        [Fact]
        public async Task UpdateTeamAsync_WithValidId_UpdatesTeam()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Original", Description = "Original Desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _teamService.UpdateTeamAsync(team.Id, "Updated", "Updated Desc");

            // Assert - Bekræft
            Assert.True(result);
            var updatedTeam = await _context.Teams.FindAsync(team.Id);
            Assert.NotNull(updatedTeam);
            Assert.Equal("Updated", updatedTeam.Name);
            Assert.Equal("Updated Desc", updatedTeam.Description);
        }

        [Fact]
        public async Task UpdateTeamAsync_WithInvalidId_ReturnsFalse()
        {
            // Act - Udfør
            var result = await _teamService.UpdateTeamAsync(999, "Updated", "Updated Desc");

            // Assert - Bekræft
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTeamAsync_WithValidId_DeletesTeam()
        {
            // Arrange - Klargør
            var team = new Team { Name = "To Delete", Description = "Will be deleted", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _teamService.DeleteTeamAsync(team.Id);

            // Assert - Bekræft
            Assert.True(result);
            var deletedTeam = await _context.Teams.FindAsync(team.Id);
            Assert.Null(deletedTeam);
        }

        [Fact]
        public async Task DeleteTeamAsync_WithInvalidId_ReturnsFalse()
        {
            // Act - Udfør
            var result = await _teamService.DeleteTeamAsync(999);

            // Assert - Bekræft
            Assert.False(result);
        }

        [Fact]
        public async Task AddUserToTeamAsync_AddsUserSuccessfully()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Team", Description = "Desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user = new User 
            { 
                Email = "test@test.com", 
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash", 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Teams.Add(team);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _teamService.AddUserToTeamAsync(team.Id, user.Uid);

            // Assert - Bekræft
            Assert.True(result);
            var userTeam = await _context.UserTeams
                .FirstOrDefaultAsync(ut => ut.TeamId == team.Id && ut.UserId == user.Uid);
            Assert.NotNull(userTeam);
        }

        [Fact]
        public async Task AddUserToTeamAsync_WithDuplicateMember_ReturnsFalse()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Team", Description = "Desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user = new User 
            { 
                Email = "test@test.com", 
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash", 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Teams.Add(team);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _teamService.AddUserToTeamAsync(team.Id, user.Uid);

            // Act - Udfør
            var result = await _teamService.AddUserToTeamAsync(team.Id, user.Uid);

            // Assert - Bekræft
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveUserFromTeamAsync_RemovesUserSuccessfully()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Team", Description = "Desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user = new User 
            { 
                Email = "test@test.com", 
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash", 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Teams.Add(team);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _teamService.AddUserToTeamAsync(team.Id, user.Uid);

            // Act - Udfør
            var result = await _teamService.RemoveUserFromTeamAsync(team.Id, user.Uid);

            // Assert - Bekræft
            Assert.True(result);
            var userTeam = await _context.UserTeams
                .FirstOrDefaultAsync(ut => ut.TeamId == team.Id && ut.UserId == user.Uid);
            Assert.Null(userTeam);
        }

        [Fact]
        public async Task GetTeamMembersAsync_ReturnsAllMembers()
        {
            // Arrange - Klargør
            var team = new Team { Name = "Team", Description = "Desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user1 = new User 
            { 
                Email = "user1@test.com", 
                FirstName = "User",
                LastName = "One",
                PasswordHash = "hash", 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var user2 = new User 
            { 
                Email = "user2@test.com", 
                FirstName = "User",
                LastName = "Two",
                PasswordHash = "hash", 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Teams.Add(team);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            await _teamService.AddUserToTeamAsync(team.Id, user1.Uid);
            await _teamService.AddUserToTeamAsync(team.Id, user2.Uid);

            // Act - Udfør
            var result = await _teamService.GetTeamMembersAsync(team.Id);

            // Assert - Bekræft
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Email == "user1@test.com");
            Assert.Contains(result, u => u.Email == "user2@test.com");
        }
    }
}
