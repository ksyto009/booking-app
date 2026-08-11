# ADR-0004 — Cấu trúc solution: tầng ở ngoài, module ở trong

| | |
|---|---|
| **Trạng thái** | ✅ Accepted |
| **Ngày** | 2026-07-31 |
| **Người quyết định** | Solution Architect |
| **Xuất phát từ** | Task S0-01 |
| **Liên quan** | `CLAUDE.md` §5 *(chia module theo nghiệp vụ)* · NFR-30, NFR-31 · [09-architecture.md](../09-architecture.md) |

---

## 1. Bối cảnh

Dự án khai báo **hai** nguyên tắc tổ chức mã, và thoạt nhìn chúng mâu thuẫn nhau:

| Nguyên tắc | Nguồn | Trục chia |
|---|---|---|
| **Clean Architecture** — `Domain` / `Application` / `Infrastructure` / `Api` | Yêu cầu kỹ thuật ban đầu | **Tầng** |
| **Modular Monolith** — chia theo nghiệp vụ, không theo tầng kỹ thuật | `CLAUDE.md` §5 | **Module nghiệp vụ** |

Thực tế chúng **không** mâu thuẫn — chúng là hai trục khác nhau của cùng một khối. Câu hỏi thật là: **trục nào nằm ở ngoài (thành project), trục nào nằm ở trong (thành thư mục)?**

### Ràng buộc tại thời điểm quyết định

| Ràng buộc | Giá trị |
|---|---|
| Nhân lực | **1 lập trình viên**, ~10–15h/tuần |
| Kinh nghiệm .NET | **Lần đầu** — chưa từng dựng solution |
| Số module dự kiến | 5–6 *(`Identity`, `Catalog`, `Booking`, `Payment`, `Reporting`, `Notification`)* |
| Nhu cầu deploy riêng từng module | **Không có** — xem [04-non-functional-requirements.md](../04-non-functional-requirements.md), tải ~110 đơn/ngày |

---

## 2. Vấn đề

Chọn cách sắp xếp project vật lý sao cho:

1. **Quy tắc phụ thuộc giữa tầng** được trình biên dịch ép cứng *(Domain không được biết EF Core)*
2. **Ranh giới module nghiệp vụ** không bị xói mòn theo thời gian
3. Chi phí quản lý không vượt quá năng lực của một người

---

## 3. Các phương án đã cân nhắc

### Phương án A — Tầng ngoài, module trong *(ĐÃ CHỌN)*

```
src/
  CourtBooking.Domain/            ← 1 project
      Identity/  Catalog/  Booking/  Payment/
  CourtBooking.Application/       ← 1 project
      Identity/  Catalog/  Booking/  Payment/
  CourtBooking.Infrastructure/    ← 1 project
  CourtBooking.Api/               ← 1 project
tests/
  CourtBooking.UnitTests/
  CourtBooking.IntegrationTests/
```

| Ưu | Nhược |
|---|---|
| ✅ **6 project** — một người quản được | ❌ Ranh giới module chỉ là **quy ước**, không có ràng buộc biên dịch |
| ✅ Quy tắc phụ thuộc giữa tầng **được ép cứng** | ❌ `Infrastructure` gộp chung mọi module → nguy cơ thành "God project" |
| ✅ Build nhanh — **đo thật: 1,6 giây** | ❌ Tách microservice sau này phải gỡ thủ công |
| ✅ Dễ điều hướng, ít file `.csproj` phải nhớ | |

---

### Phương án B — Module ngoài, tầng trong *(vertical slice)*

```
src/
  Modules/
    Booking/
      CourtBooking.Booking.Domain/
      CourtBooking.Booking.Application/
      CourtBooking.Booking.Infrastructure/
    Catalog/   (3 project)
    Identity/  (3 project)
    Payment/   (3 project)
    Reporting/ (3 project)
  CourtBooking.Api/
```

| Ưu | Nhược |
|---|---|
| ✅ **Ranh giới module do TRÌNH BIÊN DỊCH ép** — module A không tham chiếu module B thì không gọi được | ❌ **~17 project** — quá tải nhận thức cho một người mới .NET |
| ✅ Tách microservice: cắt là xong | ❌ Build chậm hơn nhiều lần |
| ✅ Nhiều đội làm song song không dẫm chân | ❌ Mỗi lần thêm module phải tạo 3 project + nối 4 tham chiếu |
| | ❌ Giải quyết vấn đề của **đội 15 người**, dự án này có **1** |

