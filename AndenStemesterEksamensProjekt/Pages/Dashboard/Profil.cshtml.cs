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
    public List<Team> Teams { get; set; } = new();

        /// <summary>
        /// Constructor - Injicerer nødvendige services
        /// </summary>
        /// <param name="dbService">Database service til datahåndtering</param>
        /// <param name="logger">Logger til fejlhåndtering</param>
        public DashboardModel(DatabaseService dbService, ILogger<DashboardModel> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        /// <summary>
        /// Håndterer GET request til profilsiden
        /// Henter brugerdata, profil og tilknyttede hold fra databasen
        /// </summary>
        /// <returns>Redirect til login hvis ikke logget ind, ellers viser siden</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Hent bruger ID fra session
            userId = HttpContext.Session.GetInt32("UserId");

            // Tjek om brugeren er logget ind
            if (!userId.HasValue)
            {
                return RedirectToPage("../Login");
            }

            // Hent brugeren fra databasen
            CurrentUser = await _dbService.GetUserByIdAsync(userId.Value);

            // Hvis brugeren ikke findes, log advarsel og redirect til login
            if (CurrentUser == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return RedirectToPage("../Login");
            }

            // Hent brugerens profil
            CurrentProfile = await _dbService.GetProfileAsync(userId.Value);
            
            // Hent alle hold som brugeren er medlem af
            Teams = await _dbService.GetUserTeamsAsync(userId.Value);

            return Page();
        }
        
        /// <summary>
        /// Håndterer POST request for at navigere til hold administration
        /// </summary>
        /// <returns>Redirect til Teams/Manage siden</returns>
        public async Task<IActionResult> OnPostManageTeams()
        {
            await Task.CompletedTask;
            return RedirectToPage("/Teams/Manage");
        }
    }
}

