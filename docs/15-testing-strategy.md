# 15 — Chiến lược kiểm thử (Testing Strategy)

> 🚧 **CHƯA VIẾT.** Viết ở Sprint 1, cập nhật liên tục. Dùng skill `/doc-testing-strategy`.

---

## Dàn ý bắt buộc

### 1. Kim tự tháp kiểm thử cho dự án này
| Tầng | Tỉ lệ | Kiểm cái gì | Công cụ |
|---|:--:|---|---|
| **Unit** | ~60% | Logic Domain thuần: `TimeSlot`, `Money`, `Booking.Cancel()`, tính mức hoàn tiền | xUnit + FluentAssertions |
| **Integration** | ~35% | Thứ **chỉ đúng khi có CSDL thật**: BR-06, transaction, Global Query Filter, migration | **Testcontainers PostgreSQL** |
| **E2E** | ~5% | Vài luồng sống còn: đặt → thanh toán → xác nhận | WebApplicationFactory |

### 2. 🔴 Quy tắc tuyệt đối: **cấm EF Core InMemory**

> InMemory provider **không hỗ trợ unique index và không có transaction thật**. Test chống trùng lịch sẽ **xanh giả** trong khi production vẫn đặt trùng. Đây là rủi ro **R-04** — một trong ba rủi ro điểm 9 của dự án.

Mọi test chạm CSDL đều dùng **Testcontainers** với PostgreSQL thật.

### 3. Quy ước đặt tên gắn với Business Rule
```csharp
// Mẫu: <Đối tượng>_Should<Kết quả>_When<Điều kiện>_<Mã rule>
Booking_ShouldRejectSecondBooking_WhenSameCourtAndSlot_BR06()
Refund_ShouldReturn50Percent_WhenCancelledBetween4And24Hours_BR16()
Customer_ShouldLoseTrustedStatus_AfterTwoNoShowsIn90Days_BR22()
```
→ Cho phép truy vết: **mỗi `BR-xx` có test nào phủ** (NFR-28).

### 4. Test bắt buộc theo use case
| Test | Vì sao bắt buộc |
|---|---|
| 20 request song song vào cùng slot → đúng 1 thành công, 19 nhận 409 | Chứng minh BR-06 |
| Webhook gửi 2 lần → chỉ xử lý 1 lần | BR-15 |
| Chữ ký webhook sai → từ chối, không đổi trạng thái đơn | Bảo mật |
| Đăng nhập tenant A → đọc dữ liệu tenant B phải **rỗng** | Chống rò rỉ, R-02 |
| Partner đổi `branchId` → nhận 403/404 | Chống IDOR, R-03 |
| Job sinh buổi định kỳ chạy 2 lần → không sinh trùng | BR-24, idempotency |
| Cổng hoàn tiền lỗi → đơn vẫn `Cancelled`, slot vẫn được giải phóng | UC-12/E3 |

### 5. Architecture Test
Domain không tham chiếu EF Core / ASP.NET (NFR-30) · module không join thẳng bảng của nhau (NFR-31) · không package InMemory trong test project.

### 6. Dữ liệu test
Builder pattern · mỗi test tự dựng dữ liệu, không phụ thuộc thứ tự chạy · reset CSDL giữa các test (Respawn hoặc container mới).

### 7. Test phụ thuộc thời gian
Inject `TimeProvider` thay vì `DateTime.UtcNow` — nếu không, test BR-16 (hoàn tiền theo mốc 24h/4h) và BR-22 (90 ngày) **không thể viết được**.

### 8. Ngưỡng chất lượng trong CI
Coverage Domain + Application ≥ **70%** (NFR-27) · mọi test phải xanh mới được merge · không có test bị `Skip` mà không ghi lý do.

---

## Tiêu chí hoàn thành

- [ ] Mỗi `BR-xx` trong [06-business-rules.md](06-business-rules.md) có ít nhất một test trích mã rule
- [ ] Toàn bộ luồng ngoại lệ 🔥 trong [05-use-cases.md](05-use-cases.md) có integration test
- [ ] CI chạy được toàn bộ test trên máy trắng
- [ ] Không có test nào dùng EF Core InMemory
