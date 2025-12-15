using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;

namespace AndenStemesterEksamensProjekt.Services
{
    public class TeamService
    {
        private readonly ApplicationDbContext _context;

        public TeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all teams
        /// </summary>
        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.UserTeams)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        /*public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.UserTeams)
                    .ThenInclude(ut => ut.User)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        */
        /// <summary>
        /// Get a team by ID with members
        /// </summary>
        public async Task<Team?> GetTeamByIdAsync(int teamId)
        {
            return await _context.Teams
                .Include(t => t.UserTeams)
                    .ThenInclude(ut => ut.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        /// <summary>
        /// Create a new team
        /// </summary>
        public async Task<Team> CreateTeamAsync(string name, string? description)
        {
            var team = new Team
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return team;
        }

        /// <summary>
        /// Update team details
        /// </summary>
        public async Task<bool> UpdateTeamAsync(int teamId, string name, string? description)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return false;

            team.Name = name;
            team.Description = description;
            team.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete a team
        /// </summary>
        public async Task<bool> DeleteTeamAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Add a user to a team
        /// </summary>
        public async Task<bool> AddUserToTeamAsync(int teamId, int userId)
        {
            // Check if user is already in team
            var exists = await _context.UserTeams
                .AnyAsync(ut => ut.TeamId == teamId && ut.UserId == userId);

            if (exists)
                return false;

            var userTeam = new UserTeam
            {
                TeamId = teamId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.UserTeams.Add(userTeam);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Remove a user from a team
        /// </summary>
        public async Task<bool> RemoveUserFromTeamAsync(int teamId, int userId)
        {
            var userTeam = await _context.UserTeams
                .FirstOrDefaultAsync(ut => ut.TeamId == teamId && ut.UserId == userId);

            if (userTeam == null)
                return false;

            _context.UserTeams.Remove(userTeam);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get all members of a team
        /// </summary>
        public async Task<List<User>> GetTeamMembersAsync(int teamId)
        {
            return await _context.UserTeams
                .Where(ut => ut.TeamId == teamId)
                .Include(ut => ut.User)
                .Select(ut => ut.User)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// Get all teams a user belongs to
        /// </summary>
        public async Task<List<Team>> GetUserTeamsAsync(int userId)
        {
            return await _context.UserTeams
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Team)
                .Select(ut => ut.Team)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}
