# ADR-0002 — Đổi grain slot từ 60 phút xuống 30 phút

| | |
|---|---|
| **Trạng thái** | ✅ Accepted |
| **Ngày** | 2026-07-31 |
| **Người quyết định** | Solution Architect + Chủ sân |
| **Xuất phát từ** | [CR-07](../21-change-requests.md) |
| **Liên quan** | [ADR-0001](0001-booking-concurrency-strategy.md) *(**không** bị lật)* · BR-01, BR-33, BR-14b |

---

## 1. Bối cảnh

Baseline ban đầu chốt đơn vị đặt sân là **1 giờ chẵn** (BR-01), với lý do khách hàng nêu: *"có ông đòi thuê tiếng rưỡi mà tôi không cho, rắc rối sổ sách"*.

Sau khi xem bản thiết kế, Chủ sân đổi ý và yêu cầu **cho thuê 30 phút và 1,5 tiếng**.

Đây chính là **điểm gãy đã được ghi trước** tại [ADR-0001](0001-booking-concurrency-strategy.md) §6, mục hệ quả tiêu cực #4:

> *"Nếu sau này cho phép đặt 30/90 phút, index này không còn đủ → chuyển sang phương án F (`EXCLUDE` + `tstzrange`)."*

Điểm gãy xảy ra **trong tuần đầu tiên**, trước khi có dòng code nào — thời điểm rẻ nhất có thể để xử lý.

---

## 2. Vấn đề

Thiết kế hiện tại đặt bất biến BR-06 lên một **partial unique index** trên `booking_slot(court_id, slot_start_utc) WHERE is_active`. Index này chỉ hoạt động khi **slot là các mốc rời rạc, không chồng lấn**.

Câu hỏi: cho phép 30 phút và 90 phút thì ràng buộc này còn dùng được không, hay phải thay bằng cơ chế chống overlap tổng quát?

---

## 3. Các phương án đã cân nhắc

### Phương án A — Giữ nguyên 60 phút, từ chối CR-07

| Ưu | Nhược |
|---|---|
| Chi phí 0 | Không đáp ứng nhu cầu kinh doanh thật |
| | Mất cơ hội lấp khung giờ thấp điểm bằng lượt đặt ngắn |

**→ Loại.** Yêu cầu đến từ chính người trả tiền, và nó hợp lý.

---

### Phương án B — Grain 30 phút, **căn mốc `:00` / `:30`** *(ĐÃ CHỌN)*

Slot = 30 phút. Mọi lượt đặt là **bội số của 30 phút** và bắt đầu tại mốc cố định.
`60′ = 2 slot` · `90′ = 3 slot` · `120′ = 4 slot`.

| Ưu | Nhược |
|---|---|
| ✅ **Partial unique index giữ nguyên** — chỉ đổi ý nghĩa `slot_start_utc`, không đổi cấu trúc | Số dòng `booking_slot` tăng gấp đôi |
| ✅ ADR-0001 **không bị lật** | Nguy cơ **phân mảnh lịch** (xử lý bằng BR-33) |
| ✅ EF Core hỗ trợ tốt, không cần raw SQL | |
| ✅ Truy vấn lịch trống vẫn là lookup thẳng | |
| ✅ Chi phí ~2 giờ sửa tài liệu, 0 dòng code phải viết lại | |

**Về số dòng:** 15 sân × 36 slot × 365 ngày ≈ 197k dòng/năm. PostgreSQL không coi con số này là gì cả.

---

### Phương án C — Grain 15 phút

| Ưu | Nhược |
|---|---|
| Linh hoạt hơn nữa | Không ai đặt sân cầu lông 15 phút |
| | Số dòng ×4, phân mảnh lịch nghiêm trọng hơn |
| | Lưới lịch trên giao diện trở nên khó dùng |

**→ Loại.** Linh hoạt không ai cần, đổi lấy chi phí thật.

---

### Phương án D — `EXCLUDE` constraint với `tstzrange`, giờ bắt đầu tự do

```sql
ALTER TABLE booking ADD CONSTRAINT no_overlap
  EXCLUDE USING gist (court_id WITH =, tstzrange(start_utc, end_utc) WITH &&)
  WHERE (status IN ('PendingPayment','Confirmed','CheckedIn','Completed'));
```

| Ưu | Nhược |
|---|---|
| Tổng quát nhất — chống overlap với **mọi** khoảng thời gian | Khách chọn được 18:17 — **không ai cần điều này** |
| Không cần bảng `booking_slot` | EF Core hỗ trợ kém, phải raw SQL migration |
| | Cần extension `btree_gist`, index GiST nặng hơn B-tree |
| | **Lật ADR-0001**, phải viết lại toàn bộ phần chống trùng |
| | Bảng giá theo khung giờ trở nên rắc rối — một booking cắt ngang nhiều khung giá |
| | Chi phí ~1–2 ngày + rủi ro |

**→ Không chọn cho v1.** Giữ lại làm đường lui nếu nghiệp vụ thay đổi lần nữa.

---

## 4. Quyết định

**Chọn phương án B.** Grain = **30 phút**, căn mốc `:00` / `:30`.

Kèm theo, thêm **BR-33** để xử lý tác dụng phụ:

| Khung giờ | Thời lượng tối thiểu | Tối đa |
|---|:--:|:--:|
| **Cao điểm** | **60 phút** | 240 phút |
| **Thấp điểm** | **30 phút** | 240 phút |

Ngưỡng lưu ở `price_rule.min_duration_minutes` / `max_duration_minutes` — cấu hình được, không hardcode.

