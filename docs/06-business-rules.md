# 06 — Quy tắc nghiệp vụ (Business Rules)

> **Quy tắc bắt buộc:** mỗi `BR-xx` phải **kiểm chứng được** và có **ít nhất một test** trích mã rule trong tên test.
> Ví dụ: `Booking_ShouldRejectOverlappingSlot_BR06()`

---

## 1. Đặt sân

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-01** | Đơn vị đặt là **slot 30 phút**, bắt đầu đúng mốc `:00` hoặc `:30`. Thời lượng đặt luôn là bội số của 30 phút (30′, 60′, 90′, 120′…). *(sửa theo CR-07)* | Domain: `TimeSlot` value object |
| **BR-02** | Một booking gồm 1..N slot **liên tiếp** trên **cùng một sân**. Không cho đặt slot rời rạc trong một đơn. | Domain: `Booking.Create()` |
| **BR-03** | Chỉ đặt được trong giờ mở cửa của chi nhánh (mặc định 05:00–23:00 giờ VN). | Application: validator |
| **BR-04** | Không đặt được slot quá khứ. Đơn **online** phải đặt trước ít nhất **30 phút**; đơn tại quầy không bị ràng buộc này. | Application |
| **BR-05** | Không đặt được xa quá **60 ngày** kể từ hôm nay. | Application |
| **BR-06** | 🔒 **Một sân + một khung giờ ⇒ tối đa MỘT booking đang hiệu lực.** | **CSDL: partial unique index** |
| **BR-07** | Booking `PendingPayment` **vẫn chiếm slot**. | `booking_slot.is_active = true` |
| **BR-08** | Không đặt được sân `Maintenance` hoặc rơi vào khoảng `court_closure`. | Application |
| **BR-09** | Khách **tự chọn sân cụ thể** (v1). "Sân nào cũng được" là Could-have. | UI + API |
| **BR-33** | **Thời lượng tối thiểu theo khung giờ:** cao điểm **60 phút**, thấp điểm **30 phút**. Tối đa **240 phút** mọi khung giờ. Ngưỡng cấu hình trên `price_rule`. *(CR-07)* | Application: validator |

> ⭐ **BR-06 là bất biến quan trọng nhất của toàn hệ thống.** Vi phạm nó gây hậu quả ngoài đời thật: hai nhóm khách cùng đến sân. Đây là lý do khách hàng bỏ tiền làm hệ thống.

## 2. Thanh toán

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-10** | Đơn **online** mặc định **trả trước 100%** trước khi được xác nhận. | Domain |
| **BR-11** | `PendingPayment` quá **10 phút** chưa thanh toán → `Expired`, **giải phóng slot**. | Hangfire job |
| **BR-12** | Khách có cờ **`CanPayAtCounter`** được đặt online với `PayAtCounter`, xác nhận ngay không cần trả trước. *(tách khỏi `IsTrusted` cũ theo CR-08a)* | Domain |
| **BR-13** | Đơn tạo tại quầy bởi Staff mặc định `PayAtCounter` và `Confirmed` ngay. | Domain |
| **BR-14** | Giá được **chốt cứng (snapshot)** vào từng slot tại thời điểm đặt. Đổi bảng giá sau đó **không** ảnh hưởng đơn cũ. | `booking_slot.unit_price` |
| **BR-14b** | Giá một slot 30 phút = **50%** giá giờ. Tỉ lệ này cấu hình theo tenant, **không hardcode**. *(CR-07)* | `tenant.half_hour_price_ratio` |
| **BR-15** | Mọi giao dịch thanh toán phải có `idempotency_key`. Webhook trùng không được ghi nhận 2 lần. | CSDL: unique constraint |

