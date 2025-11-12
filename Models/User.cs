using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndenStemesterEksamensProjekt.Models
{
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
    }
}
