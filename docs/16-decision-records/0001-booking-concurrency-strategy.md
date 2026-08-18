# ADR-0001 — Chiến lược chống đặt trùng sân (Booking Concurrency)

| | |
|---|---|
| **Trạng thái** | ✅ Accepted |
| **Ngày** | 2026-07-30 |
| **Người quyết định** | Solution Architect |
| **Liên quan** | BR-06, BR-07, BR-11 · `design-decisions.md` §2 |

---

## 1. Bối cảnh (Context)

Đây là **bất biến quan trọng nhất** của toàn hệ thống:

> **BR-06 — Một sân + một khung giờ ⇒ tối đa MỘT booking đang hiệu lực.**

Vi phạm rule này gây hậu quả trực tiếp ngoài đời thật: hai nhóm khách cùng đến sân, cãi nhau tại quầy, mất uy tín, phải hoàn tiền và đền bù. Đây chính là **vấn đề số một** khiến Chủ sân bỏ tiền làm hệ thống — sổ tay giấy không có cơ chế nào chặn hai nhân viên ghi trùng một ô.

### Đặc điểm tải (từ NFR)

```
~110 booking/ngày · đỉnh 17:00–23:00 · đồng thời tối đa ~50 người
```

Tải **rất thấp**. Nhưng — và đây là điểm mấu chốt phải hiểu đúng:

> **Chống đặt trùng là bài toán ĐÚNG ĐẮN DỮ LIỆU, không phải bài toán HIỆU NĂNG.**
> Dù chỉ 2 request/giây, hai người bấm nút cách nhau 5ms vẫn ghi đè lên nhau nếu thiếu ràng buộc. Tải thấp **không** làm giảm rủi ro, nó chỉ làm giảm **tần suất** — và một lỗi hiếm gặp trong tiền bạc còn tệ hơn một lỗi thường xuyên, vì nó lọt qua test và nổ trong production.

### Ràng buộc bổ sung
- Hệ thống có thể chạy **nhiều instance API** sau Nginx → không dùng được khoá trong bộ nhớ tiến trình (`lock`, `SemaphoreSlim`).
- Booking `PendingPayment` **phải chiếm slot** (BR-07), nếu không hai người cùng vào cổng thanh toán cho một sân.
- Booking đã hủy **phải giữ lại** trong CSDL (audit, báo cáo) nhưng **phải giải phóng slot**.

---

## 2. Vấn đề cần giải quyết

Race condition kiểu **TOCTOU** (Time-Of-Check to Time-Of-Use):

```
Thời điểm   Luồng A                          Luồng B
─────────────────────────────────────────────────────────────────
T1          SELECT ... WHERE slot=19h  → trống
T2                                           SELECT ... WHERE slot=19h  → trống
T3          INSERT booking_slot        → OK
T4                                           INSERT booking_slot        → OK  ❌ TRÙNG
```

Kiểm tra ở tầng ứng dụng **không thể** đóng được khe hở giữa T1 và T3.

---

## 3. Các phương án đã cân nhắc

### Phương án A — Chỉ kiểm tra ở tầng ứng dụng
```csharp
if (await _db.BookingSlots.AnyAsync(...)) throw new ConflictException();
_db.BookingSlots.Add(slot);
await _db.SaveChangesAsync();
```
| Ưu | Nhược |
|---|---|
| Đơn giản, thông báo lỗi đẹp | ❌ **Không đảm bảo gì cả.** Vẫn trùng ở T1–T4. Đây là bug, không phải giải pháp. |

**→ Loại.** Chỉ giữ lại như lớp trải nghiệm người dùng.

---

### Phương án B — Pessimistic Lock (`SELECT … FOR UPDATE`)
Khoá dòng `court` (hoặc dòng slot) trước khi ghi.

| Ưu | Nhược |
|---|---|
| Kiểm soát rõ ràng, dễ suy luận | Giữ khoá suốt transaction → giảm thông lượng |
| Chặn được cả những ràng buộc phức tạp | Nguy cơ **deadlock** nếu khoá nhiều dòng không theo thứ tự |
| | Nếu khoá ở mức `court` thì hai người đặt **hai giờ khác nhau** của cùng một sân cũng phải xếp hàng — khoá quá thô |

**→ Không chọn.** Tranh chấp ở đây thấp; cái giá về thông lượng và deadlock không đáng.

---

### Phương án C — Optimistic Concurrency (`row_version`)
| Ưu | Nhược |
|---|---|
| Không khoá, hợp mô hình web | ❌ **Không giải quyết được bài toán này.** `row_version` phát hiện xung đột khi **UPDATE** cùng một dòng. Ở đây hai luồng **INSERT hai dòng khác nhau** — không có dòng chung nào để so sánh version. |

