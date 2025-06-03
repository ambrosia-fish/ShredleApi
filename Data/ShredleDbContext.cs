using Microsoft.EntityFrameworkCore;
using ShredleApi.Models;

namespace ShredleApi.Data
{
    public class ShredleDbContext : DbContext
    {
        public ShredleDbContext(DbContextOptions<ShredleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games => Set<Game>();
        public DbSet<Solo> Solos => Set<Solo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Game entity
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("games");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.SoloId).HasColumnName("solo_id");
                
                // Unique constraint on Date
                entity.HasIndex(e => e.Date).IsUnique();
                
                // Relationship
                entity.HasOne(e => e.Solo)
                    .WithMany(s => s.Games)
                    .HasForeignKey(e => e.SoloId);
            });

            // Configure Solo entity
            modelBuilder.Entity<Solo>(entity =>
            {
                entity.ToTable("solos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Artist).HasColumnName("artist");
                entity.Property(e => e.SpotifyId).HasColumnName("spotify_id");
                entity.Property(e => e.StartTimeClip1).HasColumnName("start_time_clip1");
                entity.Property(e => e.EndTimeClip1).HasColumnName("end_time_clip1");
                entity.Property(e => e.StartTimeClip2).HasColumnName("start_time_clip2");
                entity.Property(e => e.EndTimeClip2).HasColumnName("end_time_clip2");
                entity.Property(e => e.StartTimeClip3).HasColumnName("start_time_clip3");
                entity.Property(e => e.EndTimeClip3).HasColumnName("end_time_clip3");
                entity.Property(e => e.StartTimeClip4).HasColumnName("start_time_clip4");
                entity.Property(e => e.EndTimeClip4).HasColumnName("end_time_clip4");
                entity.Property(e => e.Guitarist).HasColumnName("guitarist");
                entity.Property(e => e.Hint).HasColumnName("hint");
            });
        }
    }
}
