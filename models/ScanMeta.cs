using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarDB.Models
{
    public class ScanMeta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(16)")]
        public string? Realm { get; set; }

        [Required]
        [Column(TypeName = "ENUM('Neutral', 'Alliance', 'Horde')")]
        public string? Faction { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(64)")]
        public string? Scanner { get; set; }

        public DateTime Ts { get; set; }

        public ICollection<Auction>? Auctions { get; set; }
    }
}