> 💡 **BR-12 là lời giải cho xung đột nghiệp vụ:** Chủ sân muốn "thu tiền trước để hết no-show" nhưng cũng nói "bắt khách ruột chuyển khoản trước thì kỳ lắm". Cơ chế `CanPayAtCounter` + thu hồi tự động (BR-22) hoà giải hai mong muốn mâu thuẫn này.
>
> ⚠️ **Vì sao tách `IsTrusted` thành hai cờ (CR-08a):** cờ cũ gộp hai đặc quyền không liên quan — *được nợ tiền* và *được hủy linh hoạt*. Gộp lại thì khách no-show 2 lần sẽ mất luôn quyền hủy, dù hai chuyện chẳng dính gì tới nhau. Tách ra: `CanPayAtCounter` (BR-12, thu hồi bởi BR-22) và `CanCancelLate` (BR-35, ngưỡng thu hồi riêng).

## 3. Hủy & Hoàn tiền

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-16** | Mức hoàn theo thời điểm hủy: **≥24h → 100%** · **4–24h → 50%** · **<4h → 0%** | Domain Service: `RefundPolicy.Calculate()` — thuần tính toán, unit test được |
| **BR-17** | Chỉ hủy được khi trạng thái ∈ {`PendingPayment`, `Confirmed`}. | Domain: guard trong `Booking.Cancel()` |
| **BR-18** | Nếu **phía sân** hủy (sân hỏng, mưa, sự cố) → hoàn **100%** bất kể thời điểm, bắt buộc ghi lý do. | Domain: `Booking.CancelByVenue()` — **method riêng**, không phải cờ boolean |
| **BR-19** | Hoàn tiền là quy trình **bất đồng bộ** (`Pending → Succeeded / Failed`). Đơn hủy ngay, tiền về sau. | Worker: `RefundProcessor` (Hangfire) + bảng `refund` |
| **BR-34** | Khi hủy, khách **tự chọn**: **hoàn tiền** *(theo BR-16)* hoặc **dời lịch** *(theo BR-36)*. Hệ thống xử lý **tự động, không cần ai duyệt**. *(CR-08a)* | Api: hai endpoint riêng `/cancel` và `/reschedule` |
| **BR-35** | **Mọi khách đều được hủy** theo bậc BR-16. Khách có cờ **`CanCancelLate`** hưởng **ưu đãi thêm**: cửa sổ dời lịch rộng gấp đôi và bậc hoàn tiền tốt hơn. Đây là **ưu đãi**, không phải điều kiện tiên quyết. *(CR-08a)* | Domain Service: `RefundPolicy` đọc `CustomerProfile.CanCancelLate` |
| **BR-36** | Cửa sổ cho phép **dời lịch** = `N` giờ trước giờ chơi. `N` cấu hình **theo tenant**, mặc định **2 giờ**, không hardcode. *(CR-08a)* | Domain: guard trong `Booking.Reschedule()`, ngưỡng truyền từ `tenant.reschedule_window_hours` |
| **BR-37** | 🔒 **Dời lịch phải NGUYÊN TỬ:** chiếm slot mới và giải phóng slot cũ trong **cùng một transaction**. Nếu slot mới đã bị chiếm → rollback toàn bộ, **đơn cũ không bị đụng tới**. Tuyệt đối **không** triển khai bằng "hủy rồi đặt lại". *(CR-08b)* | 🔒 **CSDL: `uq_slot_no_double_booking` + một transaction duy nhất** |
| **BR-38** | Mỗi đơn được dời tối đa **2 lần** (cấu hình theo tenant). Slot mới giá **cao hơn** → khách bù thêm trước khi xác nhận; giá **thấp hơn** → **không** hoàn chênh lệch. Slot mới cũng phải cách hiện tại ≥ `N` giờ. *(CR-08b)* | Domain: `Booking.Reschedule()` + `CHECK` trên `booking.reschedule_count` |
| **BR-39** | Dời lịch sang **ngày khác** ✅ và **sân khác** ✅ được, **chi nhánh khác ❌** (v1). Đơn thuộc chuỗi định kỳ dời được **một buổi**, không ảnh hưởng chuỗi *(nhất quán BR-26)*. | Application: `RescheduleCommandValidator` |
| **BR-40** | 🔑 **`BranchManager` được quyền GHI ĐÈ** kết quả hoàn tiền trong phạm vi chi nhánh mình: hoàn nhiều hơn bậc, hoàn ít hơn, hoặc từ chối hoàn. **Bắt buộc nhập lý do** và ghi **audit log** *(BR-32)*. Mức hoàn hợp lệ ∈ `[0, số tiền đã trả]`. *(CR-08a)* | Api: authorization policy + phạm vi chi nhánh · Domain: `Booking.OverrideRefund()` **bắt buộc tham số `reason`** |
| **BR-41** | Ghi đè là **ngoại lệ, không phải quy trình**. Mặc định luôn là mức tự động theo BR-16 — quản lý **không phải duyệt từng đơn**. | ⚠️ **Không kiểm chứng trực tiếp được** — đây là *nguyên tắc thiết kế*, không phải rule. Kiểm gián tiếp: luồng `/cancel` **không** gọi tới `OverrideRefund` ở bất kỳ nhánh nào |