**→ Loại cho mục đích chống trùng.** *(Nhưng vẫn dùng `booking.row_version` cho việc **đổi trạng thái** đơn — xem §6.)*

---

### Phương án D — Isolation Level `SERIALIZABLE`
| Ưu | Nhược |
|---|---|
| Đúng về mặt lý thuyết, PostgreSQL tự phát hiện xung đột (SSI) | Chi phí cao hơn hẳn |
| Bắt được cả ràng buộc không diễn đạt được bằng index | Ứng dụng **bắt buộc** phải tự retry khi gặp `40001 serialization_failure` — dễ quên |
| | Áp cho toàn bộ transaction, ảnh hưởng cả những phần không cần |

**→ Không chọn.** Dùng "búa tạ" cho ràng buộc mà một unique index đã diễn đạt trọn vẹn.

---

### Phương án E — Redis Distributed Lock (Redlock)
| Ưu | Nhược |
|---|---|
| Chặn sớm, giảm tải CSDL | ❌ **Không an toàn tuyệt đối**: clock drift, GC pause, mất kết nối đều có thể khiến hai tiến trình cùng tin mình đang giữ khoá |
| | Thêm một điểm hỏng: Redis chết → không đặt được sân |
| | **Hai nguồn sự thật** giữa Redis và PostgreSQL |

**→ Loại.** Có thể cân nhắc như lớp tối ưu về sau, nhưng **không bao giờ** thay thế ràng buộc ở CSDL.

---

### Phương án F — PostgreSQL `EXCLUDE` constraint với `tstzrange`
```sql
ALTER TABLE booking ADD CONSTRAINT no_overlap
  EXCLUDE USING gist (court_id WITH =, tstzrange(start_utc, end_utc) WITH &&)
  WHERE (status IN ('PendingPayment','Confirmed','CheckedIn','Completed'));
```
| Ưu | Nhược |
|---|---|
| Giải pháp tổng quát nhất, chống overlap với **mọi** khoảng thời gian tuỳ ý | EF Core hỗ trợ kém → phải viết raw SQL migration |
| Không cần bảng `booking_slot` | Cần extension `btree_gist`, index GiST nặng hơn B-tree |
| | **Thừa** khi slot đã là giờ chẵn cố định |

**→ Không chọn cho v1**, nhưng ghi lại làm đường lui: nếu sau này hỗ trợ đặt 30/90 phút, đây là phương án thay thế.

---

### ✅ Phương án G — Partial Unique Index trên `booking_slot` *(ĐÃ CHỌN)*

```sql
CREATE UNIQUE INDEX uq_slot_no_double_booking
    ON booking_slot (court_id, slot_start_utc)
    WHERE is_active;
```

mục tiêu không phải là:

Mỗi (court_id, slot_start_utc) chỉ được xuất hiện đúng 1 lần trong toàn bộ bảng. (UNIQUE CONSTRAINT)

Mà là:

Mỗi (court_id, slot_start_utc) chỉ được xuất hiện 1 lần trong các row đang is_active = true. (Partial Unique Index)

Đây chính là lý do dùng Partial Unique Index. Chứ không phải là UNIQUE CONSTRAINT

---

## 4. Quyết định (Decision)

**Áp dụng phòng thủ 3 lớp, với lớp bảo đảm duy nhất nằm ở CSDL.**

| Lớp | Cơ chế | Vai trò | Có phải bảo đảm không? |
|---|---|---|---|
| 1. Giao diện | Ẩn slot đã đặt | Trải nghiệm người dùng | ❌ Không |
| 2. Ứng dụng | `AnyAsync` kiểm tra trước | Trả lỗi 409 đẹp cho ~99% trường hợp | ❌ Không |
| 3. **CSDL** | **Partial unique index** | **Bất biến tuyệt đối** | ✅ **Có** |

Tầng ứng dụng bắt vi phạm và dịch sang lỗi nghiệp vụ:

```csharp
try
{
    _db.Bookings.Add(booking);           // kèm các BookingSlot
    _db.OutboxMessages.Add(evt);         // cùng transaction (Outbox)
    await _db.SaveChangesAsync();
}
catch (DbUpdateException ex)
      when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation } pg
            && pg.ConstraintName == "uq_slot_no_double_booking")
{
    throw new SlotAlreadyBookedException(courtId, slotStart);   // → HTTP 409
}
```

---

## 5. Lý do chọn

