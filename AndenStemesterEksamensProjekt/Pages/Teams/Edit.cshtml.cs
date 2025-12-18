using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;
using AndenStemesterEksamensProjekt.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
// Lavet af:
// Emil
namespace AndenStemesterEksamensProjekt.Pages.Teams
{
    public class EditModel : PageModel
    {
        private readonly TeamService _teamService;
        private readonly ApplicationDbContext _context;

        public EditModel(TeamService teamService, ApplicationDbContext context)
        {
            _teamService = teamService;
            _context = context;
        }

        [BindProperty]
        public int TeamId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Navn er påkrævet")]
        [MaxLength(100, ErrorMessage = "Navn må maks være 100 tegn")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public List<int> SelectedMembers { get; set; } = new();

        public List<User> AllUsers { get; set; } = new();
        public List<int> CurrentMemberIds { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Only planners and admins can edit teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Kun plannere og administratorer kan redigere teams.";
                return RedirectToPage("/Teams/Manage");
            }

            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                TempData["ErrorMessage"] = "Hold ikke fundet.";
                return RedirectToPage("/Teams/Manage");
            }

            TeamId = team.Id;
            Name = team.Name;
            Description = team.Description;
            CurrentMemberIds = team.UserTeams.Select(ut => ut.UserId).ToList();
            SelectedMembers = new List<int>(CurrentMemberIds);

            AllUsers = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Only planners and admins can edit teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Kun plannere og administratorer kan redigere teams.";
                return RedirectToPage("/Teams/Manage");
            }

            if (!ModelState.IsValid)
            {
                AllUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();
                return Page();
            }

            try
            {
                // Update team details
                await _teamService.UpdateTeamAsync(TeamId, Name, Description);

                // Get current members
                var currentMembers = await _teamService.GetTeamMembersAsync(TeamId);
                var currentMemberIds = currentMembers.Select(m => m.Uid).ToList();

                // Add new members
                var membersToAdd = SelectedMembers.Except(currentMemberIds).ToList();
                foreach (var memberId in membersToAdd)
                {
                    await _teamService.AddUserToTeamAsync(TeamId, memberId);
                }

                // Remove members no longer selected
                var membersToRemove = currentMemberIds.Except(SelectedMembers).ToList();
                foreach (var memberId in membersToRemove)
                {
                    await _teamService.RemoveUserFromTeamAsync(TeamId, memberId);
                }

                TempData["SuccessMessage"] = $"Hold '{Name}' opdateret succesfuldt!";
                return RedirectToPage("/Teams/Manage");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved opdatering af hold: {ex.Message}";
                AllUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();
                return Page();
            }
        }
    }
}
