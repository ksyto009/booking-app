# Hướng dẫn S0-03 — EF Core DbContext + Migration đầu tiên

> Phạm vi: ba entity `Tenant`, `Branch`, `Court` + value object `OpeningHours`.
> **Khác hai task trước:** đây là code thật. Tôi làm mẫu **một** entity đầy đủ, **bạn viết hai cái còn lại**.

---

## 0. Năm khái niệm nền

### 0.1 ORM và EF Core

**ORM** (Object-Relational Mapper) dịch qua lại giữa **object trong C#** và **dòng trong bảng SQL**. EF Core là ORM chính thức của .NET.

Bạn viết `_db.Courts.Where(c => c.BranchId == id)`, EF Core sinh ra `SELECT ... FROM court WHERE branch_id = @p0`.

### 0.2 `DbContext`

Đại diện cho **một phiên làm việc với CSDL**. Nó giữ:
- Danh sách bảng (`DbSet<T>`)
- **Change Tracker** — theo dõi object nào đã bị sửa
- `SaveChanges()` — gom mọi thay đổi thành **một transaction**

> 🔑 `SaveChangesAsync()` là **một transaction**. Đây chính là cơ chế mà [ADR-0003](../16-decision-records/0003-atomic-reschedule.md) dựa vào để dời lịch nguyên tử.

### 0.3 Migration

Mã C# mô tả **thay đổi cấu trúc CSDL**, được đánh phiên bản và commit vào git.

```
Migration 001: tạo bảng tenant, branch, court
Migration 002: thêm cột court.court_type
```

Vì sao không sửa CSDL bằng tay: **CSDL của bạn và của đồng nghiệp phải giống hệt nhau**, và CI phải dựng lại được từ số 0. Sửa tay không tái lập được.

### 0.4 POCO và vì sao `Domain` không được biết EF Core

Entity trong `Domain` là **POCO** — Plain Old CLR Object. Không kế thừa class của EF, không gắn attribute của EF.

Toàn bộ cấu hình ánh xạ nằm ở `Infrastructure`, qua `IEntityTypeConfiguration<T>`. Nhờ vậy:

```
Domain      : "Court có mã và tên, mã không được rỗng"       ← nghiệp vụ
Infrastructure: "Court lưu ở bảng court, cột code varchar(20)" ← kỹ thuật
```

Đổi từ PostgreSQL sang thứ khác → `Domain` không đổi một dòng.

### 0.5 Constructor `private` — mẹo quan trọng

EF Core cần constructor không tham số để dựng object khi đọc từ CSDL. Nhưng ta **không muốn** ai đó `new Tenant()` rồi tạo ra object thiếu dữ liệu.

Giải pháp: constructor **`private`**. EF Core dùng reflection nên vẫn gọi được; code bình thường thì không.

```csharp
private Tenant() { }                       // chỉ EF Core dùng được
public static Tenant Create(...) { ... }   // đường vào duy nhất cho code
```

> Đây là cách giữ bất biến của domain mà vẫn dùng được ORM.

---

## 1. ⚖️ Quyết định của bạn — package nào vào project nào?

Ba package cần cài:

| Package | Làm gì |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Provider EF Core cho PostgreSQL |
| `EFCore.NamingConventions` | Tự đổi `PascalCase` → `snake_case` khi đặt tên bảng/cột |
| `Microsoft.EntityFrameworkCore.Design` | Công cụ sinh migration *(chỉ dùng lúc thiết kế)* |

**Câu hỏi:** mỗi package vào project nào — `Domain`, `Application`, `Infrastructure`, hay `Api`?

*Gợi ý: nhớ quy tắc vàng ở [S0-01 §0.4](S0-01-solution-skeleton.md). Và với package thứ ba, hỏi thêm: ai là project được `dotnet ef` khởi động?*

**Trả lời trước khi đọc tiếp.** Đáp án ở §7.

### Vì sao cần `EFCore.NamingConventions`?

