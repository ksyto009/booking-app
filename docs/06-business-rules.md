# 06 — Quy tắc nghiệp vụ (Business Rules)

> **Quy tắc bắt buộc:** mỗi `BR-xx` phải **kiểm chứng được** và có **ít nhất một test** trích mã rule trong tên test.
> Ví dụ: `Booking_ShouldRejectOverlappingSlot_BR06()`

---

## 1. Đặt sân

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-01** | Đơn vị đặt là **slot 1 giờ chẵn**, bắt đầu đúng đầu giờ (18:00, 19:00…). Không hỗ trợ 30/90 phút. | Domain: `TimeSlot` value object |
| **BR-02** | Một booking gồm 1..N slot **liên tiếp** trên **cùng một sân**. Không cho đặt slot rời rạc trong một đơn. | Domain: `Booking.Create()` |
| **BR-03** | Chỉ đặt được trong giờ mở cửa của chi nhánh (mặc định 05:00–23:00 giờ VN). | Application: validator |
| **BR-04** | Không đặt được slot quá khứ. Đơn **online** phải đặt trước ít nhất **30 phút**; đơn tại quầy không bị ràng buộc này. | Application |
| **BR-05** | Không đặt được xa quá **60 ngày** kể từ hôm nay. | Application |
| **BR-06** | 🔒 **Một sân + một khung giờ ⇒ tối đa MỘT booking đang hiệu lực.** | **CSDL: partial unique index** |
| **BR-07** | Booking `PendingPayment` **vẫn chiếm slot**. | `booking_slot.is_active = true` |
| **BR-08** | Không đặt được sân `Maintenance` hoặc rơi vào khoảng `court_closure`. | Application |
| **BR-09** | Khách **tự chọn sân cụ thể** (v1). "Sân nào cũng được" là Could-have. | UI + API |

> ⭐ **BR-06 là bất biến quan trọng nhất của toàn hệ thống.** Vi phạm nó gây hậu quả ngoài đời thật: hai nhóm khách cùng đến sân. Đây là lý do khách hàng bỏ tiền làm hệ thống.

## 2. Thanh toán

| Mã | Quy tắc | Kiểm chứng ở đâu |
|---|---|---|
| **BR-10** | Đơn **online** mặc định **trả trước 100%** trước khi được xác nhận. | Domain |
| **BR-11** | `PendingPayment` quá **10 phút** chưa thanh toán → `Expired`, **giải phóng slot**. | Hangfire job |
| **BR-12** | Khách `IsTrusted` được đặt online với `PayAtCounter`, xác nhận ngay không cần trả trước. | Domain |
| **BR-13** | Đơn tạo tại quầy bởi Staff mặc định `PayAtCounter` và `Confirmed` ngay. | Domain |
| **BR-14** | Giá được **chốt cứng (snapshot)** vào từng slot tại thời điểm đặt. Đổi bảng giá sau đó **không** ảnh hưởng đơn cũ. | `booking_slot.unit_price` |
| **BR-15** | Mọi giao dịch thanh toán phải có `idempotency_key`. Webhook trùng không được ghi nhận 2 lần. | CSDL: unique constraint |

> 💡 **BR-12 là lời giải cho xung đột nghiệp vụ:** khách hàng muốn "thu tiền trước để hết no-show" nhưng cũng nói "bắt khách ruột chuyển khoản trước thì kỳ lắm". Cơ chế `IsTrusted` + thu hồi tự động (BR-22) hoà giải hai mong muốn mâu thuẫn này.

## 3. Hủy & Hoàn tiền

| Mã | Quy tắc |
|---|---|
| **BR-16** | Mức hoàn theo thời điểm hủy: **≥24h → 100%** · **4–24h → 50%** · **<4h → 0%** |
| **BR-17** | Chỉ hủy được khi trạng thái ∈ {`PendingPayment`, `Confirmed`}. |
| **BR-18** | Nếu **phía sân** hủy (sân hỏng, mưa, sự cố) → hoàn **100%** bất kể thời điểm, bắt buộc ghi lý do. |
| **BR-19** | Hoàn tiền là quy trình **bất đồng bộ** (`Pending → Succeeded / Failed`). Đơn hủy ngay, tiền về sau. |

## 4. No-show & Check-in

