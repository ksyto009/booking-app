# 00 — Bảng thuật ngữ (Glossary)

> **Mục đích:** thống nhất từ vựng giữa khách hàng, tài liệu và code.
> **Quy tắc vàng:** tên trong cột "Trong code" là tên **duy nhất** được dùng trong class, bảng, API. Không có từ đồng nghĩa.

---

## Thuật ngữ nghiệp vụ

| Thuật ngữ (VN) | Trong code | Định nghĩa | Đừng nhầm với |
|---|---|---|---|
| **Chủ sở hữu / Đơn vị kinh doanh** | `Tenant` | Một pháp nhân sở hữu các cụm sân. Anh Dũng = 1 tenant. Ranh giới cách ly dữ liệu cao nhất. | `Owner` (vai trò của người), `Branch` (địa điểm) |
| **Cụm sân / Chi nhánh** | `Branch` | Một địa điểm vật lý chứa nhiều sân. Có giờ mở/đóng riêng. | `Tenant` |
| **Sân** | `Court` | Một mặt sân cụ thể, đặt được độc lập. Có mã (`S1`) và loại (trong nhà/ngoài trời). | `Branch` |
| **Khung giờ / Slot** | `Slot` | Đơn vị đặt nhỏ nhất: **1 giờ chẵn** trên **1 sân**. VD: sân S3, 19:00–20:00 ngày 05/08. | "giờ chơi" chung chung |
| **Đơn đặt sân** | `Booking` | Một lần đặt của khách, gồm **1..N slot liên tiếp** trên **cùng một sân**. | `Slot` (đơn 2 tiếng = 1 booking, 2 slot) |
| **Mã đơn** | `BookingCode` | Mã người đọc được, khách đọc qua điện thoại. VD `BK-2608-0042`. | `Booking.Id` (UUID, chỉ dùng nội bộ) |
| **Giữ chỗ** | `PendingPayment` | Trạng thái đơn đã chiếm slot nhưng chưa trả tiền. Tự hết hạn sau 10 phút. | "đã đặt" |
| **Khách ruột** | `IsTrusted` | Khách được phép đặt online mà trả tiền tại quầy. Do Staff/Owner đánh dấu. | "khách VIP", "thành viên" |
| **Không đến** | `NoShow` | Đã đặt, đã đến giờ + 15 phút, không check-in. | `Cancelled` (khách chủ động hủy) |
| **Đặt định kỳ** | `RecurringSeries` | Lịch lặp hàng tuần (VD: mọi tối thứ 3). Sinh ra nhiều `Booking` con. | `Booking` đơn lẻ |
| **Buổi trong chuỗi** | `Booking` có `SeriesId` | Một `Booking` bình thường, chỉ khác là được sinh từ series. | `RecurringSeries` |
| **Đóng sân tạm** | `CourtClosure` | Khoảng thời gian sân không cho đặt: bảo trì, mưa, sự kiện. | `Court.Status = Maintenance` (đóng vô thời hạn) |
| **Bảng giá** | `PriceRule` | Quy tắc tính giá theo chi nhánh/sân + thứ + khung giờ. Có độ ưu tiên. | `BookingSlot.UnitPrice` (giá **đã chốt** của một đơn) |
| **Tỉ lệ lấp đầy** | `UtilizationRate` | Số slot đã bán ÷ số slot có thể bán, trong một kỳ. | "doanh thu" |
| **Phạm vi chi nhánh** | `BranchScope` | Tập chi nhánh mà một người được phép thấy dữ liệu. Rỗng ⇒ toàn tenant. | `Role` (vai trò) |

---

## Thuật ngữ kỹ thuật dùng trong tài liệu

| Thuật ngữ | Nghĩa trong dự án này |
|---|---|
| **Invariant** (bất biến) | Điều kiện **luôn đúng** với dữ liệu. VD: BR-06 — một sân một giờ chỉ một đơn hiệu lực. |
| **Grain** (độ mịn) | "Một dòng trong bảng này đại diện cho cái gì". Grain của `booking_slot` = một giờ sân bị chiếm. |
| **Aggregate** | Cụm object có một gốc, đảm bảo bất biến bên trong. `Booking` là gốc của `BookingSlot`. |
| **TOCTOU** | Time-Of-Check to Time-Of-Use — khe hở giữa lúc kiểm tra và lúc ghi, nguồn gốc race condition. |
| **Idempotent** | Chạy N lần cho kết quả giống hệt 1 lần. |
| **Outbox** | Bảng trung gian để ghi DB và gửi message trong cùng một transaction. |
| **Data-scoped authorization** | Phân quyền theo **bản ghi nào**, không chỉ theo **hành động gì**. |
| **Soft delete** | Đánh dấu `deleted_at` thay vì xoá thật. |
| **Rolling window** | Cửa sổ trượt — chỉ sinh trước dữ liệu trong N tuần tới, không sinh vô hạn. |

---

## Từ **không** được dùng

| ❌ Tránh | ✅ Dùng thay | Vì sao |
|---|---|---|
| "reservation" | `Booking` | Một khái niệm, một tên |
| "order" | `Booking` | `Order` gợi ý thương mại điện tử, không đúng nghiệp vụ |
| "field", "pitch" | `Court` | Nhất quán tiếng Anh |
| "user" khi nói về khách | `Customer` | `User` là tài khoản đăng nhập, `Customer` là vai trò nghiệp vụ |
| "cancel" cho no-show | `NoShow` | Hai trạng thái khác nhau, chính sách tiền khác nhau |
| "location", "store" | `Branch` | |