Mặc định EF Core đặt tên bảng theo tên class: `Court`, `OpeningHours`. Nhưng PostgreSQL quy ước dùng `snake_case`, và [10-database-design.md](../10-database-design.md) của ta viết `court`, `booking_slot`.

Không có package này, mọi câu SQL viết tay đều phải bọc nháy kép: `SELECT * FROM "Court"`. Rất phiền và dễ sai.

Cài 
```
cd D:\Ksyto\Booking-app && dotnet add src/CourtBooking.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
cd D:\Ksyto\Booking-app && dotnet add src/CourtBooking.Infrastructure package EFCore.NamingConventions --version 9.0.0
cd D:\Ksyto\Booking-app && dotnet add src/CourtBooking.Api package Microsoft.EntityFrameworkCore.Design --version 9.0.19
```
---

## 2. Tầng Domain — bạn viết

Tạo cấu trúc thư mục theo module *(ràng buộc #3 của [ADR-0004](../16-decision-records/0004-solution-structure-layers-outside.md))*:

```
src/CourtBooking.Domain/
  Common/
    DomainException.cs
  Catalog/
    Tenant.cs          ← tôi làm mẫu
    Branch.cs          ← bạn viết
    Court.cs           ← bạn viết
    OpeningHours.cs    ← tôi làm mẫu
    Enums.cs
```

### 2.1 `DomainException` — làm mẫu

```csharp
namespace CourtBooking.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

Vì sao cần class riêng thay vì `InvalidOperationException`: sau này middleware bắt lỗi sẽ **phân biệt** được "vi phạm quy tắc nghiệp vụ" *(trả 422)* với "lỗi hệ thống" *(trả 500)*.

### 2.2 `OpeningHours` — Value Object, làm mẫu

```csharp
namespace CourtBooking.Domain.Catalog;

using CourtBooking.Domain.Common;

public sealed record OpeningHours
{
    public TimeOnly Open { get; init; }
    public TimeOnly Close { get; init; }

    private OpeningHours(TimeOnly open, TimeOnly close)
    {
        Open = open;
        Close = close;
    }

    public static OpeningHours Create(TimeOnly open, TimeOnly close)
    {
        if (close <= open)
            throw new DomainException("Giờ đóng cửa phải sau giờ mở cửa");

        return new OpeningHours(open, close);
    }

    public static OpeningHours Default => Create(new TimeOnly(5, 0), new TimeOnly(23, 0));
}
```

**Ba điểm đáng để ý:**

| | Vì sao |
|---|---|
| `record` chứ không `class` | `record` tự sinh so sánh **theo giá trị** — đúng bản chất Value Object |
| Constructor `private` + factory `Create` | Không thể tạo `OpeningHours` sai. *"Make illegal states unrepresentable"* |
| `TimeOnly` chứ không `DateTime` | Đây là **giờ trong ngày**, không phải một thời điểm. Dùng đúng kiểu là tự tài liệu hoá |

### 2.3 `Enums.cs` — bạn viết

Cần 4 enum: `TenantStatus` *(Active, Suspended)* · `BranchStatus` *(Active, Inactive)* · `CourtStatus` *(Active, Maintenance, Inactive)* · `CourtType` *(Indoor, Outdoor)*.

### 2.4 `Tenant` — làm mẫu

```csharp
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
    public decimal HalfHourPriceRatio  { get; private set; }   // BR-14b
    public int     RescheduleWindowHours { get; private set; } // BR-36
    public int     MaxRescheduleCount  { get; private set; }   // BR-38
    public int     HoldMinutes         { get; private set; }   // BR-11

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
            HalfHourPriceRatio  = 0.5m,        // mặc định, đổi được sau
            RescheduleWindowHours = 2,
            MaxRescheduleCount  = 2,
            HoldMinutes         = 10
        };
    }
}
```

**Bốn điểm đáng để ý:**

| | Vì sao |
|---|---|
| `{ get; private set; }` | Bên ngoài **đọc được, không sửa được**. Sửa phải qua method |
| `Guid.CreateVersion7()` | UUID v7 — sắp theo thời gian, index không phân mảnh *(design-decisions §10)* |
| `DateTimeOffset now` truyền vào | **Không** gọi `DateTime.UtcNow` bên trong. Nếu gọi thì không test được |
| `0.5m` — hậu tố `m` | `decimal`. Không bao giờ dùng `float`/`double` cho tiền |

### 2.5 `Branch` và `Court` — 🔴 bạn viết

Đối chiếu [10-database-design.md §2.2](../10-database-design.md) để lấy đúng danh sách cột.

**`Branch`** cần: `Id`, `TenantId`, `Name`, `Address`, `Phone?`, `OpeningHours`, `TimeZone`, `Status`, `DeletedAt?`, `CreatedAt`, `CreatedBy?`, `UpdatedAt?`, `UpdatedBy?`

**`Court`** cần: `Id`, `TenantId`, `BranchId`, `Code`, `Name`, `CourtType`, `Status`, `DeletedAt?`, + 4 cột audit

Cả hai theo đúng khuôn của `Tenant`: property `private set`, constructor `private`, factory `Create` có kiểm tra, `Guid.CreateVersion7()`, `now` truyền từ ngoài.

> 💡 **Câu hỏi tự kiểm:** `Branch` có nên giữ `List<Court>` không?
> Nhớ lại [07-domain-model.md §3.3](../07-domain-model.md) — bạn đã quyết `Court` **tách khỏi** `Branch`. Vậy `Branch` chỉ biết `Court` qua… gì?

---

## 3. Tầng Infrastructure — cấu hình ánh xạ

```
src/CourtBooking.Infrastructure/
  Persistence/
    CourtBookingDbContext.cs
    Configurations/
      TenantConfiguration.cs     ← tôi làm mẫu
      BranchConfiguration.cs     ← bạn viết
      CourtConfiguration.cs      ← bạn viết
