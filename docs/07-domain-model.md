# 07 — Mô hình miền (Domain Model)

> 🚧 **CHƯA VIẾT — ƯU TIÊN CAO NHẤT.** Phải hoàn thành **trước Sprint 0 task S0-03** (migration đầu tiên).
> Dùng skill `/doc-domain-model` để viết.

---

## Vì sao file này chặn Sprint 0

Thứ tự thiết kế đúng là:

```
Business Rules  →  Domain Model  →  Database Schema
                   (Aggregate,       (bảng chỉ là CÁCH LƯU
                    Invariant,        của domain, không phải
                    Domain Event)     ngược lại)
```

Nếu bỏ qua bước giữa và đi thẳng từ rule sang bảng (tư duy **data-first**), hậu quả là **Anemic Domain Model**: các class trong tầng `Domain` chỉ còn property, toàn bộ logic nghiệp vụ tràn vào Handler, và Clean Architecture chỉ còn là mấy cái thư mục rỗng nghĩa.

Đây là **rủi ro R-11** trong [17-risk-analysis.md](17-risk-analysis.md).

---

## Dàn ý bắt buộc

### 1. Ngôn ngữ chung (Ubiquitous Language)
Đối chiếu với [00-glossary.md](00-glossary.md) — tên trong code phải trùng tên nghiệp vụ.

### 2. Bounded Context / Module
Đề xuất: `Identity` · `Catalog` · `Booking` · `Payment` · `Reporting`
Với mỗi context: trách nhiệm, dữ liệu sở hữu, cách giao tiếp với context khác.

### 3. Aggregate và ranh giới
| Aggregate Root | Thành phần bên trong | Bất biến phải bảo vệ |
|---|---|---|
| `Booking` | `BookingSlot` (collection) | BR-02 (slot liên tiếp, cùng sân), BR-06, đồng bộ `is_active` với `Status` |
| `Court` | `CourtClosure` | Không đóng chồng lấn |
| `RecurringSeries` | — | BR-24 (`generated_until`) |
| `Payment` | — | BR-15 (idempotency) |

**Quy tắc vàng:** một transaction chỉ sửa **một** aggregate. Giao tiếp giữa aggregate qua **Domain Event**.

### 4. Entity vs Value Object
| Value Object cần có | Vì sao |
|---|---|
| `TimeSlot` | Đóng gói BR-01 (giờ chẵn) — không thể tạo slot 18:30 |
| `Money` | Đóng gói BR: không âm, cùng đơn vị tiền tệ |
| `PhoneNumber` | Chuẩn hoá + validate ngay khi khởi tạo |
| `BookingCode` | Định dạng `BK-YYMM-NNNN` |
| `DateRange` | Dùng cho `CourtClosure`, `RecurringSeries` |

### 5. Trạng thái & hành vi của `Booking`
Liệt kê **method**, không phải property setter:
`Booking.CreateOnline()` · `CreateAtCounter()` · `ConfirmPayment()` · `Expire()` · `Cancel()` · `CheckIn()` · `MarkNoShow()` · `Complete()`
Mỗi method: tiền điều kiện, hậu điều kiện, domain event phát ra.

### 6. Domain Event
`BookingCreated` · `BookingConfirmed` · `BookingCancelled` · `BookingExpired` · `NoShowRecorded` · `RefundRequested` · `TrustedStatusRevoked`
Với mỗi event: ai phát, ai lắng nghe, đi qua Outbox hay không.

### 7. Domain Service
Chỉ tạo khi logic **không thuộc về** một aggregate nào:
`PricingService` (tra `PriceRule` theo độ ưu tiên) · `RefundPolicyService` (BR-16)

### 8. Sơ đồ lớp (mermaid `classDiagram`)

### 9. Ánh xạ Domain → Bảng
Bảng đối chiếu sang [10-database-design.md](10-database-design.md), **nêu rõ chỗ nào phi chuẩn hoá có chủ đích và vì sao**.

---

## Tiêu chí hoàn thành

- [ ] Mỗi `BR-xx` được bảo vệ ở **một chỗ xác định** trong domain (nêu rõ class + method)
- [ ] Không có setter công khai trên aggregate root — chỉ có method mang nghĩa nghiệp vụ
- [ ] Mọi ranh giới aggregate đều giải thích được: *"vì sao X ở trong, Y ở ngoài?"*
- [ ] Tầng Domain **không** tham chiếu EF Core, ASP.NET, hay thư viện hạ tầng nào (NFR-30)
- [ ] Có ánh xạ rõ ràng sang schema, kể cả các điểm phi chuẩn hoá
