namespace CourtBooking.Infrastructure.Persistence.Configurations;

using CourtBooking.Domain.Catalog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> b)
    {
        b.ToTable("branch");

        // Primary key
        b.HasKey(x => x.Id);

        // Tenant relationship
        b.Property(x => x.TenantId).IsRequired();

        // FK -> Tenant
        //b.HasOne(x => x.Tenant) navigation không có
        b.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Basic properties
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();

        b.Property(x => x.Address).HasMaxLength(300).IsRequired();

        b.Property(x => x.Phone)
            .HasMaxLength(20);

        // Opening hours - Value Object
        b.ComplexProperty(x => x.OpeningHours, opening =>
        {
            opening.Property(x => x.Open)
                .HasColumnName("open_time")
                .HasColumnType("time")
                .IsRequired();

            opening.Property(x => x.Close)
                .HasColumnName("close_time")
                .HasColumnType("time")
                .IsRequired();
        });

        // Time zone
        b.Property(x => x.TimeZone)
            .HasMaxLength(50)
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
                            //Trong luồng bình thường, default của CSDL không bao giờ chạy 
                    // — vì domain luôn gán trước. 
                    // Vậy nó là cấu hình chết đánh lừa người đọc. 
                    // Và nếu một ngày nào đó nó có chạy (seed bằng SQL thô), 
                    // bạn sẽ có created_at không khớp với thời gian mà domain nghĩ.
        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.CreatedBy);

        b.Property(x => x.UpdatedAt);

        b.Property(x => x.UpdatedBy);

        // Partial index:
        // CREATE INDEX ix_branch_tenant
        // ON branch(tenant_id)
        // WHERE deleted_at IS NULL;
        b.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_branch_tenant")
            .HasFilter("deleted_at IS NULL");
    }
}