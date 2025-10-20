using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartGear.PM0902.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SmartGear.PM0902.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    
    public AppDbContext CreateDbContext(string[] args)
    {
        var b = new DbContextOptionsBuilder<AppDbContext>();
        b.UseSqlite("Data Source=dev.db");
        return new AppDbContext(b.Options);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();

            e.HasData(
                new Category { Id = 1, Name = "Apparel", Slug = "apparel" },
                new Category { Id = 2, Name = "Footwear", Slug = "footwear" },
                new Category { Id = 3, Name = "Accessories", Slug = "accessories" }
            );
        });

        b.Entity<Product>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();

            e.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);

            e.HasOne(x => x.Category)
             .WithMany()
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasData(
                new Product { Id = 1, Name = "Team Jersey", Slug = "team-jersey", BasePrice = 999, CategoryId = 1, IsActive = true },
                new Product { Id = 2, Name = "Training Shorts", Slug = "training-shorts", BasePrice = 399, CategoryId = 1, IsActive = true },
                new Product { Id = 3, Name = "Alpha Boots", Slug = "alpha-boots", BasePrice = 1299, CategoryId = 2, IsActive = true },
                new Product { Id = 4, Name = "Grip Gloves", Slug = "grip-gloves", BasePrice = 299, CategoryId = 3, IsActive = true }
            );
        });
    }

    public override int SaveChanges()
    {
        StampSlugs();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampSlugs();
        return base.SaveChangesAsync(ct);
    }

    private void StampSlugs()
    {
        string Slugify(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "item";
            var t = Regex.Replace(s.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
            t = Regex.Replace(t, "-{2,}", "-").Trim('-');
            return string.IsNullOrEmpty(t) ? "item" : t;
        }

        foreach (var e in ChangeTracker.Entries<Category>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            e.Entity.Slug = Slugify(e.Entity.Name);

        foreach (var e in ChangeTracker.Entries<Product>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            e.Entity.Slug = Slugify(e.Entity.Name);
    }
}