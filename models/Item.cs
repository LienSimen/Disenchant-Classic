using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WarDB.Models
{
    public class Item
    {
        [Key]
        [Column(TypeName = "VARCHAR(32)")]
        public string? Id { get; set; }

        public int ShortId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(128)")]
        public string? Name { get; set; }

        public int SellPrice { get; set; }

        public int StackCount { get; set; }

        public int ClassID { get; set; }

        public int SubClassID { get; set; }

        public int Rarity { get; set; }

        public int MinLevel { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string? Link { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string? OLink { get; set; }

        public DateTime Ts { get; set; }

        // navigation property test- item has many auctions
        public ICollection<Auction>? Auctions { get; set; }
    }
}