```

### 3.1 `TenantConfiguration` — làm mẫu

```csharp
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
```

**Ba quyết định:**

| | Vì sao |
|---|---|
| `HasConversion<string>()` cho enum | Lưu `"Active"` thay vì `0`. Đọc CSDL bằng mắt hiểu ngay; và **chèn giá trị mới vào giữa enum không làm hỏng dữ liệu cũ** |
| `HasMaxLength` | Không có thì thành `text` không giới hạn — mất tầng validate ở CSDL |
| `HasCheckConstraint` | Bất biến do **CSDL** gánh, không phó thác cho tầng ứng dụng. Đúng nguyên tắc #1 của dự án |

### 3.2 `BranchConfiguration` và `CourtConfiguration` — bạn viết

Yêu cầu bắt buộc:

| Yêu cầu | Gợi ý |
|---|---|
| `Branch.OpeningHours` map thành **2 cột** `open_time`, `close_time` | `b.ComplexProperty(x => x.OpeningHours, ...)` |
| `Court.Code` **duy nhất trong một branch**, chỉ tính bản ghi chưa xoá mềm | `HasIndex(...).IsUnique().HasFilter("deleted_at IS NULL")` ← **partial index**, đúng cái bạn học ở ADR-0001 |
| Mọi enum lưu dạng chuỗi | |
| Khoá ngoại `Court → Branch` | `HasOne<Branch>().WithMany().HasForeignKey(x => x.BranchId)` |
| Soft delete: query mặc định ẩn bản ghi đã xoá | `b.HasQueryFilter(x => x.DeletedAt == null)` |

### 3.3 `DbContext`

```csharp
namespace CourtBooking.Infrastructure.Persistence;

using CourtBooking.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

