# Hướng dẫn S0-01 — Dựng bộ khung Solution

> Dành cho người **lần đầu làm .NET**. Mỗi lệnh đều có giải thích nó làm gì và vì sao cần.
> Chạy trong **PowerShell**, tại thư mục `D:\Ksyto\Booking-app`.

---

## 0. Bốn khái niệm phải hiểu trước khi gõ lệnh

### 0.1 Project (`.csproj`)

**Là gì:** đơn vị biên dịch nhỏ nhất của .NET. Một project → biên dịch ra **một file `.dll`**.

**Vì sao cần nhiều project:** để **ép ranh giới**. Nếu tất cả code nằm chung một project, không gì ngăn tầng `Domain` gọi thẳng vào EF Core. Tách project rồi thì **trình biên dịch** trở thành người gác cổng — `Domain` không tham chiếu EF Core thì code trong đó **không thể** dùng EF Core, dù lập trình viên có muốn.

> Đây là lý do sâu xa của Clean Architecture: biến quy tắc kiến trúc thành **lỗi biên dịch**, thay vì thành lời nhắc trong tài liệu.

### 0.2 Solution (`.sln`)

**Là gì:** một file text liệt kê các project đi cùng nhau. Nó **không** chứa code.

**Để làm gì:** để `dotnet build` một phát build hết, và để IDE mở cả bộ.

### 0.3 ProjectReference

`A` tham chiếu `B` ⇒ code trong `A` **dùng được** class của `B`. Một chiều.

Đây chính là công cụ để thực thi quy tắc phụ thuộc:

```
Api  →  Application  →  Domain
 ↓                          ↑
Infrastructure ────────────┘

Domain KHÔNG tham chiếu ai cả.
```

### 0.4 PackageReference

Thư viện ngoài tải từ NuGet *(EF Core, Serilog, MediatR…)*.

**Quy tắc vàng của dự án này:** `Domain` **không có** `PackageReference` nào ngoài thư viện chuẩn .NET.

---

## 1. Kiểm tra môi trường

```powershell
dotnet --version
```

Phải ra `9.x.x`. Nếu báo lỗi "không tìm thấy lệnh" → chưa cài .NET SDK.

```powershell
dotnet --list-sdks
```

Xem tất cả phiên bản SDK đang có trên máy.

---

## 2. Tạo solution

```powershell
cd D:\Ksyto\Booking-app
dotnet new sln -n CourtBooking
```

| Thành phần | Nghĩa |
|---|---|
| `dotnet new` | Lệnh sinh mã từ template |
| `sln` | Loại template: solution |
| `-n CourtBooking` | Tên file → `CourtBooking.sln` |

**Kiểm tra:** thư mục gốc xuất hiện `CourtBooking.sln`.

---

## 3. Tạo 4 project tầng

Chạy lần lượt **4 lệnh**:

```powershell
dotnet new classlib -o src/CourtBooking.Domain
```

```powershell
dotnet new classlib -o src/CourtBooking.Application
```

```powershell
dotnet new classlib -o src/CourtBooking.Infrastructure
```

```powershell
dotnet new webapi -o src/CourtBooking.Api --use-controllers
```

| Thành phần | Nghĩa |
|---|---|
| `classlib` | Class Library — thư viện, không chạy độc lập được. Đúng cho 3 tầng trong |
| `webapi` | ASP.NET Core Web API — **chạy được**, có `Program.cs`, đây là điểm khởi động |
| `-o <đường dẫn>` | Thư mục đích. `dotnet` tự tạo nếu chưa có |
| `--use-controllers` | Dùng kiểu Controller thay vì Minimal API. Chọn Controller vì dự án có nhiều endpoint và cần nhóm theo tài nguyên |

> 💡 **Vì sao chỉ `Api` là `webapi`?** Vì chỉ nó là **tiến trình chạy thật**. Ba tầng còn lại là thư viện được nó nạp vào. Trong Clean Architecture, tầng ngoài cùng là nơi khởi động — gọi là **composition root**.

---

## 4. Tạo 2 project test

```powershell
dotnet new xunit -o tests/CourtBooking.UnitTests
```

```powershell
dotnet new xunit -o tests/CourtBooking.IntegrationTests
```

`xunit` là framework test phổ biến nhất trong .NET. Template này tự thêm sẵn xUnit và test runner.

---

## 5. Đưa tất cả project vào solution

```powershell
dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj | Select-Object -ExpandProperty FullName)
```

Lệnh PowerShell này tìm **mọi** file `.csproj` trong cây thư mục rồi đưa hết vào solution.

**Kiểm tra:**

```powershell
dotnet sln list
```

Phải thấy đủ **6 project**.

---

