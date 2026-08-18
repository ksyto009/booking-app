namespace CourtBooking.Domain.Catalog;

using CourtBooking.Domain.Common;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Tham số chính sách — cấu hình theo tenant, KHÔNG hardcode (CR-07, CR-08)
    public decimal HalfHourPriceRatio { get; private set; }   // BR-14b
    public int RescheduleWindowHours { get; private set; } // BR-36
    public int MaxRescheduleCount { get; private set; }   // BR-38
    public int HoldMinutes { get; private set; }   // BR-11

    private Tenant() { }   // chỉ EF Core

    public static Tenant Create(string name, string slug, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên chủ sở hữu không được rỗng");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug không được rỗng");

        return new Tenant
        {
            Id = Guid.CreateVersion7(),        // UUID v7 — sắp theo thời gian
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Status = TenantStatus.Active,
            CreatedAt = now,
            HalfHourPriceRatio = 0.5m,        // mặc định, đổi được sau
            RescheduleWindowHours = 2,
            MaxRescheduleCount = 2,
            HoldMinutes = 10
        };
    }
}