| Mã | Quy tắc |
|---|---|
| **BR-20** | Quá giờ bắt đầu **15 phút** chưa check-in → Staff được quyền đánh dấu `NoShow`. |
| **BR-21** | `NoShow` **không hoàn tiền**. |
| **BR-22** | Khách `IsTrusted` bị `NoShow` **2 lần trong 90 ngày** → **tự động** mất trạng thái trusted. |

## 5. Đặt định kỳ (Recurring)

| Mã | Quy tắc |
|---|---|
| **BR-23** | Series định nghĩa bởi: sân + thứ trong tuần + giờ bắt đầu + số giờ + ngày bắt đầu + ngày kết thúc. Giảm giá mặc định **15%**. |
| **BR-24** | Hệ thống **sinh trước** booking con trong cửa sổ **8 tuần** (rolling window), job chạy hàng tuần. **Không sinh vô hạn.** |
| **BR-25** | Buổi bị trùng lịch → **bỏ qua buổi đó**, ghi log, thông báo. **Không làm hỏng cả series.** |
| **BR-26** | Hủy **một buổi** không hủy series. Hủy **series** chỉ ảnh hưởng buổi **tương lai**. |
| **BR-27** | v1: thanh toán **theo từng buổi**, không thu tiền cả tháng một lần. |

> ⚠️ **BR-25 thể hiện một nguyên tắc tổng quát:** trong xử lý theo lô, **lỗi cục bộ không được làm hỏng toàn cục**.

## 6. Phân quyền & Đa chủ sở hữu

| Mã | Quy tắc |
|---|---|
| **BR-28** | Mọi bản ghi nghiệp vụ thuộc về **đúng một tenant**. Truy vấn chéo tenant bị chặn ở **tầng hạ tầng**, không phụ thuộc lập trình viên nhớ. |
| **BR-29** | `BranchManager` và `Partner` chỉ thấy dữ liệu các **chi nhánh được cấp phạm vi**. |
| **BR-30** | `Partner` là quyền **chỉ đọc báo cáo**. Không tạo/sửa/hủy bất kỳ dữ liệu nào. |

## 7. Dữ liệu & Vận hành

| Mã | Quy tắc |
|---|---|
| **BR-31** | Master data (`branch`, `court`, `price_rule`) dùng **soft delete**. Dữ liệu giao dịch (`booking`, `payment`) **KHÔNG** soft delete — chúng đã có trạng thái riêng. |
| **BR-32** | Mọi hành động nhạy cảm (hủy đơn, hoàn tiền, đổi giá, đổi quyền, đổi `IsTrusted`) phải ghi **audit log** kèm actor, thời điểm, giá trị trước/sau. |

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
| Hủy booking **của khách** | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Check-in / đánh dấu No-show | ❌ | ❌ | 🔶 | 🔶 | ❌ | ✅ |
| Thực hiện hoàn tiền | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Quản lý sân, đóng sân bảo trì | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Sửa bảng giá | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Đánh dấu khách `IsTrusted` | ❌ | ❌ | ❌ | 🔶 | ❌ | ✅ |
| Xem báo cáo doanh thu | ❌ | ❌ | ❌ | 🔶 | 🔶 | ✅ |
| Quản lý nhân sự & phân quyền | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

**Chú thích:** ✅ toàn bộ tenant · 🔶 **chỉ trong phạm vi chi nhánh được cấp** · ❌ không có quyền

> 🔶 chính là **data-scoped authorization**. Không thể triển khai bằng `[Authorize(Roles = "Manager")]` — vai trò cho phép **gọi** API, nhưng không ngăn người dùng đổi `branchId` trên URL để xem dữ liệu chi nhánh khác (lỗ hổng **IDOR**, OWASP Broken Access Control hạng #1). Xem [design-decisions.md](design-decisions.md) §4.

---

## 9. Bảng kiểm tra độ phủ test

| Nhóm rule | Số rule | Loại test chính |
|---|---|---|
| Đặt sân (BR-01…09) | 9 | Unit (domain) + **Integration cho BR-06** |
| Thanh toán (BR-10…15) | 6 | Integration (webhook, idempotency) |
| Hủy & hoàn (BR-16…19) | 4 | Unit (tính mức hoàn) + Integration |
| No-show (BR-20…22) | 3 | Unit + Integration (thu hồi trusted) |
| Định kỳ (BR-23…27) | 5 | Integration (job sinh booking) |
| Phân quyền (BR-28…30) | 3 | **Integration bắt buộc** — test rò rỉ tenant |
| Dữ liệu (BR-31…32) | 2 | Integration |
| **Tổng** | **32** | |