## 6. 🔒 Nối tham chiếu — bước quan trọng nhất

Đây là bước **biến quy tắc kiến trúc thành ràng buộc của trình biên dịch**. Làm sai ở đây thì Clean Architecture chỉ còn là mấy cái thư mục.

```powershell
dotnet add src/CourtBooking.Application reference src/CourtBooking.Domain
```

```powershell
dotnet add src/CourtBooking.Infrastructure reference src/CourtBooking.Application
```

```powershell
dotnet add src/CourtBooking.Api reference src/CourtBooking.Application
```

```powershell
dotnet add src/CourtBooking.Api reference src/CourtBooking.Infrastructure
```

```powershell
dotnet add tests/CourtBooking.UnitTests reference src/CourtBooking.Domain
```

```powershell
dotnet add tests/CourtBooking.UnitTests reference src/CourtBooking.Application
```

```powershell
dotnet add tests/CourtBooking.IntegrationTests reference src/CourtBooking.Api
```

### ❗ Ba điều phải để ý

**1. Không có lệnh nào thêm reference VÀO `Domain`.** Cố ý. `Domain` là trung tâm, không phụ thuộc ai.

**2. `Infrastructure → Application`, không phải ngược lại.** Nghe phản trực giác: "hạ tầng chứa repository, tầng application dùng repository, sao application không tham chiếu infrastructure?"

Câu trả lời là **Dependency Inversion**:
- `Application` định nghĩa **interface** `IBookingRepository` *(nó nói: tôi cần ai đó lưu booking)*
- `Infrastructure` **hiện thực** interface đó bằng EF Core
- Lúc chạy, DI container ghép hai thứ lại

Nhờ vậy `Application` **không biết** EF Core tồn tại. Đổi từ PostgreSQL sang thứ khác → chỉ sửa `Infrastructure`.

**3. `Api → Infrastructure` là nhân nhượng có chủ đích.** Về lý thuyết `Api` chỉ nên biết `Application`. Nhưng ai đó phải đăng ký `IBookingRepository → BookingRepository` vào DI, và nơi đó là `Program.cs`. Đây là **composition root** — chỗ duy nhất được phép biết mọi thứ.

**Kiểm tra:**

```powershell
dotnet list src/CourtBooking.Domain reference
```

Phải in ra: **không có tham chiếu nào.** Nếu có bất cứ dòng nào → sai, xoá đi.

---

## 7. `Directory.Build.props` — cấu hình dùng chung

**Vấn đề nó giải quyết:** mỗi `.csproj` đang khai báo riêng `TargetFramework`. Có 6 project = 6 chỗ. Nâng lên .NET 10 phải sửa 6 file, và chắc chắn sẽ quên một.

MSBuild tự động nạp file `Directory.Build.props` ở thư mục gốc và áp cho **mọi** project bên dưới.

Tạo file `Directory.Build.props` ở **thư mục gốc** với nội dung:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

| Thuộc tính | Ý nghĩa | Vì sao bật |
|---|---|---|
| `TargetFramework` | Phiên bản .NET | **Một chỗ duy nhất** |
| `Nullable` | Bật kiểm tra null của trình biên dịch | Chặn `NullReferenceException` — lỗi phổ biến nhất trong C# — ngay từ lúc biên dịch |
| `ImplicitUsings` | Tự thêm `using System;` … | Đỡ rác đầu file |
| `TreatWarningsAsErrors` | **Warning = lỗi, build đỏ** | Warning hôm nay là bug tháng sau. Bật ngay từ đầu, đừng chờ tích luỹ 200 cái |
| `EnforceCodeStyleInBuild` | Áp `.editorconfig` lúc build | Quy ước không được thực thi thì là quy ước chết |

### 🧹 Dọn `.csproj` — đừng bỏ sót bước này

Sau khi tạo `Directory.Build.props`, **xoá khỏi cả 6 file `.csproj`** những dòng đã được khai báo tập trung:

```xml
<TargetFramework>net9.0</TargetFramework>     <!-- xoá -->
<ImplicitUsings>enable</ImplicitUsings>       <!-- xoá -->
<Nullable>enable</Nullable>                   <!-- xoá -->
```

**Vì sao phải xoá dù giá trị đang giống hệt nhau?**

MSBuild áp `Directory.Build.props` **trước**, rồi `.csproj` **ghi đè lên**. Nghĩa là ngày nào đó bạn tắt `Nullable` ở file trung tâm để thử nghiệm, **6 project vẫn bật** — và bạn sẽ ngồi tìm nửa tiếng không hiểu vì sao cấu hình không có tác dụng.

> **Cấu hình khai báo hai chỗ thì chỗ nào cũng không đáng tin.** Đó là toàn bộ lý do `Directory.Build.props` tồn tại — để nguyên bản sao ở `.csproj` là nó chỉ làm được một nửa việc.

