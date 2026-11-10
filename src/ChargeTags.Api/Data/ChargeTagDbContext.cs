using Microsoft.EntityFrameworkCore;

namespace ChargeTags.Api.Data;

public class ChargeTagDbContext : DbContext
{
    public ChargeTagDbContext(DbContextOptions<ChargeTagDbContext> options) : base(options)
    {
    }

    public DbSet<ChargeTag> ChargeTags => Set<ChargeTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChargeTag>(entity =>
        {
            entity.HasKey(tag => tag.Id);

            // "dk-" + 10 digits + "-clever" is always 20 characters
            entity.Property(tag => tag.Id).HasMaxLength(20);
        });
    }
}
