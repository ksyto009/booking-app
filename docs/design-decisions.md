# ⭐ Giải thích quyết định thiết kế

> **Đây là file quan trọng nhất trong repo này.**
> Schema thì ai cũng đọc được. Cái phân biệt một kỹ sư với người chép code là **giải thích được vì sao**.
> Mỗi mục dưới đây đều theo cấu trúc: *Quyết định → Vì sao → Phương án khác → Trade-off → Khi nào KHÔNG nên dùng → Câu hỏi phỏng vấn.*

---

## Mục lục

| # | Quyết định | Độ quan trọng khi phỏng vấn |
|---|---|---|
| 1 | Tách `booking_slot` khỏi `booking` — chọn đúng "grain" | 🔥🔥🔥 |
| 2 | Chống double booking bằng **partial unique index** | 🔥🔥🔥🔥🔥 |
| 3 | Cố tình **phi chuẩn hoá** `court_id` + `is_active` | 🔥🔥🔥 |
| 4 | Phân quyền theo **phạm vi dữ liệu**, không chỉ theo vai trò | 🔥🔥🔥🔥 |
| 5 | Multi-tenant **row-level** + Global Query Filter | 🔥🔥🔥🔥 |
| 6 | **Snapshot** giá vào đơn hàng | 🔥🔥🔥 |
| 7 | Redis dùng ở đâu — và **không** dùng ở đâu | 🔥🔥🔥🔥 |
| 8 | **Outbox Pattern** | 🔥🔥🔥🔥 |
| 9 | **Idempotency** cho thanh toán | 🔥🔥🔥🔥 |
| 10 | Khoá chính **UUID v7** | 🔥🔥 |
| 11 | `timestamptz` + UTC | 🔥🔥🔥 |
| 12 | Soft delete **có chọn lọc** | 🔥🔥 |
| 13 | Recurring bằng **rolling window** | 🔥🔥🔥 |

---

## 1. Tách `booking_slot` khỏi `booking` — chọn đúng "grain"

### 📖 Khái niệm mới: Grain (độ mịn)

**Grain** = "một dòng trong bảng này đại diện cho **cái gì**?". Chọn sai grain là sai lầm đắt nhất trong thiết kế CSDL, vì sửa nó nghĩa là viết lại toàn bộ truy vấn.

**Ví dụ đơn giản:** trong hoá đơn siêu thị, grain của bảng `order` là *một lần thanh toán*; grain của `order_item` là *một mặt hàng*. Không ai nhét 10 mặt hàng vào một dòng.

### Quyết định
`booking` = một **lần đặt** của khách. `booking_slot` = một **giờ sân bị chiếm**.
Đặt 19:00–21:00 → **1 dòng** `booking` + **2 dòng** `booking_slot`.

### Vì sao
Vì "tài nguyên bị chiếm" có grain là **giờ**, không phải **đơn hàng**. Khi grain của ràng buộc trùng với grain của bảng, ta có thể diễn đạt ràng buộc bằng **một unique index** — mục 2. Nếu để `booking(start, end)` thì việc chống trùng phải viết bằng logic so sánh khoảng, và **không có cách nào bắt CSDL tự đảm bảo**.

### Phương án khác

| Phương án | Ưu | Nhược |
|---|---|---|
| **A. Chỉ 1 bảng `booking(start_utc, end_utc)`** | Schema gọn nhất | Chống trùng phải tự viết logic overlap. Không ép được bằng constraint đơn giản. Truy vấn "sân nào trống lúc 19h" phải quét khoảng. |
| **B. `booking` + `booking_slot`** ✅ | Ràng buộc trùng lịch **do CSDL đảm bảo**. Truy vấn lịch trống là lookup thẳng. Snapshot giá theo từng giờ (giá 19h ≠ giá 21h). | Nhiều dòng hơn. Phải đồng bộ trạng thái xuống slot. |
| **C. Sinh sẵn bảng `court_availability` mọi slot** | Query trống cực nhanh | ~98k dòng/năm chỉ để lưu "trống". Cần job sinh trước. Sai lệch nếu job chết. |
| **D. PostgreSQL `EXCLUDE` constraint với `tstzrange`** | Rất "đúng bài" về mặt lý thuyết, chống overlap tổng quát | EF Core hỗ trợ kém, phải viết raw SQL migration. Khó debug. Overkill khi slot đã là giờ chẵn. |

