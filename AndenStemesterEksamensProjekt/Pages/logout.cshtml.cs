using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Services;
// Lavet af:
// Emil
namespace AndenStemesterEksamensProjekt.Pages
{
    public class logoutModel : PageModel
    {
        private readonly DatabaseService _dbService;

        public logoutModel(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            // Delete all sessions for this user from database
            if (userId.HasValue)
            {
                await _dbService.DeleteUserSessionsAsync(userId.Value);
            }

            // Clear session
            HttpContext.Session.Clear();

            // Delete session cookie
            Response.Cookies.Delete("SessionToken");
            
            // Redirect to Index page
            return RedirectToPage("/Index");
        }
    }
}
