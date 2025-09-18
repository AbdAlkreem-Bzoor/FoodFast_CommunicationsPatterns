using FoodFast.API.Domain.Entities;
using FoodFast.API.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodFast.API.Data;

public class FoodFastDbContext : IdentityDbContext<ApplicationUser>
{
    public FoodFastDbContext(DbContextOptions<FoodFastDbContext> options) : base(options) { }

    public DbSet<UploadJob> UploadJobs { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Announcement> Announcements { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Order>().HasIndex(o => o.RestaurantId);
        builder.Entity<UploadJob>().HasIndex(j => j.OwnerUserId);
    }
}