public sealed class CourtBookingDbContext(DbContextOptions<CourtBookingDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Court>  Courts   => Set<Court>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CourtBookingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

`ApplyConfigurationsFromAssembly` tự tìm **mọi** class `IEntityTypeConfiguration<>` trong assembly. Thêm entity mới chỉ cần tạo file config — không phải sửa `DbContext`.

---

## 4. Chuỗi kết nối — **không** hardcode

Dùng **User Secrets**: lưu ngoài repo hoàn toàn, không có khả năng lỡ commit.

```powershell
dotnet user-secrets init --project src/CourtBooking.Api
```

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=courtbooking;Username=courtbooking;Password=MAT_KHAU_TRONG_FILE_ENV" --project src/CourtBooking.Api
```

> Thay `MAT_KHAU_TRONG_FILE_ENV` bằng `POSTGRES_PASSWORD` trong `.env`.
> User Secrets nằm ở `%APPDATA%\Microsoft\UserSecrets\` — **ngoài thư mục dự án**, nên không bao giờ vào git.

### Đăng ký DbContext trong `Program.cs`

Thêm **trước** `var app = builder.Build();`:

```csharp
builder.Services.AddDbContext<CourtBookingDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());
```

`UseSnakeCaseNamingConvention()` là thứ biến `HalfHourPriceRatio` thành cột `half_hour_price_ratio`.

---

## 5. Tạo migration

Cài công cụ *(một lần cho cả máy)*:

```powershell
dotnet tool install --global dotnet-ef
```

```powershell
dotnet ef migrations add InitialCatalog --project src/CourtBooking.Infrastructure --startup-project src/CourtBooking.Api
```

| Tham số | Nghĩa |
|---|---|
| `--project` | Nơi **chứa** migration và `DbContext` |
| `--startup-project` | Project có **cấu hình và DI** để `dotnet ef` dựng được `DbContext` |

> 🔑 Hai tham số này khác nhau chính là hệ quả của Clean Architecture: `DbContext` sống ở `Infrastructure`, nhưng chuỗi kết nối nằm ở `Api`.

**Kiểm tra:** thư mục `src/CourtBooking.Infrastructure/Migrations/` xuất hiện. **Mở file `*_InitialCatalog.cs` ra đọc** — bạn phải hiểu nó tạo bảng gì, cột gì, index gì. Đừng bao giờ áp một migration mình chưa đọc.

---

## 6. Áp migration và nghiệm thu

```powershell
dotnet ef database update --project src/CourtBooking.Infrastructure --startup-project src/CourtBooking.Api
```

Kiểm tra bảng đã được tạo:

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "\dt"
```

Phải thấy `tenant`, `branch`, `court`, và `__EFMigrationsHistory` *(bảng EF dùng để nhớ đã áp migration nào)*.

Kiểm tra tên cột đúng `snake_case`:

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "\d tenant"
```

Kiểm tra partial unique index của `court`:

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "\d court"
```

Phải thấy dòng index kèm `WHERE (deleted_at IS NULL)`.

---

## 7. ✅ Đáp án câu hỏi ở §1

| Package | Project | Vì sao |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | **Infrastructure** | Chi tiết công nghệ. `Domain` và `Application` không được biết ta dùng PostgreSQL |
| `EFCore.NamingConventions` | **Infrastructure** | Quy ước ánh xạ — cùng lý do |
| `Microsoft.EntityFrameworkCore.Design` | **Api** | Chỉ dùng lúc thiết kế, bởi `dotnet ef`, và `Api` là startup project |

**`Domain` vẫn phải có 0 package.** Kiểm tra:

```powershell
Get-Content src/CourtBooking.Domain/CourtBooking.Domain.csproj
```

Vẫn phải rỗng trơn. Nếu có bất kỳ `PackageReference` nào → bạn đã đặt sai chỗ.

> Nhớ dùng **Central Package Management**: thêm `<PackageVersion>` vào `Directory.Packages.props`, còn `.csproj` chỉ ghi `<PackageReference Include="..." />` không kèm `Version`.

---