**→ Không chọn.** Đây là mua một cái giá mà hiện không có ai trả.

---

### Phương án C — Một project duy nhất, chia bằng thư mục

| Ưu | Nhược |
|---|---|
| Đơn giản nhất, build nhanh nhất | ❌ **Không có ràng buộc nào cả.** `Domain` gọi thẳng EF Core được, và sẽ có người gọi |
| | ❌ Mất toàn bộ giá trị của Clean Architecture |
| | ❌ Không thể viết architecture test có ý nghĩa |

**→ Loại.** Thư mục là *gợi ý*; project là *ràng buộc*. Bỏ project là bỏ luôn người gác cổng.

---

### Phương án D — Lai: tách project theo module **chỉ ở tầng Domain**

`Domain` tách thành 5 project theo module; `Application`, `Infrastructure`, `Api` gộp.

| Ưu | Nhược |
|---|---|
| Ranh giới module được ép ở tầng quan trọng nhất | ❌ **Bất đối xứng gây rối** — người mới không đoán được quy tắc |
| ~10 project | ❌ Ranh giới bị ép ở `Domain` nhưng lỏng ở `Application` → rò rỉ qua đường Application |
| | ❌ Có đủ nhược điểm của B mà không có đủ ưu điểm |

**→ Loại.** Cấu trúc không nhất quán khó dạy và khó giữ hơn cấu trúc nhất quán, kể cả khi cấu trúc nhất quán đó lỏng hơn.

---

## 4. Quyết định

**Chọn phương án A.**

- **Tầng** → project *(ép bởi trình biên dịch)*
- **Module nghiệp vụ** → thư mục + namespace bên trong mỗi project *(ép bởi **architecture test**)*

```
CourtBooking.Domain.Booking.Booking          ← namespace phản ánh module
CourtBooking.Application.Catalog.Queries
```

### Ràng buộc kèm theo — **không được phép hoãn**

| # | Ràng buộc | Khi nào |
|---|---|---|
| 1 | Architecture test: `Domain` không tham chiếu EF Core / ASP.NET *(NFR-30)* | **S0-07** |
| 2 | Architecture test: module không gọi trực tiếp vào namespace của module khác *(NFR-31)* | **S0-07** |
| 3 | Namespace **phải** phản ánh module — cấm đặt class ở gốc project | Ngay từ file đầu tiên |

> ⚠️ Ràng buộc #2 là **điều kiện để quyết định này hợp lệ**. Chọn A mà không viết architecture test nghĩa là chọn phương án C trá hình — và ranh giới module sẽ tan trong khoảng 2 sprint.

---

## 5. Lý do chọn

1. **Thứ quan trọng nhất đã được ép cứng.** Quy tắc phụ thuộc giữa tầng — đặc biệt *"`Domain` không biết hạ tầng"* — là bất biến kiến trúc có giá trị cao nhất, và phương án A ép nó bằng trình biên dịch. Ranh giới module xếp sau về mức độ quan trọng.

2. **Ranh giới module ở quy mô này giữ được bằng test.** 5–6 module, một người viết, mọi vi phạm đều bị CI chặn ngay. Cơ chế nhẹ hơn nhiều so với 17 project mà kết quả gần tương đương.

3. **Chi phí nhận thức là ràng buộc thật, không phải cái cớ.** Người thực hiện **lần đầu làm .NET**. 17 project sẽ tiêu tốn phần lớn ngân sách 10–15h/tuần vào việc quản lý cấu trúc thay vì học nghiệp vụ và kỹ thuật lõi. Đây là đánh đổi có ý thức, không phải cắt xén.

4. **YAGNI.** Phương án B giải quyết ba vấn đề: nhiều đội làm song song, deploy độc lập từng module, biên dịch song song quy mô lớn. Dự án này **không có vấn đề nào trong ba**.

5. **Đường lui rẻ và đã biết.** Chuyển A → B là refactor cơ học *(tách project, dời file, nối lại tham chiếu)*. Và nếu architecture test đã chạy từ đầu, ranh giới module lúc đó **đã sạch** — việc tách chỉ là hợp thức hoá thứ vốn đã đúng.

