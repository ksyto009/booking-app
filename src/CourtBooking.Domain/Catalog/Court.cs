namespace CourtBooking.Domain.Catalog;

using CourtBooking.Domain.Common;

public sealed class Court
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public CourtType CourtType { get; private set; }
    public CourtStatus Status { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private Court() { } // EF Core

    public static Court Create(
        Guid tenantId,
        Guid branchId,
        string code,
        string name,
        CourtType courtType,
        Guid? createdBy,
        DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId không hợp lệ");

        if (branchId == Guid.Empty)
            throw new DomainException("BranchId không hợp lệ");

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Mã sân không được rỗng");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên sân không được rỗng");

        return new Court
        {
            Id = Guid.CreateVersion7(),

            TenantId = tenantId,
            BranchId = branchId,

            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),

            CourtType = courtType,
            Status = CourtStatus.Active,

            CreatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string code,
        string name,
        CourtType courtType,
        Guid? updatedBy,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Mã sân không được rỗng");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên sân không được rỗng");

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        CourtType = courtType;

        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    public void CloseForMaintenance(Guid? updatedBy, DateTimeOffset now)
    {
        Status = CourtStatus.Maintenance;

        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    public void Activate(Guid? updatedBy, DateTimeOffset now)
    {
        Status = CourtStatus.Active;

        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    public void Deactivate(Guid? updatedBy, DateTimeOffset now)
    {
        Status = CourtStatus.Inactive;

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