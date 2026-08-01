# ADR-0003 — Dời lịch nguyên tử, không dùng "hủy rồi đặt lại"

| | |
|---|---|
| **Trạng thái** | ✅ Accepted |
| **Ngày** | 2026-07-31 |
| **Người quyết định** | Solution Architect + Chủ sân |
| **Xuất phát từ** | [CR-08a, CR-08b](../21-change-requests.md) |
| **Liên quan** | [ADR-0001](0001-booking-concurrency-strategy.md) · [ADR-0002](0002-slot-grain-30-minutes.md) · BR-34…BR-42 |

---

## 1. Bối cảnh

Chủ sân đề nghị: khách báo trước `N` giờ thì được **hoàn tiền hoặc dời lịch**.

Đây là **hai việc khác nhau** bị gộp trong một câu:

| | Bản chất | Độ khó |
|---|---|---|
| **Hoàn tiền theo cửa sổ mới** | Đổi tham số của rule đã có (BR-16) | 🟢 Rẻ |
| **Dời lịch (reschedule)** | **Tính năng hoàn toàn mới** | Cần quyết định kiến trúc |

ADR này giải quyết phần thứ hai.

---

## 2. Vấn đề

Dời một đơn từ 19:00 sang 20:00 gồm hai thao tác:
1. **Chiếm** slot mới
2. **Giải phóng** slot cũ

Nếu hai thao tác này không nguyên tử, tồn tại một cửa sổ nguy hiểm:

```
T1  Hủy đơn 19:00              → slot 19:00 được giải phóng
T2  (0,5 giây sau)             → người khác cướp slot 20:00
T3  Khách cố đặt 20:00         → 409 Conflict

Kết quả: khách MẤT CẢ HAI slot.
```

Tình huống này xảy ra **đúng vào giờ cao điểm** — lúc khách cần dời nhất và slot khan hiếm nhất.

---

## 3. Các phương án đã cân nhắc

### Phương án A — Không làm gì, bảo khách "tự hủy rồi đặt lại"

| Ưu | Nhược |
|---|---|
| 0 dòng code | 🔴 **Khách có thể mất cả hai slot** (kịch bản T1–T3) |
| | Hủy = có thể phải hoàn tiền ⇒ mất doanh thu, dù khách vẫn muốn chơi |
| | Không giữ được dấu vết "đơn này từng bị dời" — không thống kê được |
| | Không giới hạn được số lần dời |

**→ Loại.** Nhìn thì "đơn giản hơn", thực chất là **đẩy rủi ro sang khách hàng**.

---

### Phương án B — Hủy rồi đặt lại, nhưng **giữ chỗ ưu tiên** cho khách vài phút

Sau khi hủy, hệ thống khoá slot mới cho riêng khách đó trong ~2 phút.

| Ưu | Nhược |
|---|---|
| Giảm rủi ro mất chỗ | Phải xây **cơ chế giữ chỗ có chủ sở hữu** — hoàn toàn mới |
| | Vẫn có cửa sổ hỏng nếu khách đóng trình duyệt giữa chừng |
| | Phức tạp hơn phương án C mà bảo đảm yếu hơn |

**→ Loại.** Tốn công hơn mà kết quả tệ hơn.

---

### Phương án C — Hoán đổi nguyên tử trong một transaction *(ĐÃ CHỌN)*

```csharp
using var tx = await _db.Database.BeginTransactionAsync();

booking.Reschedule(newSlots, _timeProvider.GetUtcNow());
//  ├─ INSERT booking_slot mới    → uq_slot_no_double_booking tự chặn nếu bị chiếm
//  ├─ UPDATE slot cũ is_active = false
//  ├─ UPDATE booking.start_utc / end_utc / reschedule_count
//  └─ Thêm OutboxMessage: BookingRescheduled

await _db.SaveChangesAsync();   // vi phạm unique → ROLLBACK TOÀN BỘ
await tx.CommitAsync();
```

