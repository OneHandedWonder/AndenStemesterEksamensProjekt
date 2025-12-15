using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;
using AndenStemesterEksamensProjekt.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AndenStemesterEksamensProjekt.Pages.Teams
{
    public class CreateModel : PageModel
    {
        private readonly TeamService _teamService;
        private readonly ApplicationDbContext _context;

        public CreateModel(TeamService teamService, ApplicationDbContext context)
        {
            _teamService = teamService;
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Navn er påkrævet")]
        [MaxLength(100, ErrorMessage = "Navn må maks være 100 tegn")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public List<int> SelectedMembers { get; set; } = new();

        public List<User> AllUsers { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Only planners and admins can create teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Kun plannere og administratorer kan oprette teams.";
                return RedirectToPage("/Teams/Manage");
            }

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

            // Only planners and admins can create teams
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Kun plannere og administratorer kan oprette teams.";
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
                // Create the team
                var team = await _teamService.CreateTeamAsync(Name, Description);

                // Add selected members
                if (SelectedMembers != null && SelectedMembers.Any())
                {
                    foreach (var memberId in SelectedMembers)
                    {
                        await _teamService.AddUserToTeamAsync(team.Id, memberId);
                    }
                }

                TempData["SuccessMessage"] = $"Hold '{team.Name}' oprettet succesfuldt med {SelectedMembers?.Count ?? 0} medlemmer!";
                return RedirectToPage("/Teams/Manage");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved oprettelse af hold: {ex.Message}";
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
