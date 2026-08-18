namespace CourtBooking.Infrastructure.Persistence;

using CourtBooking.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

public sealed class CourtBookingDbContext(DbContextOptions<CourtBookingDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Court>  Courts   => Set<Court>();

    //ApplyConfigurationsFromAssembly tự tìm mọi class IEntityTypeConfiguration<> trong assembly. Thêm entity mới chỉ cần tạo file config — không phải sửa DbContext.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CourtBookingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}