## 4. No-show & Check-in

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-20** | Quá giờ bắt đầu **15 phút** chưa check-in → Staff được quyền đánh dấu `NoShow`. | Domain: guard trong `Booking.MarkNoShow(now)` — `now` inject qua `TimeProvider` |
| **BR-21** | `NoShow` **không hoàn tiền**. | Domain: `Booking.MarkNoShow()` đặt `refund = Money.Zero` |
| **BR-22** | Khách bị `NoShow` **2 lần trong 90 ngày** → **tự động** mất cờ **`CanPayAtCounter`**. ⚠️ **Không** đụng tới `CanCancelLate` — hai đặc quyền độc lập, hai lý do thu hồi độc lập. *(sửa theo CR-08a)* | Domain: `CustomerProfile.RecordNoShow(now)` — kích hoạt bởi domain event `NoShowRecorded` |
| **BR-42** | **Dời lịch KHÔNG tính vào thống kê no-show.** Dời là hành vi tốt — khách báo trước. Phạt nó là khuyến khích no-show. *(CR-08b)* | Domain: `Booking.Reschedule()` **không chạm** `no_show_count`. Cần **test hồi quy** vì đây là rule dạng "không được làm gì" — loại rule dễ bị vi phạm nhất khi refactor |

## 5. Đặt định kỳ (Recurring)

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-23** | Series định nghĩa bởi: sân + thứ trong tuần + giờ bắt đầu + số giờ + ngày bắt đầu + ngày kết thúc. Giảm giá mặc định **15%**. | Domain: constructor `RecurringSeries` |
| **BR-24** | Hệ thống **sinh trước** booking con trong cửa sổ **8 tuần** (rolling window), job chạy hàng tuần. **Không sinh vô hạn.** | Worker: `GenerateRecurringBookingsJob` + `series.generated_until` *(chỉ tiến, không lùi)* |
| **BR-25** | Buổi bị trùng lịch → **bỏ qua buổi đó**, ghi log, thông báo. **Không làm hỏng cả series.** | Application: job xử lý **từng buổi một transaction riêng**, bắt `UniqueViolation` cục bộ — **không** gói cả lô vào một transaction |
| **BR-26** | Hủy **một buổi** không hủy series. Hủy **series** chỉ ảnh hưởng buổi **tương lai**. | Domain: `Booking` và `RecurringSeries` là **hai aggregate riêng**, FK **không** cascade delete |
| **BR-27** | v1: thanh toán **theo từng buổi**, không thu tiền cả tháng một lần. | Domain: job sinh booking với `payment_mode = PayAtCounter` |

> ⚠️ **BR-25 thể hiện một nguyên tắc tổng quát:** trong xử lý theo lô, **lỗi cục bộ không được làm hỏng toàn cục**.

