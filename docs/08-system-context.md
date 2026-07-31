# 08 — Bối cảnh hệ thống (C4 Level 1 — System Context)

> 🚧 **CHƯA VIẾT.** Viết trước Sprint 1. Dùng skill `/doc-system-context`.

---

## Mục đích

Trả lời **một** câu hỏi duy nhất: *hệ thống của chúng ta nói chuyện với AI và CÁI GÌ bên ngoài?*

Đây là sơ đồ dành cho **khách hàng và người mới vào dự án** — không có chi tiết kỹ thuật bên trong.

---

## Dàn ý bắt buộc

### 1. Sơ đồ Context (mermaid)
Một hộp duy nhất là "Court Booking Platform", xung quanh là:

**Người dùng:** Customer · Staff · BranchManager · Partner · Owner

**Hệ thống ngoài:**
| Hệ thống | Chiều | Mục đích | Giao thức |
|---|---|---|---|
| VNPay | ↔ | Thanh toán + webhook kết quả | HTTPS, chữ ký |
| Zalo OA / SMS Gateway | → | Gửi OTP, xác nhận đơn, nhắc lịch | HTTPS |
| *(Sau này)* Prometheus | ← | Thu thập metric | HTTP scrape |

### 2. Bảng mô tả từng tương tác
Với mỗi đường nối: ai gọi ai, dữ liệu gì đi qua, đồng bộ hay bất đồng bộ, chuyện gì xảy ra khi bên kia chết.

### 3. Ranh giới tin cậy (Trust Boundary)
Đánh dấu chỗ dữ liệu đi từ vùng **không tin được** vào vùng tin được — đây chính là đầu vào cho [14-security.md](14-security.md).

### 4. Chế độ suy giảm (Degraded Mode)
| Hệ thống ngoài chết | Hệ thống ta còn làm được gì |
|---|---|
| VNPay | Vẫn xem lịch, vẫn đặt tại quầy. Đơn online hết hạn giữ chỗ và tự giải phóng |
| SMS/Zalo | Mọi thứ vẫn chạy; thông báo nằm chờ trong Outbox |

---

## Tiêu chí hoàn thành

- [ ] Mọi hệ thống ngoài đều có mô tả **chuyện gì xảy ra khi nó chết**
- [ ] Sơ đồ đọc được bởi người **không** biết lập trình
- [ ] Ranh giới tin cậy được đánh dấu rõ
