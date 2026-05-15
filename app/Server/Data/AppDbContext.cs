using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Security> Securities { get; set; }
    public DbSet<Holding> Holdings { get; set; }
    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── App User ──────────────────────────────────────────────────
        modelBuilder.Entity<AppUser>().ToTable("app_user");

        // ── Portfolio ─────────────────────────────────────────────────
        modelBuilder.Entity<Portfolio>().ToTable("portfolio");
        modelBuilder.Entity<Portfolio>()
            .HasOne(p => p.AppUser)
            .WithOne(u => u.Portfolio)
            .HasForeignKey<Portfolio>(p => p.AppUserId);
        modelBuilder.Entity<Portfolio>()
            .Property(p => p.CashBalance)
            .HasColumnType("decimal(18,4)");

        // ── Security ─────────────────────────────────────────────────
        modelBuilder.Entity<Security>().ToTable("security");
        modelBuilder.Entity<Security>().HasKey(s => s.Ticker);
        modelBuilder.Entity<Security>()
            .Property(s => s.AssetClass)
            .HasConversion<string>();
        modelBuilder.Entity<Security>()
            .Property(s => s.LastPrice)
            .HasColumnType("decimal(18,4)");

        // ── Holding ─────────────────────────────────────────────────
        modelBuilder.Entity<Holding>().ToTable("holding");
        modelBuilder.Entity<Holding>().Property(h => h.Quantity)
            .HasColumnType("decimal(18,4)");
        modelBuilder.Entity<Holding>().Property(h => h.AvgCost)
            .HasColumnType("decimal(18,4)");
        modelBuilder.Entity<Holding>()
            .HasIndex(h => new { h.PortfolioId, h.Ticker })
            .IsUnique();

        // ── Trade ─────────────────────────────────────────────────
        modelBuilder.Entity<Trade>().ToTable("trade");
        modelBuilder.Entity<Trade>().Property(t => t.TradeType)
            .HasConversion<string>();
        modelBuilder.Entity<Trade>().Property(t => t.Quantity)
            .HasColumnType("decimal(18,4)");
        modelBuilder.Entity<Trade>().Property(t => t.PricePerShare)
            .HasColumnType("decimal(18,4)");
        modelBuilder.Entity<Trade>().Property(t => t.TotalValue)
            .HasColumnType("decimal(18,4)");
    }
}