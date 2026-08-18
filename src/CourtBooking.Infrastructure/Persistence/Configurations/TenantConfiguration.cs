namespace CourtBooking.Infrastructure.Persistence.Configurations;

using CourtBooking.Domain.Catalog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenant");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();

        b.Property(x => x.Status)
            .HasConversion<string>()          // lưu "Active" thay vì 0
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.HalfHourPriceRatio).HasPrecision(4, 3);

        b.ToTable(t => t.HasCheckConstraint(
            "ck_tenant_ratio",
            "half_hour_price_ratio > 0 AND half_hour_price_ratio <= 1"));
    }
}
