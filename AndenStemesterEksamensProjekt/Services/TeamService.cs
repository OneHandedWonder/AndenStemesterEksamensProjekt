using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;
// lavet af:
// Emil
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
        /// Hent alle teams
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
        /// Hent et team efter ID med medlemmer
        /// </summary>
        public async Task<Team?> GetTeamByIdAsync(int teamId)
        {
            return await _context.Teams
                .Include(t => t.UserTeams)
                    .ThenInclude(ut => ut.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        /// <summary>
        /// Opret et nyt team
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
        /// Opdater team detaljer
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
        /// Slet et team
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
        /// Tilføj en bruger til et team
        /// </summary>
        public async Task<bool> AddUserToTeamAsync(int teamId, int userId)
        {
            // Tjek om brugeren allerede er i teamet
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
        /// Fjern en bruger fra et team
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
        /// Hent alle medlemmer af et team
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
        /// Hent alle teams en bruger tilhører
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