1. **Bảo đảm tuyệt đối, không phụ thuộc con người.** Dù có bao nhiêu instance API, bao nhiêu luồng, bao nhiêu bug ở tầng ứng dụng — PostgreSQL vẫn từ chối dòng thứ hai. Bất biến nghiệp vụ quan trọng **phải** được đặt ở tầng dữ liệu.
2. **Không tốn khoá, không deadlock.** Unique index kiểm tra ngay lúc INSERT, không giữ tài nguyên.
3. **`WHERE is_active` giải quyết trọn vẹn bài toán hủy đơn.** Đơn hủy vẫn nằm lại phục vụ audit và báo cáo, nhưng biến mất khỏi index → slot được đặt lại bình thường. Index cũng nhỏ hơn nhiều so với index đầy đủ.
4. **Miễn phí kèm theo một index tra cứu tốt** cho truy vấn lịch trống — cùng một cấu trúc phục vụ hai mục đích.
5. **Đúng mức độ.** Với tranh chấp thấp, đây là giải pháp rẻ nhất đạt được đảm bảo mạnh nhất.

---

## 6. Hệ quả (Consequences)

### ✅ Tích cực
- BR-06 **không thể** bị vi phạm, kể cả khi có bug logic.
- Mở rộng ngang (nhiều instance API) không cần thay đổi gì.
- Kiểm thử dễ: viết test bắn N request song song, khẳng định đúng 1 thành công và N−1 nhận 409.

### ⚠️ Tiêu cực / cần lưu ý
1. **Phi chuẩn hoá bắt buộc.** `booking_slot` phải chứa `court_id` và `is_active` — vì partial index không tham chiếu được bảng khác. Đây là cái giá đã cân nhắc và chấp nhận.
2. **`is_active` phải luôn đồng bộ với `booking.status`**, trong cùng transaction. Rủi ro lệch dữ liệu → **giảm thiểu bằng cách đặt logic trong Domain Model**, không rải ở Handler:
   ```csharp
   public void Cancel(...)
   {
       Status = BookingStatus.Cancelled;
       foreach (var slot in _slots) slot.Release();   // is_active = false
   }
   ```
   Bổ sung một **job đối soát hàng đêm** phát hiện slot lệch trạng thái và cảnh báo.
3. **Lỗi trả về là exception, không phải giá trị.** Bắt buộc phải map `UniqueViolation` sang HTTP 409 ở exception middleware, nếu không khách nhận lỗi 500 khó hiểu.
4. **Không chống được thay đổi lịch phức tạp.** Nếu sau này cho phép đặt 30/90 phút, index này không còn đủ → chuyển sang phương án F (`EXCLUDE` + `tstzrange`). **Đây là điểm gãy đã biết trước của thiết kế.**
5. `booking.row_version` vẫn cần thiết — nhưng cho mục đích **khác**: chống hai người cùng sửa trạng thái một đơn (ví dụ Staff bấm check-in trong khi khách bấm hủy). Đừng nhầm hai cơ chế này.

---

## 7. Kiểm chứng bằng test

ADR không có test đi kèm chỉ là một bài văn. Sprint 3 phải có:

```csharp
[Fact] // BR-06
public async Task Should_AllowOnlyOneBooking_When_20RequestsHitSameSlotConcurrently()
{
    var tasks = Enumerable.Range(0, 20)
        .Select(_ => _client.PostAsJsonAsync("/api/v1/bookings", sameSlotRequest));

    var responses = await Task.WhenAll(tasks);

    responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
    responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(19);
}
```

> ⚠️ Test này **phải chạy trên PostgreSQL thật** (dùng **Testcontainers**), không dùng EF Core InMemory provider — InMemory **không hỗ trợ unique index**, nên test sẽ xanh giả trong khi production vẫn trùng lịch. Đây là cái bẫy khiến rất nhiều đội tin nhầm là mình đã an toàn.

---

## 8. Câu hỏi phỏng vấn liên quan

1. Hai người bấm đặt cùng một sân cùng lúc, hệ thống xử lý thế nào?
2. Vì sao kiểm tra trước khi insert lại không đủ? (TOCTOU)
3. Optimistic vs pessimistic locking — khác nhau ở đâu, bạn chọn cái nào và vì sao?
4. Vì sao dùng **partial** unique index thay vì unique index thường?
5. Nếu chạy 3 instance API thì giải pháp còn đúng không? Vì sao?
6. Redis distributed lock có thay thế được không? Rủi ro là gì?
7. `SERIALIZABLE` giải quyết được không? Cái giá phải trả?
8. Nếu ngày mai khách được đặt 90 phút, thiết kế của bạn hỏng ở đâu và sửa thế nào?
9. Bạn test bài toán concurrency này bằng cách nào?
