using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndenStemesterEksamensProjekt.Models
{
    public enum UserRole
    {
        Guest,
        Student,
        Lecturer,
        Planner,
        Censor,
        Admin
    }

    [Table("users")]
    public class User
    {
        [Key]
        [Column("uid")]
        public int Uid { get; set; }
        
        [Column("email")]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Column("firstname")]
        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Column("lastname")]
        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;
        
        [Column("password_hash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = "guest";

        // Helper property for type-safe role access
        [NotMapped]
        public UserRole RoleEnum
        {
            get => Enum.TryParse<UserRole>(Role, true, out var role) ? role : UserRole.Guest;
            set => Role = value.ToString().ToLower();
        }
    }
}
