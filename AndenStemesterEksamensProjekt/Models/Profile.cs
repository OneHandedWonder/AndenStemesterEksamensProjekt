using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Lavet af:
// Sophie
namespace AndenStemesterEksamensProjekt.Models
{
    /// <summary>
    /// Profil klasse der repræsenterer en brugers personlige information
    /// </summary>
    [Table("profiles")]
    public class Profile
    {
        /// <summary>
        /// Primær nøgle - Bruger ID (samme som User.Uid)
        /// </summary>
        [Key]
        [Column("uid")]
        public int Uid { get; set; }

        /// <summary>
        /// Brugerens fulde navn (obligatorisk, maks 255 tegn)
        /// </summary>
        [Column("navn")]
        [Required]
        [MaxLength(255)]
        public string Navn { get; set; } = string.Empty;

        /// <summary>
        /// Brugerens adresse (valgfri, maks 500 tegn)
        /// </summary>
        [Column("adresse")]
        [MaxLength(500)]
        public string? Adresse { get; set; }

        /// <summary>
        /// Brugerens mobilnummer (valgfri, maks 20 tegn)
        /// </summary>
        [Column("mobil_nr")]
        [MaxLength(20)]
        public string? MobilNr { get; set; }

        /// <summary>
        /// Tidspunkt for hvornår profilen blev oprettet
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tidspunkt for seneste opdatering af profilen
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
