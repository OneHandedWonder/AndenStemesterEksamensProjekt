using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;

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
        /// Get all events for a specific user
        /// Includes both events created by the user AND events where the user is a participant
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsAsync(int userId)
        {
            // Get event IDs where user is a participant
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
        /// Get events for a specific user within a date range
        /// Includes both events created by the user AND events where the user is a participant
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            // Get event IDs where user is a participant
            var participantEventIds = await _context.EventParticipants
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.EventId)
                .ToListAsync();

            // Get events created by user OR where user is a participant
            return await _context.CalendarEvents
                .Where(e => (e.UserId == userId || participantEventIds.Contains(e.EventId)) &&
                           e.StartTime >= startDate && 
                           e.StartTime <= endDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific event by ID
        /// </summary>
        public async Task<CalendarEvent?> GetEventByIdAsync(int eventId, int userId)
        {
            return await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);
        }

        /// <summary>
        /// Create a new calendar event
        /// </summary>
        public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
        {
            // Ensure times are in UTC
            calendarEvent.StartTime = DateTime.SpecifyKind(calendarEvent.StartTime, DateTimeKind.Utc);
            calendarEvent.EndTime = DateTime.SpecifyKind(calendarEvent.EndTime, DateTimeKind.Utc);
            calendarEvent.CreatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedAt = DateTime.UtcNow;

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();
            
            return calendarEvent;
        }

        /// <summary>
        /// Update an existing event
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
        /// Delete an event
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
        /// Get events for current month
        /// </summary>
        public async Task<List<CalendarEvent>> GetCurrentMonthEventsAsync(int userId, int year, int month)
        {
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            return await GetUserEventsByDateRangeAsync(userId, startDate, endDate);
        }

        /// <summary>
        /// Add a participant to an event
        /// </summary>
        public async Task<bool> AddParticipantAsync(int eventId, int userId, int requestingUserId)
        {
            // Check if event exists and requesting user owns it or is admin
            var calendarEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (calendarEvent == null)
                return false;

            // Check if user already participating
            var existingParticipant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (existingParticipant != null)
                return false; // Already participating

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
        /// Remove a participant from an event
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
        /// Update participant status (accept/decline)
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
        /// Get all participants for an event
        /// </summary>
        public async Task<List<EventParticipant>> GetEventParticipantsAsync(int eventId)
        {
            return await _context.EventParticipants
                .Include(p => p.User)
                .Where(p => p.EventId == eventId)
                .ToListAsync();
        }

        /// <summary>
        /// Get all events a user is participating in
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
    }
}
