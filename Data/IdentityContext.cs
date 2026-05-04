using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moviest.Models;

namespace Moviest.Data
{
    public class IdentityContext : IdentityDbContext<IdentityUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

        public DbSet<WatchlistItem> WatchlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WatchlistItem>(entity =>
            {
                entity.HasIndex(w => new { w.UserId, w.MovieId }).IsUnique();
                entity.Property(w => w.MovieTitle).HasMaxLength(400);
                entity.Property(w => w.MoviePoster).HasMaxLength(200);
                entity.Property(w => w.MovieYear).HasMaxLength(10);

                entity.HasOne<IdentityUser>()
                      .WithMany()
                      .HasForeignKey(w => w.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