Giá slot 30 phút = `tenant.half_hour_price_ratio` × giá giờ, **mặc định 0.5** (BR-14b).

---

## 5. Lý do chọn

1. **Không ai đặt sân cầu lông lúc 18:17.** Sự linh hoạt mà phương án D mua về không có người mua. Ngoài đời người ta hẹn "7 giờ" hoặc "7 rưỡi".
2. **Bảo toàn toàn bộ khoản đầu tư vào ADR-0001.** Bất biến quan trọng nhất hệ thống vẫn do một dòng SQL bảo đảm.
3. **Tải không đổi về bản chất.** 197k dòng/năm vẫn nằm sâu dưới mọi ngưỡng cần lo lắng.
4. **BR-33 biến nhược điểm thành lợi thế kinh doanh.** Cho 30 phút ở khung **thấp điểm** — nơi Chủ sân đang muốn lấp chỗ trống; chặn nó ở khung **cao điểm** — nơi phân mảnh lịch gây mất doanh thu thật. Quy tắc kỹ thuật khớp chính xác với mục tiêu kinh doanh.

### Vì sao BR-33 cần thiết — bài toán phân mảnh lịch

```
Nếu cho đặt 30 phút ở mọi khung giờ, tối thứ Sáu có thể thành:

  18:00–18:30  [ĐÃ ĐẶT]
  18:30–19:00  trống  ← 30 phút mồ côi
  19:00–19:30  [ĐÃ ĐẶT]
  19:30–20:00  trống  ← 30 phút mồ côi

Sân còn 1 tiếng trống nhưng KHÔNG AI đặt được 1 tiếng liền.
Mất doanh thu đúng vào khung giờ đắt nhất.
```

---

## 6. Hệ quả

### ✅ Tích cực
- Đáp ứng đủ nhu cầu kinh doanh với chi phí gần bằng không
- ADR-0001 và toàn bộ chiến lược chống trùng lịch **còn nguyên hiệu lực**
- Cùng cấu trúc `booking_slot` phục vụ luôn việc dời lịch nguyên tử ([ADR-0003](0003-atomic-reschedule.md))
- Tham số hoá theo tenant → chủ sân khác có chính sách khác cũng dùng chung hệ thống được

### ⚠️ Tiêu cực / cần lưu ý
1. **Số dòng `booking_slot` tăng gấp đôi.** Không đáng lo ở quy mô này, nhưng cần nhớ khi ước lượng lại tải.
2. **Nguy cơ phân mảnh lịch vẫn tồn tại ở khung thấp điểm.** Chấp nhận có ý thức — đó là khung đang ế, lấp được phút nào tốt phút đó. **Cần theo dõi bằng báo cáo tỉ lệ lấp đầy.** Ghi thành rủi ro **R-25**.
3. **Lưới lịch trên giao diện dài gấp đôi.** Cần cân nhắc trải nghiệm trên điện thoại (80% truy cập — NFR-40).
4. **Bảng giá vẫn lưu giá THEO GIỜ**, giá slot suy ra bằng tỉ lệ. Nếu sau này cần đặt giá riêng cho từng slot 30 phút, phải đổi cấu trúc `price_rule`.

### 🔮 Điểm gãy đã biết
> Thiết kế này hỏng khi nghiệp vụ yêu cầu **giờ bắt đầu tự do** (ví dụ đặt 18:17), hoặc thời lượng **không phải bội số của 30 phút** (ví dụ 45 phút).
> Khi đó: chuyển sang **phương án D** — `EXCLUDE` constraint với `tstzrange` + extension `btree_gist`. Ước lượng 1–2 ngày, và sẽ **lật ADR-0001**.

---

## 7. Kiểm chứng bằng test

```csharp
[Fact] // BR-01
public void TimeSlot_ShouldReject_When_StartIsNotOnHalfHourBoundary()
    => Assert.Throws<DomainException>(() => TimeSlot.Create(At("18:17"), 30));

[Fact] // BR-33
public async Task Booking_ShouldReject30Minutes_When_InPeakHours_BR33() { … }

[Fact] // BR-33
public async Task Booking_ShouldAllow30Minutes_When_InOffPeakHours_BR33() { … }

[Fact] // BR-01, BR-02
public async Task Booking_ShouldCreate3Slots_When_Duration90Minutes() { … }

[Fact] // BR-06 — vẫn phải xanh sau khi đổi grain
public async Task Should_AllowOnlyOneBooking_When_20RequestsHitSameSlotConcurrently() { … }
```

> ⚠️ Test cuối cùng là **test hồi quy quan trọng nhất** của ADR này: nó chứng minh việc đổi grain **không** phá vỡ bất biến BR-06.
> Chạy trên **Testcontainers PostgreSQL**, không dùng EF Core InMemory *(InMemory không có unique index — sẽ cho kết quả xanh giả)*.

---

## 8. Câu hỏi phỏng vấn liên quan

1. "Grain" trong thiết kế CSDL là gì? Chọn sai grain thì hậu quả ra sao?
2. Khách hàng đổi từ đặt-theo-giờ sang đặt-30-phút. Bạn đánh giá tác động thế nào?
3. Vì sao bạn không chọn giải pháp tổng quát (`EXCLUDE` + `tstzrange`) cho chắc?
4. Phân mảnh lịch là gì? Bạn xử lý bằng cách nào?
5. Bạn biết trước thiết kế sẽ hỏng khi nào không? *(→ câu này là chỗ để nói về mục "điểm gãy đã biết" — thứ khiến người phỏng vấn nhớ bạn)*
6. Đổi grain có làm hỏng ràng buộc chống đặt trùng không? Bạn chứng minh bằng cách nào?
