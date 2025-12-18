using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Lavet af:
// Emil & Sophie
namespace AndenStemesterEksamensProjekt.Models
{
    [Table("user_teams")]
    public class UserTeam
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("TeamId")]
        public Team Team { get; set; } = null!;
    }
}