`Domain.csproj` sau khi dọn sẽ chỉ còn:

```xml
<Project Sdk="Microsoft.NET.Sdk">
</Project>
```

Rỗng trơn — và đó là **dấu hiệu tốt**. Một project domain sạch thì `.csproj` của nó không có gì để nói.

---

## 8. `Directory.Packages.props` — quản lý phiên bản package tập trung

### Vấn đề nó giải quyết

Sau bước 4, phiên bản package đang nằm rải rác:

```
UnitTests        : xunit 2.9.2 · Microsoft.NET.Test.Sdk 17.12.0 · coverlet 6.0.2
IntegrationTests : xunit 2.9.2 · Microsoft.NET.Test.Sdk 17.12.0 · coverlet 6.0.2
Api              : Microsoft.AspNetCore.OpenApi 9.0.5
```

Hiện mới 2 project trùng nhau nên chưa thấy đau. Nhưng sắp tới bạn sẽ thêm EF Core, Npgsql, Serilog, MediatR, FluentValidation, Testcontainers… vào 4–5 project.

**Chuyện sẽ xảy ra:** `Infrastructure` dùng EF Core `9.0.1`, `IntegrationTests` dùng `9.0.3`. **Build vẫn xanh.** Chạy thì lỗi runtime kiểu `MethodNotFoundException` — loại lỗi tốn cả buổi mới tìm ra nguyên nhân, vì thông báo lỗi không hề nhắc tới phiên bản.

### Cách làm — Central Package Management

Tạo `Directory.Packages.props` ở **thư mục gốc**:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
  </ItemGroup>
</Project>
```

Sau đó trong từng `.csproj`, **bỏ hẳn thuộc tính `Version`**:

```xml
<!-- Trước -->
<PackageReference Include="xunit" Version="2.9.2" />

