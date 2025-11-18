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
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsAsync(int userId)
        {
            return await _context.CalendarEvents
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get events for a specific user within a date range
        /// </summary>
        public async Task<List<CalendarEvent>> GetUserEventsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.CalendarEvents
                .Where(e => e.UserId == userId && 
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
    }
}
