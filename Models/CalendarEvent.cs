using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndenStemesterEksamensProjekt.Models
{
    [Table("calendar_events")]
    public class CalendarEvent
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column("start_time")]
        [Required]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        [Required]
        public DateTime EndTime { get; set; }

        [Column("location")]
        [MaxLength(200)]
        public string? Location { get; set; }

        [Column("is_all_day")]
        public bool IsAllDay { get; set; } = false;

        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; } = "written";

        [Column("color")]
        [MaxLength(7)]
        public string Color { get; set; } = "#3788d8";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
    }
}
