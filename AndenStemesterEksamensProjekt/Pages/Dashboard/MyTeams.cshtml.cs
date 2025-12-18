using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Services;
using AndenStemesterEksamensProjekt.Models;
// Lavet af:
// Emil
namespace AndenStemesterEksamensProjekt.Pages.Dashboard
{
    public class MyTeamsModel : PageModel
    {
        private readonly DatabaseService _dbService;

        public MyTeamsModel(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public User? CurrentUser { get; set; }
        public List<Team> Teams { get; set; } = new();

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

            // Get current user
            CurrentUser = await _dbService.GetUserByIdAsync(userId.Value);
            
            // Get user's teams
            Teams = await _dbService.GetUserTeamsAsync(userId.Value);

            return Page();
        }
    }
}
