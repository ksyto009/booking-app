namespace CourtBooking.Domain.Catalog;

using CourtBooking.Domain.Common;

public sealed class Branch
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string? Phone { get; private set; }

    public OpeningHours OpeningHours { get; private set; } = null!;

    public string TimeZone { get; private set; } = null!;
    public BranchStatus Status { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private Branch() { } // EF Core

    public static Branch Create(
        Guid tenantId,
        string name,
        string address,
        string? phone,
        OpeningHours openingHours,
        string timeZone,
        Guid? createdBy,
        DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId không hợp lệ");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên chi nhánh không được rỗng");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Địa chỉ không được rỗng");

        // if (openingHours == null)
        //     throw new ArgumentNullException(nameof(openingHours));
        ArgumentNullException.ThrowIfNull(openingHours);

        if (string.IsNullOrWhiteSpace(timeZone))
            throw new DomainException("Time zone không được rỗng");

        return new Branch
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name.Trim(),
            Address = address.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            OpeningHours = openingHours,
            TimeZone = timeZone.Trim(),
            Status = BranchStatus.Active,
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name,
        string address,
        string? phone,
        OpeningHours openingHours,
        string timeZone,
        Guid? updatedBy,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên chi nhánh không được rỗng");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Địa chỉ không được rỗng");

        ArgumentNullException.ThrowIfNull(openingHours);

        if (string.IsNullOrWhiteSpace(timeZone))
            throw new DomainException("Time zone không được rỗng");

        Name = name.Trim();
        Address = address.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

        OpeningHours = openingHours;
        TimeZone = timeZone.Trim();

        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    public void Deactivate(Guid? updatedBy, DateTimeOffset now)
    {
        Status = BranchStatus.Inactive;
        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    public void SoftDelete(Guid? updatedBy, DateTimeOffset now)
    {
        if (DeletedAt.HasValue)
            return;

        DeletedAt = now;
        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

}