### Trade-off
Đổi **một chút dư thừa dữ liệu** lấy **một bất biến được CSDL bảo đảm tuyệt đối**. Với hệ thống booking, đây là món hời.

### ❌ Khi nào KHÔNG nên dùng
Khi thời lượng đặt **liên tục và tuỳ ý** (khách sạn theo đêm lẻ, thuê xe theo phút). Lúc đó số slot bùng nổ → phải quay về phương án D (`EXCLUDE` + `tstzrange`).

### 💬 Câu hỏi phỏng vấn
- *"Vì sao anh tách bảng slot ra thay vì lưu start/end?"*
- *"Nếu sau này khách được đặt 30 phút thì thiết kế của anh có phải sửa không?"* → **Có**, và câu trả lời hay là: đổi grain xuống 30 phút, hoặc chuyển sang `EXCLUDE` constraint. Biết trước điểm gãy của thiết kế mình là dấu hiệu của người có kinh nghiệm.

---

## 2. 🔥 Chống double booking — bài toán lõi

### Quyết định: phòng thủ **3 lớp**

```
Lớp 1 — UX:        Ẩn slot đã đặt trên giao diện        (trải nghiệm, KHÔNG phải bảo đảm)
Lớp 2 — Ứng dụng:  Kiểm tra trùng trong transaction     (thông báo lỗi đẹp cho người dùng)
Lớp 3 — CSDL:      🔒 PARTIAL UNIQUE INDEX              (bảo đảm TUYỆT ĐỐI)
```

```sql
CREATE UNIQUE INDEX uq_slot_no_double_booking
    ON booking_slot (court_id, slot_start_utc)
    WHERE is_active;
```

### 📖 Vì sao lớp 2 **không đủ**

Đây là lỗi Junior kinh điển:

```csharp
// ❌ SAI — có race condition
var daBiDat = await _db.BookingSlots
    .AnyAsync(s => s.CourtId == courtId && s.SlotStartUtc == slot && s.IsActive);

if (daBiDat) throw new ConflictException("Sân đã được đặt");

_db.BookingSlots.Add(newSlot);      // ← giữa AnyAsync và SaveChanges
await _db.SaveChangesAsync();       //   luồng khác đã kịp chèn
```

Khoảng thời gian giữa **kiểm tra** và **ghi** gọi là **TOCTOU** (Time-Of-Check to Time-Of-Use). Với 2 request cách nhau 5ms, cả hai đều đọc thấy "chưa ai đặt", cả hai đều ghi. **Tải thấp không cứu được bạn** — đây là bài toán đúng đắn dữ liệu, không phải hiệu năng.

### ✅ Cách đúng

```csharp
try
{
    _db.BookingSlots.AddRange(slots);
    await _db.SaveChangesAsync();          // CSDL là trọng tài cuối cùng
}
catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                   { SqlState: PostgresErrorCodes.UniqueViolation })
{
    throw new SlotAlreadyBookedException(courtId, slot);   // → HTTP 409 Conflict
}
```

Vẫn giữ kiểm tra ở lớp 2 — **không phải để đảm bảo đúng đắn**, mà để 99% trường hợp trả lỗi đẹp thay vì bắt exception.

### Vì sao dùng **partial** index (`WHERE is_active`)?

Vì đơn đã hủy vẫn phải nằm lại trong bảng (audit, báo cáo, tra cứu). Nếu index full, đơn hủy sẽ chặn người khác đặt lại slot đó. `WHERE is_active` khiến index **chỉ chứa các slot đang thực sự bị chiếm** → vừa đúng nghiệp vụ, vừa nhỏ gọn hơn nhiều.

### Phương án khác

| Phương án | Cơ chế | Ưu | Nhược | Nên dùng khi |
|---|---|---|---|---|
| **Unique index (partial)** ✅ | CSDL từ chối dòng thứ 2 | Đơn giản, tuyệt đối, không tốn khoá | Phải bắt exception | Ràng buộc diễn đạt được bằng khoá |
| **Pessimistic lock** `SELECT … FOR UPDATE` | Khoá dòng sân/slot trước khi ghi | Kiểm soát rõ ràng | Giữ khoá suốt transaction → giảm thông lượng, nguy cơ deadlock | Tranh chấp **rất cao** (bán vé concert) |
| **Optimistic concurrency** (`row_version`) | Phát hiện xung đột lúc ghi | Không khoá, hợp web | Chỉ chống **sửa** đè lên nhau, **không** chống chèn trùng | Cập nhật bản ghi có sẵn |
| **Isolation `SERIALIZABLE`** | Postgres tự phát hiện | Đúng về lý thuyết | Chi phí cao, app phải tự retry khi `40001` | Ràng buộc phức tạp không diễn đạt bằng index |
| **Redis distributed lock** | Khoá ngoài CSDL | Chặn sớm, giảm tải DB | ⚠️ **Không an toàn tuyệt đối** (clock drift, mất kết nối) | Chỉ như tối ưu, **không bao giờ** thay thế constraint |

