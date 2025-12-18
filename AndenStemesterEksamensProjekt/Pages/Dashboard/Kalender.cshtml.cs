using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;
using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
// Lavet af:
// Emil & Sophie
namespace AndenStemesterEksamensProjekt.Pages.Dashboard
{
    public class KalenderModel : PageModel
    {
        private readonly EventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly TeamService _teamService;

        public KalenderModel(EventService eventService, ApplicationDbContext context, TeamService teamService)
        {
            _eventService = eventService;
            _context = context;
            _teamService = teamService;
        }

        public List<CalendarEvent> Events { get; set; } = new();
        public List<User> AllUsers { get; set; } = new();

        public List<Team> AllTeams { get; set; } = new();
        public Dictionary<int, List<EventParticipant>> EventParticipants { get; set; } = new();
        public int CurrentUserId { get; set; }
        public string CurrentUserRole { get; set; } = string.Empty;
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public string CurrentMonthName { get; set; } = string.Empty;

        [BindProperty]
        public CalendarEvent NewEvent { get; set; } = new();

        [BindProperty]
        public List<int> SelectedParticipants { get; set; } = new();

        [BindProperty]
        public List<int> SelectedTeams { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? InlineErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? year, int? month)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if user is a guest - guests cannot access calendar
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "guest")
            {
                TempData["ErrorMessage"] = "Gæster har ikke adgang til kalenderen.";
                return RedirectToPage("/Index");
            }

            // Set current year and month
            CurrentYear = year ?? DateTime.Now.Year;
            CurrentMonth = month ?? DateTime.Now.Month;
            CurrentMonthName = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");

            // Store current user ID for view
            CurrentUserId = userId.Value;
            CurrentUserRole = userRole ?? string.Empty;

            // Get events for current month
            Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, CurrentYear, CurrentMonth);

            // Load all users for participant selection
            AllUsers = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Email).ToListAsync();

            // Load all teams for team selection
            AllTeams = await _teamService.GetAllTeamsAsync();

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

            // Check if user has permission to create events (only planners and admins)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                ErrorMessage = "Kun plannere og administratorer kan oprette events.";
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, DateTime.Now.Year, DateTime.Now.Month);
                return Page();
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
                InlineErrorMessage = "Sluttidspunkt skal være efter starttidspunkt.";
                // Reload page data
                CurrentYear = NewEvent.StartTime.Year;
                CurrentMonth = NewEvent.StartTime.Month;
                CurrentMonthName = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");
                CurrentUserId = userId.Value;
                CurrentUserRole = userRole ?? string.Empty;
                
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, CurrentYear, CurrentMonth);
                AllUsers = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Email).ToListAsync();
                AllTeams = await _teamService.GetAllTeamsAsync();
                
                // Load participants for each event
                foreach (var evt in Events)
                {
                    var participants = await _eventService.GetEventParticipantsAsync(evt.EventId);
                    EventParticipants[evt.EventId] = participants;
                }
                
                return Page();
            }
            // Validate that event is not overlapping with existing events for any participant
            var overlappingEvents = await _eventService.GetOverlappingEventsAsync(NewEvent, SelectedParticipants);
            if (overlappingEvents.Count > 0)
            {
                // Get participants from overlapping events who are also in the selected participants list
                var overlappingParticipantIds = new HashSet<int>();
                foreach (var evt in overlappingEvents)
                {
                    var participants = await _eventService.GetEventParticipantsAsync(evt.EventId);
                    foreach (var participant in participants)
                    {
                        if (SelectedParticipants.Contains(participant.UserId))
                        {
                            overlappingParticipantIds.Add(participant.UserId);
                        }
                    }
                }
                var overlappingUsers = await _context.Users
                    .Where(u => overlappingParticipantIds.Contains(u.Uid))
                    .Select(u => $"{u.FirstName} {u.LastName} ({u.Email})")
                    .ToListAsync();
                    
                InlineErrorMessage = $"Event overlapper med eksisterende eksaminer for: {string.Join(", ", overlappingUsers)}";
                Services.Diag_logService.Log($"Overlap detected in KalenderModel.OnPostCreateEventAsync for users: {string.Join(", ", overlappingUsers)}");
                
                // Reload page data
                CurrentYear = NewEvent.StartTime.Year;
                CurrentMonth = NewEvent.StartTime.Month;
                CurrentMonthName = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");
                CurrentUserId = userId.Value;
                CurrentUserRole = userRole ?? string.Empty;
                
                Events = await _eventService.GetCurrentMonthEventsAsync(userId.Value, CurrentYear, CurrentMonth);
                AllUsers = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Email).ToListAsync();
                AllTeams = await _teamService.GetAllTeamsAsync();
                
                // Load participants for each event
                foreach (var evt in Events)
                {
                    var participants = await _eventService.GetEventParticipantsAsync(evt.EventId);
                    EventParticipants[evt.EventId] = participants;
                }
                
                return Page();
            }

            try
            {
                NewEvent.UserId = userId.Value;
                var createdEvent = await _eventService.CreateEventAsync(NewEvent);
                
                // Collect all participant IDs from both individual users and teams
                var allParticipantIds = new HashSet<int>(SelectedParticipants ?? new List<int>());

                // Add members from selected teams
                if (SelectedTeams != null && SelectedTeams.Any())
                {
                    foreach (var teamId in SelectedTeams)
                    {
                        var teamMembers = await _teamService.GetTeamMembersAsync(teamId);
                        foreach (var member in teamMembers)
                        {
                            allParticipantIds.Add(member.Uid);
                        }
                    }
                }

                // Add all participants to the event
                if (allParticipantIds.Any())
                {
                    foreach (var participantId in allParticipantIds)
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

            // Check if user has permission to delete events (only planners and admins)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "planner" && userRole != "admin")
            {
                ErrorMessage = "Kun plannere og administratorer kan slette events.";
                return RedirectToPage();
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
