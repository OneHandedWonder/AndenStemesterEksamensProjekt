using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;

namespace AndenStemesterEksamensProjekt.Pages.Teams
{
    public class ManageModel : PageModel
    {
        private readonly TeamService _teamService;

        public ManageModel(TeamService teamService)
        {
            _teamService = teamService;
        }

        public List<Team> Teams { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string CurrentUserRole { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if user is a guest - guests cannot access teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "guest")
            {
                TempData["ErrorMessage"] = "Gæster har ikke adgang til teams.";
                return RedirectToPage("/Index");
            }

            CurrentUserRole = userRole ?? string.Empty;
            Teams = await _teamService.GetAllTeamsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int teamId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Only planners and admins can delete teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Kun plannere og administratorer kan slette teams.";
                return RedirectToPage();
            }

            var success = await _teamService.DeleteTeamAsync(teamId);
            if (success)
            {
                TempData["SuccessMessage"] = "Team slettet succesfuldt!";
            }
            else
            {
                TempData["ErrorMessage"] = "Team blev ikke fundet.";
            }

            return RedirectToPage();
        }
    }
}
