using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Services;
using BCrypt.Net;

namespace AndenStemesterEksamensProjekt.Pages
{
    public class loginModel : PageModel
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<loginModel> _logger;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [TempData]
        public string? ErrorMessage { get; set; }

        public loginModel(DatabaseService dbService, ILogger<loginModel> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email og adgangskode er påkrævet.";
                return Page();
            }

            try
            {
                // Get user from database
                var user = await _dbService.GetUserByEmailAsync(Email);

                if (user == null)
                {
                    ErrorMessage = "Ugyldig email eller adgangskode.";
                    _logger.LogWarning("Login attempt failed: User not found for email {Email}", Email);
                    return Page();
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                {
                    ErrorMessage = "Ugyldig email eller adgangskode.";
                    _logger.LogWarning("Login attempt failed: Invalid password for email {Email}", Email);
                    return Page();
                }

                // Update last login
                await _dbService.UpdateLastLoginAsync(user.Uid);

                // Set session
                HttpContext.Session.SetInt32("UserId", user.Uid);
                HttpContext.Session.SetString("UserEmail", user.Email);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                // Redirect to home page
                return RedirectToPage("/Dashboard/Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email {Email}", Email);
                ErrorMessage = "Der opstod en fejl. Prøv igen senere.";
                return Page();
            }
        }
    }
}
