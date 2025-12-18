using Microsoft.EntityFrameworkCore;
using AndenStemesterEksamensProjekt.Data;
using AndenStemesterEksamensProjekt.Models;
using AndenStemesterEksamensProjekt.Services;

namespace AndenStemesterEksamensProjekt.Tests
{
    public class EventServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _eventService = new EventService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUserEventsAsync_ReturnsEventsCreatedByUser()
        {
            // Arrange - Klargør
            var user = await CreateTestUser("creator@test.com");
            var event1 = new CalendarEvent
            {
                Title = "Event 1",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var event2 = new CalendarEvent
            {
                Title = "Event 2",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.AddRange(event1, event2);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _eventService.GetUserEventsAsync(user.Uid);

            // Assert - Bekræft
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(user.Uid, e.UserId));
        }

        [Fact]
        public async Task GetUserEventsAsync_ReturnsEventsWhereUserIsParticipant()
        {
            // Arrange - Klargør
            var creator = await CreateTestUser("creator@test.com");
            var participant = await CreateTestUser("participant@test.com");

            var calendarEvent = new CalendarEvent
            {
                Title = "Event",
                UserId = creator.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            var eventParticipant = new EventParticipant
            {
                EventId = calendarEvent.EventId,
                UserId = participant.Uid
            };
            _context.EventParticipants.Add(eventParticipant);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _eventService.GetUserEventsAsync(participant.Uid);

            // Assert - Bekræft
            Assert.Single(result);
            Assert.Equal(calendarEvent.EventId, result[0].EventId);
        }

        [Fact]
        public async Task GetUserEventsByDateRangeAsync_ReturnsEventsInRange()
        {
            // Arrange - Klargør
            var user = await CreateTestUser("user@test.com");
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var eventInRange = new CalendarEvent
            {
                Title = "In Range",
                UserId = user.Uid,
                StartTime = startDate.AddDays(3),
                EndTime = startDate.AddDays(3).AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var eventOutOfRange = new CalendarEvent
            {
                Title = "Out of Range",
                UserId = user.Uid,
                StartTime = endDate.AddDays(1),
                EndTime = endDate.AddDays(1).AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.AddRange(eventInRange, eventOutOfRange);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _eventService.GetUserEventsByDateRangeAsync(user.Uid, startDate, endDate);

            // Assert - Bekræft
            Assert.Single(result);
            Assert.Equal("In Range", result[0].Title);
        }

        [Fact]
        public async Task CreateEventAsync_CreatesNewEvent()
        {
            // Arrange - Klargør
            var user = await CreateTestUser("creator@test.com");
            var newEvent = new CalendarEvent
            {
                Title = "New Event",
                Description = "Test Description",
                Location = "Test Location",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            // Act - Udfør
            var result = await _eventService.CreateEventAsync(newEvent);

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.True(result.EventId > 0);
            Assert.Equal("New Event", result.Title);
            Assert.Equal(DateTimeKind.Utc, result.StartTime.Kind);
            Assert.Equal(DateTimeKind.Utc, result.EndTime.Kind);

            // Verificer i databasen
            var eventInDb = await _context.CalendarEvents.FindAsync(result.EventId);
            Assert.NotNull(eventInDb);
            Assert.Equal("New Event", eventInDb.Title);
        }

        [Fact]
        public async Task UpdateEventAsync_UpdatesExistingEvent()
        {
            // Arrange - Klargør
            var user = await CreateTestUser("creator@test.com");
            var originalEvent = new CalendarEvent
            {
                Title = "Original",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.Add(originalEvent);
            await _context.SaveChangesAsync();

            var updatedEvent = new CalendarEvent
            {
                EventId = originalEvent.EventId,
                Title = "Updated",
                Description = "Updated Description",
                Location = "Updated Location",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            // Act - Udfør
            var result = await _eventService.UpdateEventAsync(updatedEvent);

            // Assert - Bekræft
            Assert.True(result);
            var eventInDb = await _context.CalendarEvents.FindAsync(originalEvent.EventId);
            Assert.NotNull(eventInDb);
            Assert.Equal("Updated", eventInDb.Title);
            Assert.Equal("Updated Description", eventInDb.Description);
            Assert.Equal("Updated Location", eventInDb.Location);
        }

        [Fact]
        public async Task DeleteEventAsync_DeletesEvent()
        {
            // Arrange - Klargør
            var user = await CreateTestUser("creator@test.com");
            var calendarEvent = new CalendarEvent
            {
                Title = "To Delete",
                UserId = user.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _eventService.DeleteEventAsync(calendarEvent.EventId, user.Uid);

            // Assert - Bekræft
            Assert.True(result);
            var deletedEvent = await _context.CalendarEvents.FindAsync(calendarEvent.EventId);
            Assert.Null(deletedEvent);
        }

        [Fact]
        public async Task AddParticipantAsync_AddsParticipantToEvent()
        {
            // Arrange - Klargør
            var creator = await CreateTestUser("creator@test.com");
            var participant = await CreateTestUser("participant@test.com");

            var calendarEvent = new CalendarEvent
            {
                Title = "Event",
                UserId = creator.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Act - Udfør
            var result = await _eventService.AddParticipantAsync(calendarEvent.EventId, participant.Uid, creator.Uid);

            // Assert - Bekræft
            Assert.True(result);
            var eventParticipant = await _context.EventParticipants
                .FirstOrDefaultAsync(ep => ep.EventId == calendarEvent.EventId && ep.UserId == participant.Uid);
            Assert.NotNull(eventParticipant);
        }

        [Fact]
        public async Task GetEventParticipantsAsync_ReturnsAllParticipants()
        {
            // Arrange - Klargør
            var creator = await CreateTestUser("creator@test.com");
            var participant1 = await CreateTestUser("participant1@test.com");
            var participant2 = await CreateTestUser("participant2@test.com");

            var calendarEvent = new CalendarEvent
            {
                Title = "Event",
                UserId = creator.Uid,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            await _eventService.AddParticipantAsync(calendarEvent.EventId, participant1.Uid, creator.Uid);
            await _eventService.AddParticipantAsync(calendarEvent.EventId, participant2.Uid, creator.Uid);

            // Act - Udfør
            var result = await _eventService.GetEventParticipantsAsync(calendarEvent.EventId);

            // Assert - Bekræft
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.User != null && p.User.Email == "participant1@test.com");
            Assert.Contains(result, p => p.User != null && p.User.Email == "participant2@test.com");
        }

        private async Task<User> CreateTestUser(string email)
        {
            var user = new User
            {
                Email = email,
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
