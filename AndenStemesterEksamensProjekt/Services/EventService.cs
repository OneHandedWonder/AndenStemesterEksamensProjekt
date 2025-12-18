using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;
// Lavet af:
// Emil
namespace AndenStemesterEksamensProjekt.Services
{
    public class EventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hent alle begivenheder for en specifik bruger
        /// Inkluderer både begivenheder oprettet af brugeren OG begivenheder hvor brugeren er deltager
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsAsync(int userId)
        {
            // Hent begivenhed ID'er hvor brugeren er deltager
            var participantEventIds = await _context.EventParticipants
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.EventId)
                .ToListAsync();

            // Get events created by user OR where user is a participant
            return await _context.CalendarEvents
                .Where(e => e.UserId == userId || participantEventIds.Contains(e.EventId))
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Hent begivenheder for en specifik bruger inden for en datoperiode
        /// Inkluderer både begivenheder oprettet af brugeren OG begivenheder hvor brugeren er deltager
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            // Hent begivenhed ID'er hvor brugeren er deltager
            var participantEventIds = await _context.EventParticipants
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.EventId)
                .ToListAsync();

            // Hent begivenheder oprettet af brugeren ELLER hvor brugeren er deltager
            return await _context.CalendarEvents
                .Where(e => (e.UserId == userId || participantEventIds.Contains(e.EventId)) &&
                           e.StartTime >= startDate && 
                           e.StartTime <= endDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Hent en specifik begivenhed efter ID
        /// </summary>
        public async Task<CalendarEvent?> GetEventByIdAsync(int eventId, int userId)
        {
            return await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);
        }

        /// <summary>
        /// Opret en ny kalenderbegivenhed
        /// </summary>
        public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
        {
            // Sikr at tider er i UTC
            calendarEvent.StartTime = DateTime.SpecifyKind(calendarEvent.StartTime, DateTimeKind.Utc);
            calendarEvent.EndTime = DateTime.SpecifyKind(calendarEvent.EndTime, DateTimeKind.Utc);
            calendarEvent.CreatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedAt = DateTime.UtcNow;

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();
            
            return calendarEvent;
        }

        /// <summary>
        /// Opdater en eksisterende begivenhed
        /// </summary>
        public async Task<bool> UpdateEventAsync(CalendarEvent calendarEvent)
        {
            var existingEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == calendarEvent.EventId && e.UserId == calendarEvent.UserId);

            if (existingEvent == null)
                return false;

            existingEvent.Title = calendarEvent.Title;
            existingEvent.Description = calendarEvent.Description;
            existingEvent.StartTime = DateTime.SpecifyKind(calendarEvent.StartTime, DateTimeKind.Utc);
            existingEvent.EndTime = DateTime.SpecifyKind(calendarEvent.EndTime, DateTimeKind.Utc);
            existingEvent.Location = calendarEvent.Location;
            existingEvent.IsAllDay = calendarEvent.IsAllDay;
            existingEvent.Color = calendarEvent.Color;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Slet en begivenhed
        /// </summary>
        public async Task<bool> DeleteEventAsync(int eventId, int userId)
        {
            var calendarEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);

            if (calendarEvent == null)
                return false;

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Hent begivenheder for nuværende måned
        /// </summary>
        public async Task<List<CalendarEvent>> GetCurrentMonthEventsAsync(int userId, int year, int month)
        {
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            return await GetUserEventsByDateRangeAsync(userId, startDate, endDate);
        }

        /// <summary>
        /// Tilføj en deltager til en begivenhed
        /// </summary>
        public async Task<bool> AddParticipantAsync(int eventId, int userId, int requestingUserId)
        {
            // Tjek om begivenheden eksisterer og den anmodende bruger ejer den eller er admin
            var calendarEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (calendarEvent == null)
                return false;

            // Tjek om brugeren allerede deltager
            var existingParticipant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (existingParticipant != null)
                return false; // Deltager allerede

            var participant = new EventParticipant
            {
                EventId = eventId,
                UserId = userId,
                Status = "pending",
                JoinedAt = DateTime.UtcNow
            };

            _context.EventParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Fjern en deltager fra en begivenhed
        /// </summary>
        public async Task<bool> RemoveParticipantAsync(int eventId, int userId)
        {
            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (participant == null)
                return false;

            _context.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Opdater deltager status (accepter/afvis)
        /// </summary>
        public async Task<bool> UpdateParticipantStatusAsync(int eventId, int userId, string status)
        {
            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (participant == null)
                return false;

            participant.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Hent alle deltagere for en begivenhed
        /// </summary>
        public async Task<List<EventParticipant>> GetEventParticipantsAsync(int eventId)
        {
            return await _context.EventParticipants
                .Include(p => p.User)
                .Where(p => p.EventId == eventId)
                .ToListAsync();
        }

        /// <summary>
        /// Hent alle begivenheder en bruger deltager i
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserParticipatingEventsAsync(int userId)
        {
            return await _context.EventParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .Select(p => p.Event!)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }
        /// <summary>
        /// Hent overlappende begivenheder for givne deltagere
        /// </summary>
        public async Task<List<CalendarEvent>> GetOverlappingEventsAsync(CalendarEvent newEvent, List<int> participantIds)
        {
            var overlappingEvents = new List<CalendarEvent>();

            foreach (var participantId in participantIds)
            {
                var participantEventIds = await _context.EventParticipants
                    .Where(ep => ep.UserId == participantId)
                    .Select(ep => ep.EventId)
                    .ToListAsync();

                var events = await _context.CalendarEvents
                    .Where(e => participantEventIds.Contains(e.EventId) &&
                                ((newEvent.StartTime < e.EndTime) && (newEvent.EndTime > e.StartTime)))
                    .ToListAsync();

                overlappingEvents.AddRange(events);
            }

            return overlappingEvents.Distinct().ToList();
        }
    }
}
