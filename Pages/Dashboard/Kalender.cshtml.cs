using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;
using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;

namespace AndenStemesterEksamensProjekt.Pages.Dashboard
{
    public class KalenderModel : PageModel
    {
        private readonly EventService _eventService;
        private readonly ApplicationDbContext _context;

        public KalenderModel(EventService eventService, ApplicationDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }

        public List<CalendarEvent> Events { get; set; } = new();
        public List<User> AllUsers { get; set; } = new();
        public Dictionary<int, List<EventParticipant>> EventParticipants { get; set; } = new();
        public int CurrentUserId { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public string CurrentMonthName { get; set; } = string.Empty;

        [BindProperty]
        public CalendarEvent NewEvent { get; set; } = new();

        [BindProperty]
        public List<int> SelectedParticipants { get; set; } = new();

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

            // Store current user ID for view
            CurrentUserId = userId.Value;

            // Get events for current month
            Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, CurrentYear, CurrentMonth);

            // Load all users for participant selection
            AllUsers = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Email).ToListAsync();

            // Load participants for each event
            foreach (var evt in Events)
            {
                var participants = await _eventService.GetEventParticipantsAsync(evt.EventId);
                EventParticipants[evt.EventId] = participants;
            }

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
                var createdEvent = await _eventService.CreateEventAsync(NewEvent);
                
                // Add selected participants
                if (SelectedParticipants != null && SelectedParticipants.Any())
                {
                    foreach (var participantId in SelectedParticipants)
                    {
                        await _eventService.AddParticipantAsync(createdEvent.EventId, participantId, userId.Value);
                    }
                }
                
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

        public async Task<IActionResult> OnPostAddParticipantAsync(int eventId, List<int> participantUserId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                if (participantUserId == null || !participantUserId.Any())
                {
                    ErrorMessage = "Vælg mindst én deltager.";
                    return RedirectToPage();
                }

                int successCount = 0;
                foreach (var participantId in participantUserId)
                {
                    var success = await _eventService.AddParticipantAsync(eventId, participantId, userId.Value);
                    if (success) successCount++;
                }

                if (successCount > 0)
                {
                    SuccessMessage = $"{successCount} deltager{(successCount > 1 ? "e" : "")} tilføjet succesfuldt!";
                }
                else
                {
                    ErrorMessage = "Kunne ikke tilføje nogen deltagere.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved tilføjelse af deltagere: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveParticipantAsync(int eventId, int participantUserId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var success = await _eventService.RemoveParticipantAsync(eventId, participantUserId);
                if (success)
                {
                    SuccessMessage = "Deltager fjernet succesfuldt!";
                }
                else
                {
                    ErrorMessage = "Kunne ikke fjerne deltager.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved fjernelse af deltager: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
