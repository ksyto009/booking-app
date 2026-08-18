namespace CourtBooking.Infrastructure.Persistence.Configurations;

using CourtBooking.Domain.Catalog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> b)
    {
        b.ToTable("court");

        // Primary key
        b.HasKey(x => x.Id);

        // Tenant relationship
        b.Property(x => x.TenantId)
            .IsRequired();

        // FK -> Tenant
        b.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Branch relationship
        b.Property(x => x.BranchId)
            .IsRequired();

        // FK -> Branch
        b.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Code
        b.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        // Name
        b.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Court type
        b.Property(x => x.CourtType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Status
        b.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Soft delete
        b.Property(x => x.DeletedAt);
        b.HasQueryFilter(x => x.DeletedAt == null);

        // Audit
        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.CreatedBy);

        b.Property(x => x.UpdatedAt);

        b.Property(x => x.UpdatedBy);

        // UNIQUE (branch_id, code)
        // WHERE deleted_at IS NULL
        b.HasIndex(x => new
        {
            x.BranchId,
            x.Code
        })
            .IsUnique()
            .HasDatabaseName("uq_court_code")
            .HasFilter("deleted_at IS NULL");
    }
}