> ⚠️ Trong dự án này, `row_version` trên bảng `booking` dùng cho việc **sửa** đơn (đổi trạng thái). Nó **không** phải là cơ chế chống double booking. Nhầm hai thứ này là lỗi rất phổ biến.

### 💬 Câu hỏi phỏng vấn — chuẩn bị kỹ, đây là câu ăn điểm nhất
- *"Hai người bấm đặt cùng lúc, hệ thống của anh xử lý sao?"*
- *"Vì sao check trước rồi insert lại không đủ?"*
- *"Optimistic và pessimistic lock khác nhau chỗ nào? Anh chọn cái nào, vì sao?"*
- *"Nếu anh chạy 3 instance API sau load balancer thì giải pháp còn đúng không?"* → **Còn**, vì bảo đảm nằm ở CSDL dùng chung. Đây chính là điểm mạnh của việc đặt bất biến ở tầng dữ liệu.
- *"Dùng Redis lock được không?"* → Được, nhưng **chỉ như lớp tối ưu**. Redis lock có thể mất trong sự cố mạng; unique index thì không.

---

## 3. Cố tình **phi chuẩn hoá** `court_id` và `is_active`

### Quyết định
`booking_slot` chứa `court_id` (đã có ở `booking`) và `is_active` (suy ra được từ `booking.status`). Đây là vi phạm chuẩn 3NF **có chủ đích**.

### Vì sao
Vì **partial unique index không thể tham chiếu bảng khác**. Muốn CSDL tự bảo đảm BR-06, cả hai cột phải nằm ngay trên `booking_slot`.

Đây là ví dụ hoàn hảo của nguyên tắc: **chuẩn hoá là mặc định, phi chuẩn hoá là quyết định có lý do được ghi lại.**

### Cái giá phải trả
`booking_slot.is_active` phải được cập nhật **trong cùng transaction** với `booking.status`. Nếu quên → dữ liệu lệch, slot bị "ma ám" (không ai đặt được nhưng cũng không có đơn nào).

**Cách phòng:** đặt logic này trong **Domain Model**, không rải rác ở Handler.

```csharp
public void Cancel(string reason, DateTimeOffset now, Guid actorId)
{
    if (Status is not (BookingStatus.PendingPayment or BookingStatus.Confirmed))
        throw new DomainException("Chỉ hủy được đơn chờ thanh toán hoặc đã xác nhận"); // BR-17

    Status = BookingStatus.Cancelled;
    CancelledAt = now;
    CancellationReason = reason;

    foreach (var slot in _slots) slot.Release();   // ⬅️ không thể quên
}
```

### 💬 Câu hỏi phỏng vấn
- *"Vì sao anh lặp `court_id` ở hai bảng? Thế không vi phạm chuẩn hoá à?"*
- *"Làm sao đảm bảo hai cột đó không bị lệch?"*

---

## 4. 🔥 Phân quyền theo **phạm vi dữ liệu**, không chỉ theo vai trò

### 📖 Khái niệm: RBAC vs Data-scoped Authorization

- **RBAC (Role-Based)** trả lời: *"vai trò này được làm hành động gì?"* → `[Authorize(Roles = "Manager")]`
- **Data-scoped** trả lời: *"trên **những bản ghi nào**?"*

### Vì sao dự án này bắt buộc phải có cả hai
Nhớ lại lời anh Dũng:

> *"Cụm C anh hợp tác với thằng bạn, nó góp vốn 50%. **Nó chỉ được coi doanh thu cụm C thôi, đừng cho nó thấy cụm A với B.**"*

