using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;

namespace AndenStemesterEksamensProjekt.Pages.Dashboard
{
    public class KalenderModel : PageModel
    {
        private readonly EventService _eventService;

        public KalenderModel(EventService eventService)
        {
            _eventService = eventService;
        }

        public List<CalendarEvent> Events { get; set; } = new();
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public string CurrentMonthName { get; set; } = string.Empty;

        [BindProperty]
        public CalendarEvent NewEvent { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? year, int? month)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Set current year and month
            CurrentYear = year ?? DateTime.Now.Year;
            CurrentMonth = month ?? DateTime.Now.Month;
            CurrentMonthName = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");

            // Get events for current month
            Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, CurrentYear, CurrentMonth);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateEventAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Udfyld venligst alle påkrævede felter.";
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, DateTime.Now.Year, DateTime.Now.Month);
                return Page();
            }

            // Validate end time is after start time
            if (NewEvent.EndTime <= NewEvent.StartTime)
            {
                ErrorMessage = "Sluttidspunkt skal være efter starttidspunkt.";
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, DateTime.Now.Year, DateTime.Now.Month);
                return Page();
            }

            try
            {
                NewEvent.UserId = userId.Value;
                await _eventService.CreateEventAsync(NewEvent);
                SuccessMessage = "Event oprettet succesfuldt!";
                
                // Redirect to refresh the page
                return RedirectToPage(new { year = NewEvent.StartTime.Year, month = NewEvent.StartTime.Month });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved oprettelse af event: {ex.Message}";
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, DateTime.Now.Year, DateTime.Now.Month);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteEventAsync(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var success = await _eventService.DeleteEventAsync(eventId, userId.Value);
                if (success)
                {
                    SuccessMessage = "Event slettet succesfuldt!";
                }
                else
                {
                    ErrorMessage = "Event blev ikke fundet.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved sletning af event: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
