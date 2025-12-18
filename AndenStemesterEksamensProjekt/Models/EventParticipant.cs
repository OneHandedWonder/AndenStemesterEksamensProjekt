using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Lavet af:
// Emil
namespace AndenStemesterEksamensProjekt.Models
{
    [Table("event_participants")]
    public class EventParticipant
    {
        [Key]
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("event_id")]
        [Required]
        public int EventId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, accepted, declined

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EventId")]
        public CalendarEvent? Event { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