`[Authorize(Roles = "Partner")]` cho phép ông ấy gọi API báo cáo — nhưng **không ngăn** ông ấy đổi `branchId` trên URL để xem cụm A. Đây là lỗ hổng **IDOR** (Insecure Direct Object Reference), nằm trong OWASP Top 10 (Broken Access Control — hạng **#1**).

### Cách triển khai

```csharp
// Tầng hạ tầng cung cấp phạm vi, lấy từ claim trong JWT
public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    TenantRole Role { get; }
    IReadOnlySet<Guid> BranchScope { get; }   // rỗng ⇒ Owner ⇒ toàn tenant
}

// Query handler bắt buộc lọc theo phạm vi
var query = _db.Bookings.AsNoTracking();

if (_currentUser.BranchScope.Count > 0)                       // BR-29
    query = query.Where(b => _currentUser.BranchScope.Contains(b.BranchId));
```

### ⚠️ Lỗi thường gặp
1. **Lọc ở tầng UI thay vì tầng dữ liệu.** Ẩn nút bấm không phải là phân quyền.
2. **Tin vào `branchId` client gửi lên** mà không đối chiếu với phạm vi của token.
3. **Đưa danh sách branch vào JWT rồi không bao giờ làm mới.** Thu hồi quyền không có tác dụng cho đến khi token hết hạn → phải để access token ngắn (~15 phút).

### 💬 Câu hỏi phỏng vấn
- *"Phân biệt authentication và authorization."*
- *"RBAC có đủ cho hệ thống của anh không? Vì sao không?"*
- *"IDOR là gì? Anh chống bằng cách nào?"*
- *"Thu hồi quyền của một người thì bao lâu mới có hiệu lực?"*

---

## 5. Multi-tenant kiểu **row-level** + Global Query Filter

### 📖 Ba kiểu multi-tenancy

| Kiểu | Cách ly | Chi phí | Phù hợp |
|---|---|---|---|
| **Database riêng mỗi tenant** | Cao nhất | Rất đắt (N kết nối, N migration) | Khách hàng doanh nghiệp lớn, yêu cầu pháp lý |
| **Schema riêng mỗi tenant** | Trung bình | Migration nhân lên theo tenant | Vài chục tenant |
| **Chung bảng, cột `tenant_id`** ✅ | Thấp nhất | Rẻ nhất, 1 migration | Hàng nghìn tenant nhỏ |

### Quyết định: chung bảng + `tenant_id`
Vì tenant ở đây là **chủ sân nhỏ** (anh Dũng, ông bạn Bình Dương), dữ liệu ít, không có yêu cầu pháp lý về cách ly.

### Rủi ro lớn nhất và cách chặn
Rủi ro: **quên `WHERE tenant_id = ...` một lần duy nhất** → rò rỉ dữ liệu giữa các khách hàng. Đây là sự cố có thể giết một công ty SaaS.

**Không được phụ thuộc vào việc lập trình viên nhớ.** Ép bằng EF Core Global Query Filter:

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Booking>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
    b.Entity<Court>()  .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId
                                         && x.DeletedAt == null);
    // ... áp cho MỌI entity có tenant_id
}
```

### ⚠️ Cạm bẫy của Global Query Filter — phải biết
1. `IgnoreQueryFilters()` vô hiệu hoá **toàn bộ** filter, kể cả soft delete. Dùng rất cẩn thận.
2. Filter **không tự áp cho `Insert`**. Phải tự gán `TenantId` trong `SaveChanges` override.
3. Background job (Hangfire) **không có HTTP context** → `TenantContext` rỗng → filter thành `TenantId == Guid.Empty` → không trả về gì. Phải thiết lập tenant thủ công trong job.
4. Filter không áp cho raw SQL.

### 💬 Câu hỏi phỏng vấn
- *"Có mấy cách làm multi-tenant? Anh chọn cách nào, vì sao?"*
- *"Làm sao đảm bảo không rò rỉ dữ liệu giữa tenant?"*
- *"Global Query Filter có nhược điểm gì?"*
- *"Khi nào anh sẽ chuyển sang database riêng cho mỗi tenant?"* → khi có khách yêu cầu cách ly vật lý, hoặc một tenant lớn tới mức ảnh hưởng hiệu năng của phần còn lại.

---

## 6. **Snapshot** giá vào đơn hàng

### Quyết định
`booking_slot.unit_price` lưu **giá tại thời điểm đặt**, không join sang `price_rule` khi hiển thị.

### Vì sao
Khách đặt sân tháng 8 với giá 120k. Tháng 9 anh Dũng tăng giá lên 150k. Nếu báo cáo join sang bảng giá hiện tại → **doanh thu quá khứ tự động thay đổi**. Sổ sách sai, đối soát sai, khách khiếu nại đúng.

### 📖 Nguyên tắc tổng quát
> **Dữ liệu giao dịch phải bất biến. Chỉ dữ liệu danh mục mới được thay đổi.**

Cùng nguyên tắc này áp cho: tên khách trên hoá đơn, địa chỉ giao hàng, thuế suất, tỉ giá.

### ⚠️ Lỗi thường gặp
Junior hay chuẩn hoá quá tay: "giá đã có ở `price_rule` rồi, lưu lại làm gì cho dư thừa?". Đây **không phải** dư thừa — đó là **hai sự thật khác nhau**: "giá hiện tại" và "giá đã bán".

### 💬 Câu hỏi phỏng vấn
- *"Nếu đổi bảng giá thì đơn cũ có bị ảnh hưởng không? Anh xử lý thế nào?"*

---

## 7. 🔥 Redis dùng ở đâu — và **không** dùng ở đâu

Đây là mục tôi muốn bạn đọc kỹ nhất, vì nó dạy cách **chọn công nghệ theo bài toán** thay vì nhồi cho đủ bộ.

### ❌ Redis **KHÔNG** dùng để giữ chỗ tạm

Rất nhiều hướng dẫn trên mạng bảo: "dùng Redis với TTL 10 phút để hold slot". **Trong thiết kế này, việc đó là thừa và có hại.**

Vì sao? Vì booking ở trạng thái `PendingPayment` **đã** chiếm slot trong CSDL (BR-07), được unique index bảo vệ. Thêm Redis vào chỉ tạo ra **hai nguồn sự thật**:

| Rủi ro | Hậu quả |
|---|---|
| Redis nói "trống", Postgres nói "đã đặt" | Khách bấm đặt rồi báo lỗi — trải nghiệm tệ |
| Redis nói "đã giữ", Postgres không có gì | Slot bị khoá ma, không ai đặt được cho đến khi TTL hết |
| Redis chết | Toàn bộ luồng đặt sân sập, dù CSDL vẫn khoẻ |

**Hết hạn giữ chỗ dùng gì?** → Hangfire job quét `booking(status='PendingPayment' AND hold_expires_at < now())`. Có **partial index** phục vụ đúng truy vấn này, chỉ vài chục dòng, chạy mỗi phút — chi phí gần bằng không.

> **Bài học:** thêm một thành phần hạ tầng phải trả giá bằng một **chế độ hỏng hóc mới**. Chỉ thêm khi lợi ích lớn hơn cái giá đó.

### ✅ Redis dùng ở đâu — nơi nó thực sự thắng

| Việc | Vì sao hợp | Cấu hình |
|---|---|---|
| **Cache lịch trống theo ngày** | Đọc gấp 20–50 lần ghi (NFR). Đây là hot path thật sự. | Key `avail:{branchId}:{date}`, TTL 60s, **xoá key khi có booking mới/hủy** |
| **Rate limiting** | Cần bộ đếm nguyên tử, chia sẻ giữa nhiều instance | Sliding window theo IP + theo user |
| **Chống spam OTP** | Đếm số lần gửi OTP theo SĐT | Key có TTL |
| **Danh sách đen refresh token** | Tra cứu nhanh khi thu hồi | TTL = hạn của token |

### 📖 Khái niệm: Cache Invalidation

Cách xoá cache khi dữ liệu đổi. Có 3 chiến lược:

| Chiến lược | Cách làm | Ưu | Nhược |
|---|---|---|---|
| **TTL** (hết hạn theo thời gian) | Đặt 60s rồi kệ | Đơn giản nhất | Dữ liệu cũ tới 60s |
| **Write-through / xoá chủ động** ✅ | Ghi DB xong thì `DEL` key | Luôn tươi | Phải nhớ xoá ở **mọi** chỗ ghi |
| **Event-driven** | Domain event → handler xoá cache | Sạch, tập trung | Cần hạ tầng event |

**Dự án này dùng TTL ngắn (60s) + xoá chủ động qua domain event.** Kết hợp cả hai: xoá chủ động cho độ tươi, TTL làm lưới an toàn nếu xoá sót.

### 💬 Câu hỏi phỏng vấn
- *"Anh dùng Redis vào việc gì?"* → **đừng** trả lời "để cache cho nhanh". Trả lời cụ thể hot path nào, tỉ lệ đọc/ghi bao nhiêu.
- *"Cache invalidation anh làm sao?"*
- *"Nếu Redis chết thì hệ thống còn chạy được không?"* → **Phải chạy được.** Cache miss thì đọc thẳng CSDL. Nếu Redis chết mà hệ thống sập thì Redis đã không còn là cache, nó thành single point of failure.
- *"Vì sao anh không dùng Redis để hold slot?"* ← câu này trả lời tốt sẽ gây ấn tượng rất mạnh.

---

## 8. 🔥 Outbox Pattern

### 📖 Vấn đề: **Dual Write**

Khi xác nhận booking, ta cần làm 2 việc:
1. Ghi `booking.status = Confirmed` vào PostgreSQL
2. Gửi message `BookingConfirmed` sang RabbitMQ (để gửi SMS, cộng điểm, ghi audit)

```csharp
// ❌ SAI
await _db.SaveChangesAsync();              // ✅ thành công
await _bus.PublishAsync(new BookingConfirmed(...));  // ❌ RabbitMQ chết
```

Kết quả: **đơn đã xác nhận nhưng khách không bao giờ nhận được tin nhắn.** Ngược lại, publish trước rồi DB lỗi → khách nhận SMS cho đơn không tồn tại.

Đây là **dual write problem**: không thể có transaction trải qua hai hệ thống khác nhau.

### Giải pháp: Outbox

Ghi message vào **bảng `outbox_message` trong cùng transaction** với booking:

```csharp
_db.Bookings.Update(booking);
_db.OutboxMessages.Add(OutboxMessage.From(new BookingConfirmed(booking.Id)));
await _db.SaveChangesAsync();     // ⬅️ MỘT transaction, hoặc cả hai cùng thành công, hoặc cả hai cùng thất bại
```

Một background worker đọc bảng outbox, đẩy sang RabbitMQ, đánh dấu `processed_at`.

```
[Transaction]                        [Worker riêng]
booking ──┐                          outbox ──► RabbitMQ ──► SMS/Audit/Point
outbox  ──┘ cùng commit                  │
                                      đánh dấu processed_at
