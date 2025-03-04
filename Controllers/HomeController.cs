using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarDB.Models;
using WarDB.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace WarDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        public HomeController(DataContext context)
        {
            _context = context;
        }

        // Helper to convert copper to gold/silver/copper.
        private string ConvertCopper(int copper)
        {
            int gold = copper / 10000;
            int silver = (copper % 10000) / 100;
            int copperRemainder = copper % 100;
            return $"{gold}g {silver}s {copperRemainder}c";
        }

        // Helper to compare two prices.
        private string FormatPriceDifference(int latest, int previous)
        {
            int diff = latest - previous;
            if (diff == 0)
                return "No Change";
            string diffFormatted = ConvertCopper(Math.Abs(diff));
            return diff > 0 ? $"Up by {diffFormatted}" : $"Down by {diffFormatted}";
        }

        public async Task<IActionResult> Index(string query, string searchType)
        {
            // Start the stopwatch for performance measurements.
            var stopwatch = Stopwatch.StartNew();

            ViewBag.Query = query;
            ViewBag.SearchType = searchType;

            if (string.IsNullOrEmpty(query))
                return View(Enumerable.Empty<SearchResultViewModel>());

            // 1. Retrieve filtered items.
            var filteredItemsList = searchType == "precise"
                ? await _context.Items.AsNoTracking().Where(i => i.Name == query).ToListAsync()
                : await _context.Items.AsNoTracking().Where(i => i.Name.Contains(query)).ToListAsync();
            Console.WriteLine($"[Timing] FilteredItemsList query: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            if (filteredItemsList.Count == 0)
                return View(Enumerable.Empty<SearchResultViewModel>());

            // Get the IDs of the filtered items for further filtering.
            var filteredItemIds = filteredItemsList.Select(i => i.Id).ToList();

            // 2. Get latest scan id.
            var latestScanId = await _context.ScanMeta.AsNoTracking().MaxAsync(s => s.Id);
            Console.WriteLine($"[Timing] LatestScanId query: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 3. Retrieve auctions from the latest scan for only filtered items.
            var auctionsLatest = await _context.Auctions.AsNoTracking()
                .Where(a => a.Buyout > 0 && a.ScanId == latestScanId && a.ItemCount != 0
                            && filteredItemIds.Contains(a.ItemId))
                .Select(a => new { a.ItemId, a.Buyout, a.MinBid, a.ItemCount })
                .ToListAsync();
            Console.WriteLine($"[Timing] AuctionsLatest query: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 4. Group latest auctions into aggregated data.
            var auctionData = (from a in auctionsLatest
                               join i in filteredItemsList on a.ItemId equals i.Id
                               group a by a.ItemId into g
                               select new
                               {
                                   ItemId = g.Key,
                                   TotalListings = g.Count(),
                                   TotalQuantity = g.Sum(a => a.ItemCount),
                                   MinUnitBuyout = g.Min(a => (decimal)a.Buyout / a.ItemCount),
                                   AvgUnitBuyout = g.Average(a => (decimal)a.Buyout / a.ItemCount),
                                   MinUnitBid = g.Min(a => (decimal)a.MinBid / a.ItemCount)
                               }).ToList();
            Console.WriteLine($"[Timing] AuctionData grouping: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 5. Retrieve auctions from all scans for price trend, filtered by items.
            var auctionsPriceTrend = await _context.Auctions.AsNoTracking()
                .Where(a => a.Buyout > 0 && a.ItemCount != 0 && filteredItemIds.Contains(a.ItemId))
                .Select(a => new { a.ItemId, a.Buyout, a.ItemCount, a.Ts })
                .ToListAsync();
            Console.WriteLine($"[Timing] AuctionsPriceTrend query: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 6. Group price trend data.
            var priceTrendData = (from a in auctionsPriceTrend
                                  orderby a.Ts descending
                                  group a by a.ItemId into g
                                  let latestAuction = g.FirstOrDefault()
                                  select new
                                  {
                                      ItemId = g.Key,
                                      RecentScanTime = latestAuction != null ? latestAuction.Ts : (DateTime?)null,
                                      RecentPrice = (latestAuction != null && latestAuction.ItemCount != 0)
                                                    ? (decimal)latestAuction.Buyout / latestAuction.ItemCount
                                                    : (decimal?)null
                                  }).ToList();
            Console.WriteLine($"[Timing] PriceTrendData grouping: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 7. Retrieve the second latest scan id.
            var secondLatestScanId = await _context.ScanMeta.AsNoTracking()
                .Where(s => s.Id < latestScanId)
                .MaxAsync(s => (int?)s.Id);
            Console.WriteLine($"[Timing] SecondLatestScanId query: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 8. Retrieve previous auctions for filtered items and group previous auction data.
            List<dynamic> previousAuctionData = new List<dynamic>();
            if (secondLatestScanId.HasValue)
            {
                var previousAuctions = await _context.Auctions.AsNoTracking()
                    .Where(a => a.Buyout > 0 && a.ScanId == secondLatestScanId.Value && a.ItemCount != 0
                                && filteredItemIds.Contains(a.ItemId))
                    .Select(a => new { a.ItemId, a.Buyout, a.ItemCount })
                    .ToListAsync();

                previousAuctionData = (from a in previousAuctions
                                       group a by a.ItemId into g
                                       select new
                                       {
                                           ItemId = g.Key,
                                           PreviousMinUnitBuyout = g.Min(a => (decimal)a.Buyout / a.ItemCount)
                                       }).ToList<dynamic>();
            }
            Console.WriteLine($"[Timing] PreviousAuctionData grouping: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();

            // 9. Final in-memory join to produce the view model.
            var results = (from item in filteredItemsList
                           join ad in auctionData on item.Id equals ad.ItemId into adGroup
                           from ad in adGroup.DefaultIfEmpty()
                           join pt in priceTrendData on item.Id equals pt.ItemId into ptGroup
                           from pt in ptGroup.DefaultIfEmpty()
                           join prev in previousAuctionData on item.Id equals prev.ItemId into prevGroup
                           from prev in prevGroup.DefaultIfEmpty()
                           select new SearchResultViewModel
                           {
                               ItemId = item.Id,
                               Name = item.Name,
                               MinLevel = item.MinLevel,
                               Rarity = item.Rarity,
                               ClassID = item.ClassID,
                               SubClassID = item.SubClassID,
                               SellPrice = item.SellPrice,
                               StackCount = item.StackCount,
                               TotalListings = ad?.TotalListings,
                               TotalQuantity = ad?.TotalQuantity,
                               MinBid = ad == null ? (int?)null : (int)decimal.Round(ad.MinUnitBid, 0),
                               MinBuyout = ad == null ? (int?)null : (int)decimal.Round(ad.MinUnitBuyout, 0),
                               AvgBuyout = ad == null ? (double?)null : (double)decimal.Round(ad.AvgUnitBuyout, 0),
                               RecentScanTime = pt?.RecentScanTime,
                               RecentPrice = pt == null || pt.RecentPrice == null
                                             ? (double?)null
                                             : (double)decimal.Round(pt.RecentPrice.Value, 0),
                               VendorPriceFormatted = ConvertCopper(item.SellPrice),
                               LatestMinBuyoutFormatted = ad == null
                                   ? "N/A"
                                   : ConvertCopper((int)decimal.Round(ad.MinUnitBuyout, 0)),
                               PreviousMinBuyoutFormatted = prev == null
                                   ? "N/A"
                                   : ConvertCopper((int)decimal.Round(prev.PreviousMinUnitBuyout, 0)),
                               PriceDifferenceFormatted = (ad == null || prev == null)
                                   ? "N/A"
                                   : FormatPriceDifference(
                                        (int)decimal.Round(ad.MinUnitBuyout, 0),
                                        (int)decimal.Round(prev.PreviousMinUnitBuyout, 0))
                           }).ToList();
            Console.WriteLine($"[Timing] Final join and projection: {stopwatch.ElapsedMilliseconds} ms");

            return View(results);
        }
    }
}