| Ưu | Nhược |
|---|---|
| ✅ **Khách không bao giờ mất cả hai** — rollback thì đơn cũ nguyên vẹn | Cần một command handler mới (~3–4 giờ) |
| ✅ **Dùng lại đúng ràng buộc của ADR-0001**, không thêm cơ chế nào | |
| ✅ Giữ được doanh thu — khách đổi giờ chứ không hủy | |
| ✅ Có `reschedule_count` để giới hạn và thống kê | |
| ✅ Chuẩn hoá được: giá chênh lệch, cửa sổ thời gian, số lần | |

---

### Phương án D — Distributed lock trên hai slot trước khi đổi

| Ưu | Nhược |
|---|---|
| Kiểm soát tường minh | Thêm Redis vào đường đi quan trọng ⇒ **thêm một chế độ hỏng hóc** |
| | Redlock **không an toàn tuyệt đối** |
| | **Thừa** — transaction của PostgreSQL đã làm đúng việc này |

**→ Loại.** Vi phạm nguyên tắc *"thêm hạ tầng = thêm chế độ hỏng hóc"*.

---

## 4. Quyết định

**Chọn phương án C.** Dời lịch là một **thao tác nguyên tử duy nhất** ở tầng API (`POST /bookings/{id}/reschedule`), thực hiện trong một transaction, dựa vào chính partial unique index của ADR-0001 để phát hiện xung đột.

### Tham số nghiệp vụ kèm theo

| Rule | Nội dung |
|---|---|
| **BR-36** | Cửa sổ dời = `tenant.reschedule_window_hours`, mặc định **2 giờ** trước giờ chơi |
| **BR-38** | Tối đa `tenant.max_reschedule_count` lần/đơn, mặc định **2** |
| **BR-38** | Giá mới **cao hơn** → bù thêm trước khi xác nhận · **thấp hơn** → **không** hoàn chênh lệch |
| **BR-38** | Slot mới phải cách hiện tại ≥ `N` giờ *(chặn lách chính sách)* |
| **BR-39** | Ngày khác ✅ · sân khác ✅ · **chi nhánh khác ❌** (v1) |
| **BR-42** | Dời lịch **không** tính vào thống kê no-show |

---

## 5. Lý do chọn

1. **Chi phí thật thấp hơn nhiều so với đánh giá ban đầu.** Nhận định đầu tiên của tôi ("dời lịch đắt hơn vẻ ngoài") **là sai** — nó chỉ đúng nếu triển khai bằng hai lời gọi API. Trong một transaction, cấu trúc `booking_slot` hiện có xử lý trọn vẹn.
2. **Không thêm một cơ chế nào.** Cùng một index vừa chống đặt trùng, vừa bảo đảm hoán đổi an toàn. Đây là dấu hiệu thiết kế tốt: một ràng buộc phục vụ nhiều mục đích.
3. **Bảo vệ doanh thu.** Khách dời thì tiền vẫn ở lại; khách hủy thì có thể phải hoàn. Với nhóm khách định kỳ *(~40% doanh thu)*, dời lịch là van xả áp cần thiết.
4. **`BR-42` khuyến khích hành vi đúng.** Phạt việc dời lịch là gián tiếp khuyến khích no-show — kết quả tệ hơn cho cả hai bên.

---

## 6. Hệ quả

### ✅ Tích cực
- Khách không bao giờ rơi vào cảnh mất cả hai slot
- Doanh thu được giữ lại thay vì hoàn ra
- `reschedule_count` cho phép giới hạn lạm dụng và thống kê hành vi
- Là chất liệu phỏng vấn rất mạnh: *"hoán đổi nguyên tử bằng cách dùng lại đúng ràng buộc CSDL sẵn có"*