```

### Trade-off
| Ưu | Nhược |
|---|---|
| **Không bao giờ mất event** | Độ trễ thêm vài giây |
| Không cần distributed transaction (2PC) | Thêm bảng + worker |
| Có thể retry an toàn | Message có thể gửi **>1 lần** nếu worker chết sau khi publish trước khi đánh dấu |

### 📖 Hệ quả quan trọng: **At-least-once delivery**
Vì có thể gửi trùng, **mọi consumer phải idempotent** — xử lý cùng một message 2 lần phải cho kết quả giống hệt 1 lần. Ví dụ: trước khi gửi SMS, kiểm tra đã gửi cho `bookingId` này chưa.

### 💬 Câu hỏi phỏng vấn
- *"Làm sao đảm bảo ghi DB và gửi message cùng thành công?"*
- *"Outbox pattern là gì? Nhược điểm?"*
- *"At-least-once và exactly-once khác nhau thế nào?"* → exactly-once trong hệ phân tán gần như là ảo tưởng; thực tế là **at-least-once + consumer idempotent**.

---

## 9. 🔥 Idempotency cho thanh toán

### 📖 Khái niệm
**Idempotent** = thực hiện N lần cho kết quả giống hệt 1 lần.

**Ví dụ đơn giản:** `SET x = 5` idempotent. `x = x + 1` **không** idempotent.

### Vì sao thanh toán bắt buộc phải có
Cổng thanh toán (VNPay/MoMo) **sẽ** gửi webhook trùng lặp — đó là thiết kế cố ý của họ (họ retry cho tới khi bạn trả 200). Nếu không chống trùng:
- Cùng một giao dịch được ghi nhận 2 lần
- Đơn hàng cộng tiền 2 lần
- Đối soát cuối tháng lệch

### Cách làm trong dự án

**Chiều đi (client → hệ thống):**
```
POST /api/v1/bookings
Idempotency-Key: 8f3a...   ← client sinh, gửi lại y hệt khi retry
```
→ `payment.idempotency_key` có `UNIQUE` constraint. Request thứ hai bị CSDL từ chối → trả về kết quả của lần đầu, **không** tạo đơn mới.

**Chiều về (cổng thanh toán → hệ thống):**
```sql
CONSTRAINT uq_webhook_event UNIQUE (provider, event_id)
```
→ Webhook trùng bị chặn ngay lúc INSERT. Xử lý xong mới đặt `processed_at`.

**Quy trình xử lý webhook đúng chuẩn:**
1. Xác thực **chữ ký** (không bao giờ tin payload chưa xác thực)
2. `INSERT` vào `payment_webhook_event` → trùng thì trả `200 OK` và dừng
3. Xử lý nghiệp vụ trong transaction
4. Đánh dấu `processed_at`
5. Trả `200 OK` **kể cả khi nghiệp vụ lỗi** — nếu trả 500, cổng sẽ retry mãi. Lỗi thì ghi vào cột `error` và xử lý bằng job riêng.

### ⚠️ Lỗi thường gặp
- Tin webhook mà không xác thực chữ ký → **kẻ tấn công tự gửi webhook "đã thanh toán"**. Đây là lỗ hổng nghiêm trọng và rất phổ biến.
- Dùng `bookingId` làm idempotency key → sai, vì một đơn có thể thanh toán lại sau khi thất bại.
- Trả 500 khi nghiệp vụ lỗi → bão retry.

### 💬 Câu hỏi phỏng vấn
- *"Idempotency là gì? Vì sao API thanh toán cần nó?"*
- *"Webhook bị gửi trùng thì sao?"*
- *"Làm sao biết webhook thật sự đến từ VNPay?"*

---

## 10. Khoá chính: **UUID v7**

| Phương án | Ưu | Nhược |
|---|---|---|
| `bigserial` (auto increment) | Nhỏ (8 byte), index rất tốt | **Lộ số lượng đơn hàng** (`/bookings/1042` → biết bạn có 1042 đơn). Khó merge khi tách microservice. Không sinh được ID ở client. |
| `uuid v4` (ngẫu nhiên) | Sinh ở bất cứ đâu, không lộ gì | **Ngẫu nhiên hoàn toàn ⇒ index B-tree phân mảnh nặng**, mỗi insert rơi vào một trang ngẫu nhiên |
| **`uuid v7`** ✅ | Sinh bất cứ đâu + **sắp theo thời gian** ⇒ insert tuần tự, index không phân mảnh | 16 byte thay vì 8 |

.NET 9 có sẵn: `Guid.CreateVersion7()`.

> **Lưu ý:** ID kỹ thuật (`uuid`) khác với **mã nghiệp vụ** (`booking_code = "BK-2607-0001"`). Khách gọi điện đọc mã nghiệp vụ, không ai đọc UUID qua điện thoại. Hai thứ này phục vụ hai mục đích khác nhau — đừng gộp.

### 💬 Câu hỏi phỏng vấn
- *"Vì sao dùng UUID thay vì auto-increment?"*
- *"UUID v4 ảnh hưởng gì tới hiệu năng index?"*

---

## 11. `timestamptz` + luôn lưu UTC

### Quyết định
Mọi cột thời điểm dùng `timestamptz`, lưu UTC. Chuyển sang giờ Việt Nam **chỉ ở tầng hiển thị**. `branch.time_zone` lưu múi giờ vận hành.

### Vì sao
- `timestamp` (không `tz`) là "thời điểm không xác định" — hai server khác múi giờ đọc ra hai kết quả khác nhau.
- Slot 19:00 giờ VN = 12:00 UTC. Unique index trên UTC vẫn đúng tuyệt đối.
- Nếu sau này có chi nhánh ở nước khác, schema không phải đổi.

### ⚠️ Cạm bẫy
- `DateTime` trong C# có `Kind` (`Utc`/`Local`/`Unspecified`) — nguồn gốc của vô số bug. **Dùng `DateTimeOffset`** hoặc `DateTime` với `Kind = Utc` nhất quán.
- Đừng dùng `DateTime.Now` — dùng `DateTime.UtcNow`, và tốt hơn nữa là inject `TimeProvider` (có sẵn từ .NET 8) để **test được**.

```csharp
// ❌ không test được
if (booking.StartUtc < DateTime.UtcNow) ...

