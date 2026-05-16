using Microsoft.EntityFrameworkCore;
using RoyalVillaApi.Models;

namespace RoyalVillaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// DbSet<T> is an Entity Framework Core type that represents the Villas table
    /// We say:
    /// “There is a collection of Villa rows in the database.”
    /// “Use this property to query, add, update, and remove them.”
    /// “Conceptually it’s a set of many villas“
    /// </summary>
    public DbSet<Villa> Villas => Set<Villa>();
}