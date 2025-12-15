using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndenStemesterEksamensProjekt.Models
{
    [Table("profiles")]
    public class Profile
    {
        [Key]
        [Column("puid")]
        public int Uid { get; set; }

        [Column("navn")]
        [Required]
        [MaxLength(255)]
        public string Navn { get; set; } = string.Empty;

        [Column("adresse")]
        [MaxLength(500)]
        public string? Adresse { get; set; }

        [Column("mobil_nr")]
        [MaxLength(20)]
        public string? MobilNr { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property til User
        [Column("uid")]
        public int uid { get; set; }
    }
}
