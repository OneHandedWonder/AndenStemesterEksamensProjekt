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

        public async Task OnGetAsync()
        {
            // Check for valid session cookie
            var sessionToken = Request.Cookies["SessionToken"];
            if (!string.IsNullOrEmpty(sessionToken))
            {
                var userId = await _dbService.ValidateSessionAsync(sessionToken);
                if (userId.HasValue)
                {
                    // Valid session found - get user and redirect
                    var user = await _dbService.GetUserByIdAsync(userId.Value);
                    if (user != null)
                    {
                        HttpContext.Session.SetInt32("UserId", user.Uid);
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetString("SessionToken", sessionToken);
                        
                        // Redirect based on role
                        if (user.Role == "guest")
                        {
                            Response.Redirect("/Index");
                        }
                        else
                        {
                            Response.Redirect("/Dashboard/Profil");
                        }
                    }
                }
            }
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

                // Create session token
                var sessionToken = await _dbService.CreateSessionAsync(user.Uid);

                // Set session
                HttpContext.Session.SetInt32("UserId", user.Uid);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("SessionToken", sessionToken);

                // Set session token in cookie for persistence
                Response.Cookies.Append("SessionToken", sessionToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

                _logger.LogInformation("User {Email} logged in successfully with session token {SessionToken}", user.Email, sessionToken);

                // Redirect to home page
                if (user.Role == "guest")
                {
                    return RedirectToPage("/Index");
                } else {
                    return RedirectToPage("Dashboard/Profil");
                }
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