## 6. Phân quyền & Đa chủ sở hữu

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-28** | Mọi bản ghi nghiệp vụ thuộc về **đúng một tenant**. Truy vấn chéo tenant bị chặn ở **tầng hạ tầng**, không phụ thuộc lập trình viên nhớ. | 🔒 **Infrastructure: EF Core Global Query Filter** (đọc) + `SaveChanges` override tự gán `TenantId` (ghi) |
| **BR-29** | `BranchManager` và `Partner` chỉ thấy dữ liệu các **chi nhánh được cấp phạm vi**. | Application: mọi query handler lọc theo `ICurrentUser.BranchScope` — **không tin `branchId` client gửi lên** |
| **BR-30** | `Partner` là quyền **chỉ đọc báo cáo**. Không tạo/sửa/hủy bất kỳ dữ liệu nào. | Api: authorization policy — vai trò `Partner` chỉ được gọi endpoint `GET /reports/*` |

## 7. Dữ liệu & Vận hành

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-31** | Master data (`branch`, `court`, `price_rule`) dùng **soft delete**. Dữ liệu giao dịch (`booking`, `payment`) **KHÔNG** soft delete — chúng đã có trạng thái riêng. | CSDL: partial unique index `WHERE deleted_at IS NULL` + Global Query Filter |
| **BR-32** | Mọi hành động nhạy cảm (hủy đơn, **dời lịch**, hoàn tiền, **ghi đè mức hoàn**, đổi giá, đổi quyền, đổi cờ `CanPayAtCounter` / `CanCancelLate`) phải ghi **audit log** kèm actor, thời điểm, giá trị trước/sau. | 🔒 **Infrastructure: `SaveChangesInterceptor` của EF Core** — ghi tự động, lập trình viên **không thể quên** |

---

## Tầng nào gánh rule nào — bảng tổng hợp

| Tầng | Rule | Nhận xét |
|---|---|---|
| 🔒 **CSDL** *(không thể lách)* | **BR-06**, BR-14, BR-15, BR-31, **BR-37**, BR-38 | Bất biến sống còn: chống đặt trùng, dời lịch nguyên tử, idempotency |
| 🔒 **Infrastructure** *(tự động)* | **BR-28**, **BR-32** | Cách ly tenant, audit log — hai thứ **không được** phụ thuộc trí nhớ |
| **Domain** *(aggregate + VO)* | BR-01, 02, 07, 10, 12, 13, 17, 18, 20, 21, 22, 23, 26, 27, 36, 38, 40, 42 | Phần lớn rule — đúng như kỳ vọng của Clean Architecture |
| **Domain Service** | BR-16, BR-35 | Logic không thuộc về aggregate nào *(cần bảng giá / chính sách tenant)* |
| **Application** | BR-03, 04, 05, 08, 25, 29, 33, 39 | Validate đầu vào, lọc phạm vi dữ liệu, điều phối job |
| **Worker** *(Hangfire)* | BR-11, BR-19, BR-24 | Việc chạy theo thời gian |
| **Api / Authorization** | BR-09, BR-30, BR-34, BR-40 | Quyền và hình dạng endpoint |
| ⚠️ **Không kiểm chứng được** | BR-41 | Nguyên tắc thiết kế, không phải rule — cân nhắc chuyển sang `09-architecture.md` |

### 🔑 Ba nhận xét rút ra từ bảng này

1. **Bốn rule quan trọng nhất — BR-06, BR-28, BR-32, BR-37 — đều KHÔNG nằm ở tầng Domain.** Chúng nằm ở CSDL và Infrastructure. Vì đây là những bất biến mà **một aggregate không nhìn thấy đủ dữ liệu để tự bảo vệ** *(xem phân tích ở [07-domain-model.md](07-domain-model.md))*, hoặc những thứ **không được phép phụ thuộc vào việc lập trình viên nhớ**.

2. **BR-41 lộ ra là không phải business rule.** Chính cột "Kiểm chứng ở đâu" bắt được điều này — nếu không viết được chỗ kiểm chứng thì đó là *nguyên tắc thiết kế*, không phải *quy tắc nghiệp vụ*. Đây đúng là mục đích tồn tại của cột này.

