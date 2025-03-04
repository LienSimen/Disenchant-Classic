namespace WarDB.ViewModels
{
    public class SearchResultViewModel
    {
        // Item details
        public string? ItemId { get; set; }
        public string? Name { get; set; }
        public int MinLevel { get; set; }
        public int Rarity { get; set; }
        public int ClassID { get; set; }
        public int SubClassID { get; set; }
        public int SellPrice { get; set; }
        public int StackCount { get; set; }

        // Auction aggregated data
        public int? TotalListings { get; set; }
        public int? TotalQuantity { get; set; }
        public int? MinBid { get; set; }
        public int? MaxBid { get; set; }
        public double? AvgBid { get; set; }
        public int? MinBuyout { get; set; }
        public int? MaxBuyout { get; set; }
        public double? AvgBuyout { get; set; }

        // Price trend (most recent auction)
        public DateTime? RecentScanTime { get; set; }
        public double? RecentPrice { get; set; }

        // Additional formatted fields
        public string VendorPriceFormatted { get; set; } = string.Empty;
        public string LatestMinBuyoutFormatted { get; set; } = string.Empty;
        public string PreviousMinBuyoutFormatted { get; set; } = string.Empty;
        public string PriceDifferenceFormatted { get; set; } = string.Empty;
    }
}