// ✅ test được
if (booking.StartUtc < _timeProvider.GetUtcNow()) ...
```

### 💬 Câu hỏi phỏng vấn
- *"Anh lưu thời gian kiểu gì? Vì sao không lưu giờ địa phương?"*
- *"Làm sao unit test logic phụ thuộc thời gian hiện tại?"*

---

## 12. Soft delete **có chọn lọc**

### Quyết định

| Loại bảng | Chiến lược | Vì sao |
|---|---|---|
| Master data (`branch`, `court`, `price_rule`) | ✅ Soft delete (`deleted_at`) | Bị booking cũ tham chiếu. Xoá cứng → báo cáo lịch sử vỡ. |
| Dữ liệu giao dịch (`booking`, `payment`) | ❌ **Không** soft delete | Đã có `status` (`Cancelled`). Thêm `deleted_at` là **hai cách diễn đạt cùng một thứ** → mâu thuẫn. |
| Log (`audit_log`, `outbox_message`) | ❌ Không xoá, dọn theo tuổi | Chỉ archive/purge sau 3 năm |

### ⚠️ Lỗi thường gặp
- Bôi soft delete lên **mọi** bảng theo phản xạ. Kết quả: `WHERE deleted_at IS NULL` rải khắp nơi, quên một chỗ là lộ dữ liệu đã xoá.
- Quên rằng **unique index phải là partial**: `UNIQUE(branch_id, code) WHERE deleted_at IS NULL`. Nếu không, xoá mềm sân "S1" rồi tạo lại "S1" sẽ bị chặn.

### 💬 Câu hỏi phỏng vấn
- *"Soft delete có nhược điểm gì?"*
- *"Soft delete ảnh hưởng thế nào tới unique constraint và foreign key?"*

---

## 13. Recurring booking bằng **rolling window**

### Vấn đề
Anh Dũng có nhóm thuê "tối thứ 3 hàng tuần, vô thời hạn". Sinh bao nhiêu booking?

| Phương án | Vấn đề |
|---|---|
| Sinh hết tới vô hạn | Không thể — vô hạn dòng |
| Không sinh gì, tính lúc truy vấn | Không giữ được sân (người khác đặt mất), không thu tiền được |
| **Rolling window 8 tuần** ✅ | Job hàng tuần sinh thêm, `generated_until` đánh dấu đã sinh tới đâu |

### Vì sao 8 tuần
Cân bằng giữa: đủ xa để khách yên tâm giữ sân, đủ gần để không khoá sân quá lâu khi khách bỏ ngang. **Đây là tham số cấu hình**, không hardcode.

### Điểm khó nhất: **xử lý xung đột** (BR-25)
Khi sinh booking cho tuần thứ 8, slot đó có thể đã bị người khác đặt.

❌ **Sai:** rollback cả series → khách mất toàn bộ lịch vì một buổi trùng.
✅ **Đúng:** bỏ qua buổi đó, ghi log, thông báo, các buổi khác vẫn sinh bình thường.

> **Nguyên tắc tổng quát:** trong xử lý theo lô, **lỗi cục bộ không được làm hỏng toàn cục**. Đây là tư duy phân biệt người viết batch job có kinh nghiệm.

### 💬 Câu hỏi phỏng vấn
- *"Đặt lịch định kỳ vô thời hạn thì anh lưu thế nào?"*
- *"Một buổi trong chuỗi bị trùng thì xử lý ra sao?"*
- *"Job sinh booking chạy hai lần cùng lúc thì sao?"* → cần **idempotent** (dựa vào `generated_until` + unique index) hoặc distributed lock của Hangfire.

---

## 📌 Tổng kết — 5 nguyên tắc xuyên suốt

1. **Bất biến nghiệp vụ quan trọng phải được CSDL bảo đảm**, không phụ thuộc lập trình viên nhớ. *(mục 2, 5)*
2. **Dữ liệu giao dịch là bất biến**; chỉ danh mục mới thay đổi. *(mục 6)*
3. **Thêm hạ tầng = thêm chế độ hỏng hóc.** Chỉ thêm khi lợi ích vượt cái giá đó. *(mục 7)*
4. **Trong hệ phân tán, hãy thiết kế cho at-least-once + idempotent**, đừng mơ exactly-once. *(mục 8, 9)*
5. **Biết trước điểm gãy của thiết kế mình.** Nói được "thiết kế này sẽ hỏng khi X xảy ra" giá trị hơn nhiều so với "thiết kế này hoàn hảo". *(mục 1)*