3. **BR-42 là rule dạng "không được làm gì"** — loại nguy hiểm nhất, vì nó không hỏng ngay mà hỏng âm thầm khi ai đó refactor. Bắt buộc phải có test hồi quy.

---

## 8. Ma trận phân quyền

| Hành động | Guest | Customer | Staff | BranchManager | Partner | Owner |
|---|:--:|:--:|:--:|:--:|:--:|:--:|
| Xem lịch trống, bảng giá | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Tạo booking cho **chính mình** | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Tạo booking **hộ khách khác** | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Xem booking **của mình** | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Xem **mọi** booking | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Hủy booking của mình | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Dời lịch booking của mình** | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Hủy / dời booking **của khách** | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Check-in / đánh dấu No-show | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Thực hiện hoàn tiền | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| **Ghi đè mức hoàn tiền** *(BR-40)* | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Quản lý sân, đóng sân bảo trì | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Sửa bảng giá | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Đặt/gỡ cờ `CanPayAtCounter`, `CanCancelLate` | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Xem báo cáo doanh thu | ❌ | ❌ | ❌ | 🔶 | 🔶 | ✅ |
| Quản lý nhân sự & phân quyền | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

**Chú thích:** ✅ toàn bộ tenant · 🔶 **chỉ trong phạm vi chi nhánh được cấp** · ❌ không có quyền

> 🔶 chính là **data-scoped authorization**. Không thể triển khai bằng `[Authorize(Roles = "Manager")]` — vai trò cho phép **gọi** API, nhưng không ngăn người dùng đổi `branchId` trên URL để xem dữ liệu chi nhánh khác (lỗ hổng **IDOR**, OWASP Broken Access Control hạng #1). Xem [design-decisions.md](design-decisions.md) §4.

---

## 9. Bảng kiểm tra độ phủ test

| Nhóm rule | Số rule | Loại test chính |
|---|---|---|
| Đặt sân (BR-01…09, **BR-33**) | 10 | Unit (domain) + **Integration cho BR-06** |
| Thanh toán (BR-10…15, **BR-14b**) | 7 | Integration (webhook, idempotency) |
| Hủy, hoàn & dời lịch (BR-16…19, **BR-34…41**) | 12 | Unit (tính mức hoàn) + **Integration cho BR-37** *(dời lịch nguyên tử)* |
| No-show (BR-20…22, **BR-42**) | 4 | Unit + Integration (thu hồi `CanPayAtCounter`) |
| Định kỳ (BR-23…27) | 5 | Integration (job sinh booking) |
| Phân quyền (BR-28…30) | 3 | **Integration bắt buộc** — test rò rỉ tenant |
| Dữ liệu (BR-31…32) | 2 | Integration |
| **Tổng** | **43** | |

### Test bắt buộc phát sinh từ CR-07 / CR-08

| Test | Chứng minh rule |
|---|---|
| Đặt 30 phút ở khung cao điểm → bị từ chối | BR-33 |
| Đặt 90 phút = 3 slot liên tiếp, đúng 1 booking | BR-01, BR-02 |
| **Dời lịch mà slot mới đã bị chiếm → 409, đơn cũ vẫn `Confirmed` và slot cũ vẫn bị giữ** | **BR-37** ⭐ |
| Dời lần thứ 3 → bị từ chối | BR-38 |
| Dời sang slot giá cao hơn mà chưa bù tiền → bị từ chối | BR-38 |
| No-show 2 lần → mất `CanPayAtCounter` nhưng **giữ** `CanCancelLate` | BR-22 |
| Dời lịch 3 lần → `no_show_count` vẫn = 0 | BR-42 |
| Staff (không phải Manager) ghi đè hoàn tiền → 403 | BR-40 |
| Manager ghi đè chi nhánh ngoài phạm vi → 403 | BR-29, BR-40 |