## 🕳️ Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| `Unable to create a DbContext` | Thiếu `--startup-project`, hoặc chưa `AddDbContext` trong `Program.cs` | Kiểm tra cả hai |
| `No connection string named 'Default'` | Chưa set user-secrets, hoặc sai key | `dotnet user-secrets list --project src/CourtBooking.Api` |
| `28P01: password authentication failed` | Mật khẩu trong user-secrets khác `.env` | Đối chiếu lại |
| `Npgsql ... Connection refused` | Container chưa chạy | `docker compose up -d` |
| Tên bảng ra `Tenants` chứ không `tenant` | Quên `UseSnakeCaseNamingConvention()`, hoặc quên `ToTable("tenant")` | Thêm vào |
| `No suitable constructor was found for entity type` | Quên constructor `private` không tham số | Thêm `private Tenant() { }` |
| Build đỏ: `CS8618 non-nullable property must contain a non-null value` | Đã bật `Nullable` mà property `string` chưa khởi tạo | Thêm `= null!;` như trong mẫu |
| `dotnet ef` không nhận lệnh | Chưa cài tool, hoặc PATH chưa cập nhật | Mở lại PowerShell sau khi `dotnet tool install` |

---

## ✅ Definition of Done

- [ ] `dotnet build` → **0 warning, 0 error**
- [ ] `Domain.csproj` vẫn **không có `PackageReference` nào**
- [ ] 3 entity + 1 value object + 4 enum, mọi property `private set`
- [ ] Mọi entity có constructor `private` và factory `Create` có kiểm tra
- [ ] Không có `DateTime.Now` / `DateTime.UtcNow` trong `Domain` — `now` luôn truyền từ ngoài
- [ ] 3 file `IEntityTypeConfiguration`, **không** dùng attribute EF trong `Domain`
- [ ] Chuỗi kết nối ở **user-secrets**, không có trong `appsettings.json`
- [ ] Migration tạo được và **bạn đã đọc hiểu** nội dung file migration
- [ ] `\dt` thấy `tenant`, `branch`, `court`, `__EFMigrationsHistory`
- [ ] Tên bảng và cột đều **`snake_case`**
- [ ] `\d court` thấy partial unique index kèm `WHERE (deleted_at IS NULL)`

---

## 📚 Bảy thứ vừa học — câu hỏi phỏng vấn

1. **Vì sao entity `Domain` không được gắn attribute của EF Core?**
   → Để `Domain` không phụ thuộc công nghệ lưu trữ. Cấu hình ánh xạ thuộc về `Infrastructure`.

2. **Constructor `private` để làm gì khi EF Core vẫn gọi được?**
   → EF dùng reflection nên vượt qua được; code thường thì không. Giữ được bất biến mà vẫn dùng ORM.

3. **Vì sao lưu enum dạng chuỗi thay vì số?**
   → Đọc CSDL hiểu ngay, và chèn giá trị mới vào giữa enum không làm hỏng dữ liệu cũ.

4. **`--project` và `--startup-project` khác nhau gì?**
   → Nơi chứa `DbContext` vs nơi có cấu hình/DI. Chúng khác nhau **vì** Clean Architecture tách hai thứ đó.

5. **Vì sao không sửa CSDL bằng tay mà phải qua migration?**
   → Để mọi môi trường tái lập được từ số 0, và thay đổi schema được version control như code.

6. **Partial index là gì, dùng khi nào?**
   → Index chỉ áp cho tập con thoả điều kiện. Ở đây: mã sân duy nhất **chỉ tính bản ghi chưa xoá mềm** — không có `HasFilter` thì xoá mềm rồi tạo lại cùng mã sẽ bị chặn.

7. **`SaveChangesAsync()` có phải một transaction không?**
   → Có. Mọi thay đổi được gom thành một transaction — đây là nền tảng của dời lịch nguyên tử (ADR-0003).

---

## ➡️ Task tiếp theo

**S0-04 — Serilog + CorrelationId middleware.**