### ⚠️ Tiêu cực / cần lưu ý
1. **Transaction dài hơn giao dịch thường** — chạm 2 nhóm slot. Vẫn rất ngắn ở quy mô này, nhưng **tuyệt đối không được gọi API bên ngoài bên trong transaction này** *(ví dụ: gọi cổng thanh toán để thu tiền bù chênh lệch)*. Thu tiền bù phải xong **trước**, rồi mới mở transaction.
2. **Giá chênh lệch làm luồng phức tạp hơn.** Nếu slot mới đắt hơn, phải có bước thanh toán bổ sung trước khi hoán đổi — nghĩa là dời lịch có thể ở trạng thái chờ.
3. **Đơn định kỳ** dời một buổi làm buổi đó lệch khỏi khuôn mẫu chuỗi. Chấp nhận: buổi đó vẫn giữ `series_id` để truy vết, nhưng thời gian khác các buổi còn lại.
4. **`refund_override_amount` do Manager đặt** có thể mâu thuẫn với việc đơn sau đó bị dời. Quy tắc: đơn **đã hủy** mới có override; đơn còn hiệu lực thì không.

### 🔮 Điểm gãy đã biết
> Thiết kế này hỏng khi cần **dời sang chi nhánh khác** — lúc đó `branch_id` đổi, kéo theo bảng giá khác, phạm vi phân quyền khác, và có thể cả tenant khác. Sẽ cần một luồng riêng, không phải mở rộng luồng này.
>
> Cũng hỏng nếu cho phép dời **một phần** đơn (ví dụ đơn 2 tiếng chỉ dời 1 tiếng sau). Hiện tại dời là **toàn bộ đơn**.

---

## 7. Kiểm chứng bằng test

```csharp
[Fact] // BR-37 — test quan trọng nhất của ADR này
public async Task Reschedule_ShouldKeepOriginalBooking_When_NewSlotAlreadyTaken_BR37()
{
    var booking = await CreateConfirmedBooking(court: 3, at: "19:00");
    await CreateConfirmedBooking(court: 3, at: "20:00");   // người khác chiếm chỗ

    var res = await _client.PostAsJsonAsync(
        $"/api/v1/bookings/{booking.Id}/reschedule", new { newStart = At("20:00") });

    res.StatusCode.Should().Be(HttpStatusCode.Conflict);

    var after = await Reload(booking.Id);
    after.Status.Should().Be(BookingStatus.Confirmed);        // đơn cũ NGUYÊN VẸN
    after.StartUtc.Should().Be(At("19:00"));
    (await SlotIsActive(court: 3, at: "19:00")).Should().BeTrue();  // vẫn giữ chỗ
}

[Fact] // BR-38
public async Task Reschedule_ShouldReject_When_ExceedsMaxCount_BR38() { … }

[Fact] // BR-38
public async Task Reschedule_ShouldRequireTopUp_When_NewSlotIsMoreExpensive_BR38() { … }

[Fact] // BR-36
public async Task Reschedule_ShouldReject_When_LessThanWindowHoursBeforeStart_BR36() { … }

[Fact] // BR-42
public async Task Reschedule_ShouldNotIncreaseNoShowCount_BR42() { … }
```

> Test đầu tiên là **lý do tồn tại của cả ADR này**. Nó chứng minh điều mà phương án "hủy rồi đặt lại" **không thể** đảm bảo.
> Bắt buộc chạy trên **Testcontainers PostgreSQL** — InMemory provider không có transaction thật lẫn unique index.

---

## 8. Câu hỏi phỏng vấn liên quan

1. Dời lịch khác gì hủy rồi đặt lại? Vì sao cách thứ hai không an toàn?
2. Làm sao đảm bảo hai thao tác (giải phóng chỗ cũ, chiếm chỗ mới) không bị nửa vời?
3. Vì sao không dùng distributed lock cho việc này?
4. Transaction của bạn dài bao lâu? Có gọi service bên ngoài trong đó không? Vì sao không được?
5. Nếu slot mới đắt hơn, luồng thanh toán bù diễn ra ở đâu so với transaction?
6. Vì sao dời lịch **không** bị tính vào thống kê no-show?
