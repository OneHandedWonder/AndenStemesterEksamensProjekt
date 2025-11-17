using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Services;
using AndenStemesterEksamensProjekt.Models;

namespace AndenStemesterEksamensProjekt.Pages.Dashboard;
public class DashboardModel : PageModel
{
    private readonly DatabaseService _dbService;
    private readonly ILogger<DashboardModel> _logger;
    
    public int? userId { get; set; }
    public User? CurrentUser { get; set; }
    public Profile? CurrentProfile { get; set; }

    public DashboardModel(DatabaseService dbService, ILogger<DashboardModel> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return RedirectToPage("../Login");
        }

        // Hent brugeren fra databasen
        CurrentUser = await _dbService.GetUserByIdAsync(userId.Value);

        if (CurrentUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return RedirectToPage("../Login");
        }

        CurrentProfile = await _dbService.GetprofileAsync(userId.Value);

        return Page();
    }
}

