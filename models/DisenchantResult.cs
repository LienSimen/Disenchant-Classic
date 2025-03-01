using System.ComponentModel.DataAnnotations.Schema;

namespace WarDB.Models
{
    public class DisenchantResult
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        [Column("item_level")]
        public int ItemLevel { get; set; }

        [Column("min_bid_raw")]
        public int MinBidRaw { get; set; }

        [Column("min_buyout_raw")]
        public int MinBuyoutRaw { get; set; }

        [Column("disenchant_value_raw")]
        public decimal DisenchantValueRaw { get; set; }

        [Column("profit_vs_minBid")]
        public decimal ProfitVsMinBid { get; set; }

        [Column("profit_vs_buyout")]
        public decimal ProfitVsBuyout { get; set; }

    }
}
