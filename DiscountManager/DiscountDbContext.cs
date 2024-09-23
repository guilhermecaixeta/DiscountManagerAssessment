using DiscountManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager
{
    public class DiscountDbContext : DbContext
    {
        public const string CONNECTION_STRING_NAME = "Postgres";

        public DbSet<DiscountCode> DiscountCodes { get; set; }

        public DiscountDbContext(DbContextOptions<DiscountDbContext> options) : base(options)
        { }

        public DiscountDbContext()
        { }

        #region Required

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscountCode>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<DiscountCode>()
                .Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(8);

            modelBuilder.Entity<DiscountCode>()
                .HasIndex(e => new { e.Code })
                .IsUnique();
        }
        #endregion
    }
}