using Microsoft.EntityFrameworkCore;

namespace RoyalVillaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // public DbSet<YourEntity> YourEntities => Set<YourEntity>();
}