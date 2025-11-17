using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndenStemesterEksamensProjekt.Models
{
    [Table("sessions")]
    public class Session
    {
        [Key]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("uid")]
        public int Uid { get; set; }

        [Column("session_token")]
        [MaxLength(32)]
        public string SessionToken { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [ForeignKey("Uid")]
        public User? User { get; set; }
    }
}