<!-- Sau -->
<PackageReference Include="xunit" />
```

| Khái niệm | Ai quyết định |
|---|---|
| `PackageReference` trong `.csproj` | Project này **cần package gì** |
| `PackageVersion` trong `Directory.Packages.props` | Cả solution dùng **phiên bản nào** |

Nâng phiên bản = sửa **một** chỗ. Không còn khả năng lệch version giữa các project.

> ⚠️ Nếu build báo lỗi **`NU1008`**, đọc kỹ thông báo — nó nói chính xác project nào còn sót thuộc tính `Version`.

**Kiểm tra:**

```powershell
dotnet build
```

---

## 9. `.editorconfig`

```powershell
dotnet new editorconfig
```

Sinh file quy ước định dạng chuẩn của .NET (thụt lề, đặt tên, thứ tự `using`…). IDE và trình biên dịch đều đọc nó.

---

## 10. Dọn file rác của template

Template `classlib` tạo sẵn `Class1.cs` vô nghĩa. Xoá:

```powershell
Remove-Item src/CourtBooking.Domain/Class1.cs, src/CourtBooking.Application/Class1.cs, src/CourtBooking.Infrastructure/Class1.cs -ErrorAction SilentlyContinue
```

---

## 11. Build và nghiệm thu

```powershell
dotnet build
```

### Kết quả cần đạt

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**0 warning** — không phải "vài cái không sao". Đã bật `TreatWarningsAsErrors` nên có warning là build đỏ luôn.

### Kiểm tra cuối

```powershell
dotnet sln list
```

```powershell
dotnet list src/CourtBooking.Domain reference
```

```powershell
Get-Content src/CourtBooking.Domain/CourtBooking.Domain.csproj
```

File `Domain.csproj` phải **rỗng trơn** — không `TargetFramework`, không `Nullable`, không `ImplicitUsings` *(tất cả đã lên `Directory.Build.props`)*, không `PackageReference`, không `ProjectReference`.

Kiểm tra cuối cùng — quét toàn bộ `.csproj` xem còn sót thuộc tính nào bị lặp không:

```powershell
Get-ChildItem -Recurse -Filter *.csproj -File | ForEach-Object { "=== $($_.Name) ==="; Select-String -Path $_.FullName -Pattern "TargetFramework|ImplicitUsings|Nullable|Version=" }
```

Kết quả mong đợi: **không dòng nào in ra** ngoài tên file. Mọi thuộc tính chung đã ở `Directory.Build.props`, mọi phiên bản package đã ở `Directory.Packages.props`.

---

## 🕳️ Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| `The specified solution file already exists` | Đã chạy `dotnet new sln` hai lần | Xoá `.sln` cũ rồi chạy lại |
| `Project already has a reference` | Chạy lệnh `add reference` trùng | Bỏ qua, vô hại |
| Build đỏ vì warning `CS1591` | Bật `GenerateDocumentationFile` | Đặt `false` như hướng dẫn |
| `Domain` vẫn tham chiếu gì đó | Gõ nhầm thứ tự trong lệnh `add reference` | `dotnet remove <project> reference <target>` |
| Lệnh `dotnet sln add (...)` báo lỗi | Đang chạy trong Git Bash chứ không phải PowerShell | Mở PowerShell |
| **`NU1008`** — *"Projects that use central package version management should not define the version"* | Đã bật `ManagePackageVersionsCentrally` nhưng `.csproj` còn sót `Version="..."` | Thông báo lỗi nói rõ project nào — xoá thuộc tính `Version` ở đó |
| **`NU1010`** — *"PackageReference ... not have a corresponding PackageVersion"* | Thêm package mới vào `.csproj` mà quên khai báo ở `Directory.Packages.props` | Thêm dòng `<PackageVersion>` tương ứng |
| Cấu hình trong `Directory.Build.props` "không có tác dụng" | `.csproj` còn khai báo lại thuộc tính đó và **ghi đè lên** | Xoá dòng trùng trong `.csproj` |

---

## ✅ Checklist trước khi nộp

- [x] `dotnet build` → `0 Warning(s), 0 Error(s)`
- [x] `dotnet sln list` → 6 project
- [x] `dotnet list src/CourtBooking.Domain reference` → rỗng
- [x] `Domain.csproj` không có `PackageReference`
- [x] `TargetFramework` chỉ xuất hiện trong `Directory.Build.props`
- [x] **`.csproj` không còn `ImplicitUsings` / `Nullable`** *(đã lên `Directory.Build.props`)*
- [x] **`Domain.csproj` rỗng trơn** — chỉ còn thẻ `<Project>`
- [x] **Có `Directory.Packages.props`, mọi `PackageReference` đã bỏ `Version`**
- [x] Có `.editorconfig`
- [x] Đã xoá `Class1.cs`

---

## 📋 Kết quả review thực tế *(2026-07-31)*

Task này đã được review và **đạt** — nhưng lần đầu nộp có **2 finding**. Ghi lại vì đây là lỗi mà gần như ai làm lần đầu cũng mắc:

| # | Finding | Bản chất |
|---|---|---|
| 1 | 6 file `.csproj` vẫn giữ `ImplicitUsings` và `Nullable` dù `Directory.Build.props` đã khai báo | Template `dotnet new` sinh sẵn — dễ chỉ xoá `TargetFramework` rồi quên hai cái còn lại |
| 2 | Phiên bản package nằm rải rác ở từng project | Mặc định của .NET; phải chủ động bật Central Package Management |

**Cả hai cùng một gốc:**

> ### Một sự thật chỉ được khai báo ở MỘT chỗ.

Đây là **DRY** — nhưng để ý: DRY ở đây **không phải về code, mà về cấu hình**.

Junior thường chỉ áp DRY cho hàm và class, rồi để cấu hình lặp lại khắp nơi — và trả giá đúng vào lúc nâng phiên bản framework hoặc gỡ một lỗi version drift.

---

## 📚 Năm thứ vừa học — sẽ bị hỏi khi phỏng vấn

1. **Vì sao tách project thay vì tách thư mục?**
   → Để trình biên dịch ép ranh giới. Thư mục chỉ là gợi ý; project là ràng buộc.

2. **Vì sao `Infrastructure → Application` chứ không ngược lại?**
   → Dependency Inversion. `Application` khai báo interface, `Infrastructure` hiện thực. Nhờ vậy nghiệp vụ không phụ thuộc công nghệ.

3. **Vì sao bật `TreatWarningsAsErrors` ngay từ đầu?**
   → Bật sau khi đã có 200 warning thì không ai bật nữa. Nợ kỹ thuật rẻ nhất là nợ chưa kịp vay.

4. **`Directory.Build.props` và `Directory.Packages.props` khác nhau gì?**
   → `Build.props` tập trung **thuộc tính biên dịch** (framework, nullable, cảnh báo). `Packages.props` tập trung **phiên bản package**. Cả hai cùng một mục đích: **một sự thật một chỗ**.

5. **Version drift giữa các project gây hậu quả gì?**
   → Build vẫn xanh nhưng chạy lỗi runtime (`MethodNotFoundException`, `TypeLoadException`) — loại lỗi khó tìm nhất vì thông báo không hề nhắc tới phiên bản. Central Package Management loại bỏ hoàn toàn khả năng này.

---

## ➡️ Task tiếp theo

**S0-02 — Docker Compose:** PostgreSQL 16 + Redis 7 + pgAdmin, chạy được bằng một lệnh.
