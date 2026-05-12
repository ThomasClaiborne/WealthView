using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Table names ──────────────────────────────────────────────────
        // EF Core pluralizes by default — override to match the schema
        modelBuilder.Entity<AppUser>().ToTable("app_user");
        modelBuilder.Entity<Portfolio>().ToTable("portfolio");

        // ── Decimal precision ────────────────────────────────────────────
        // Never store financial values as float/double
        modelBuilder.Entity<Portfolio>()
            .Property(p => p.CashBalance)
            .HasColumnType("decimal(18,4)");

        // ── One-to-one: AppUser ↔ Portfolio ─────────────────────────────
        // EF Core needs explicit config for one-to-one — won't infer it
        modelBuilder.Entity<Portfolio>()
            .HasOne(p => p.AppUser)
            .WithOne(u => u.Portfolio)
            .HasForeignKey<Portfolio>(p => p.AppUserId);
    }
}