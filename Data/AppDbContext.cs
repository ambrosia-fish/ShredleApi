using Microsoft.EntityFrameworkCore;
using ShredleApi.Models;

namespace ShredleApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Solo> Solos { get; set; } = null!;
    public DbSet<DailyGame> DailyGames { get; set; } = null!;
}