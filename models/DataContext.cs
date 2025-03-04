using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WarDB.ViewModels;

namespace WarDB.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<ScanMeta> ScanMeta { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<DisenchantResult> DisenchantResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>()
                .HasKey(a => new { a.ScanId, a.ItemId });

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.ScanMeta)
                .WithMany(s => s.Auctions)
                .HasForeignKey(a => a.ScanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Item)
                .WithMany(i => i.Auctions)
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DisenchantResult>().HasNoKey();
        }
    }
}
