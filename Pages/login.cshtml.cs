using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AndenStemesterEksamensProjekt.Pages
{
    public class loginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // TODO: Add authentication logic here
            // For now, just redirect to home page
            return RedirectToPage("/Index");
        }
    }
}