---

## 6. Hệ quả

### ✅ Tích cực
- Build 1,6 giây → vòng lặp phản hồi nhanh, quan trọng với người đang học
- Sáu project — mở solution ra là hiểu ngay bố cục
- `Domain.csproj` **rỗng trơn**, không package nào — bằng chứng trực quan rằng tầng lõi sạch
- Toàn bộ ngân sách thời gian dồn vào nghiệp vụ và kỹ thuật lõi

### ⚠️ Tiêu cực / cần theo dõi

1. **Ranh giới module là quy ước, không phải ràng buộc.** Không có gì về mặt biên dịch ngăn `Application/Booking/CreateBookingHandler.cs` gọi thẳng `Application/Catalog/CourtRepository.cs`. **Chỉ có architecture test đứng giữa.** Nếu test đó bị bỏ hoặc bị tắt để "cho build xanh", quyết định này mất hiệu lực.

2. **`Infrastructure` có nguy cơ thành "God project".** `DbContext`, migration, repository của **mọi** module nằm chung. Cần kỷ luật thư mục nghiêm ngặt, và cân nhắc tách `DbContext` theo module khi vượt ~40 entity.

3. **Namespace phải kỷ luật ngay từ file đầu tiên.** Một class đặt sai chỗ ở tuần thứ nhất sẽ được 20 file khác bắt chước.

4. **Migration EF Core gộp chung.** Mọi module chia sẻ một chuỗi migration → hai người sửa schema cùng lúc sẽ xung đột. Hiện chỉ một người nên chưa đau; ghi lại để nhớ.

### 🔮 Điểm gãy đã biết

> Quyết định này hỏng khi **một trong ba điều** xảy ra:
>
> **(a)** Có từ 2 đội trở lên cùng làm — lúc đó ranh giới bằng quy ước không đủ, cần ranh giới bằng biên dịch → chuyển sang **phương án B**.
> **(b)** Một module cần **deploy độc lập** — tách project trước, tách service sau.
> **(c)** 🔴 **Architecture test không được viết ở S0-07, hoặc bị tắt về sau** — đây là cách hỏng **có khả năng xảy ra nhất**, và nó hỏng *âm thầm*: không ai báo lỗi, ranh giới chỉ đơn giản là biến mất dần.

---

## 7. Kiểm chứng bằng test

Ba test bắt buộc ở **S0-07**:

```csharp
[Fact] // NFR-30
public void Domain_ShouldNotDependOn_Infrastructure()
    => Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOnAny("Microsoft.EntityFrameworkCore",
                                             "Microsoft.AspNetCore", "Npgsql")
            .GetResult().IsSuccessful.Should().BeTrue();

[Fact] // NFR-31 — test bảo vệ chính ADR này
public void Modules_ShouldNotDependOnEachOther_Directly()
    => Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespace("CourtBooking.Application.Booking")
            .ShouldNot().HaveDependencyOn("CourtBooking.Application.Catalog")
            .GetResult().IsSuccessful.Should().BeTrue();

[Fact] // Ràng buộc #3
public void AllTypes_ShouldResideIn_AModuleNamespace() { /* cấm class ở gốc project */ }
```

> Test thứ hai là **lý do tồn tại của ADR này**. Không có nó, phương án A suy biến thành phương án C — thứ đã bị loại ở §3.

---

## 8. Câu hỏi phỏng vấn liên quan

1. Modular Monolith và Clean Architecture có mâu thuẫn nhau không? *(→ không — hai trục khác nhau; câu hỏi thật là trục nào thành project)*
2. Vì sao bạn chia project theo tầng chứ không theo module nghiệp vụ?
3. Nếu vậy thì lấy gì đảm bảo module không gọi chéo nhau?
4. Khi nào bạn sẽ chuyển sang tách project theo module?
5. Chi phí chuyển đổi từ cấu trúc hiện tại sang vertical slice là bao nhiêu?
6. Vì sao không gộp hết vào một project cho đơn giản?
7. `Infrastructure` gộp chung mọi module có phải vấn đề không? Bạn theo dõi nó thế nào?
