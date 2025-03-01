using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarDB.Models
{
    public class Auction
    {
        [Key]
        public int ScanId { get; set; }

        [ForeignKey("ScanId")]
        public ScanMeta? ScanMeta { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(32)")]
        public string? ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item? Item { get; set; }

        public DateTime Ts { get; set; }

        [Column(TypeName = "VARCHAR(64)")]
        public string? Seller { get; set; }

        public byte TimeLeft { get; set; }

        public short ItemCount { get; set; }

        public int MinBid { get; set; }

        public int Buyout { get; set; }

        public int CurBid { get; set; }
    }
}
