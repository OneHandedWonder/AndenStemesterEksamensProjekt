using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;

namespace AndenStemesterEksamensProjekt.Pages.Teams
{
    public class DetailsModel : PageModel
    {
        private readonly TeamService _teamService;

        public DetailsModel(TeamService teamService)
        {
            _teamService = teamService;
        }

        public Team Team { get; set; } = null!;
        public List<User> Members { get; set; } = new();
        public string CurrentUserRole { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "guest")
            {
                TempData["ErrorMessage"] = "Gæster har ikke adgang til teams.";
                return RedirectToPage("/Index");
            }

            CurrentUserRole = userRole ?? string.Empty;

            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                TempData["ErrorMessage"] = "Hold ikke fundet.";
                return RedirectToPage("/Teams/Manage");
            }

            Team = team;
            Members = await _teamService.GetTeamMembersAsync(id);

            return Page();
        }
    }
}
