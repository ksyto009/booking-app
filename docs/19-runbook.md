# 19 — Sổ tay vận hành (Runbook)

> 🚧 **CHƯA VIẾT.** Viết ở Sprint 7, cập nhật sau **mỗi sự cố**. Dùng skill `/doc-runbook`.
> ⚠️ Đây là tài liệu duy nhất được đọc **khi đang hoảng**, lúc 11 giờ đêm. Viết cho người đang mệt và đang lo: câu ngắn, lệnh copy-paste được, không giải thích dài dòng.

---

## Dàn ý bắt buộc

### 1. Thông tin nhanh
Đường dẫn dashboard · lệnh xem log · lệnh vào CSDL · nơi lưu backup · ai gọi khi bí.

### 2. Kiểm tra sức khoẻ hệ thống
```
/health/live    → tiến trình còn sống?
/health/ready   → có phục vụ được không? (CSDL, Redis, RabbitMQ)
```
Bảng: mỗi thành phần chết thì triệu chứng gì, ảnh hưởng gì, xử lý ra sao.

### 3. Kịch bản sự cố — mỗi cái theo mẫu **Triệu chứng → Chẩn đoán → Xử lý → Xác minh**

| # | Sự cố | Ưu tiên |
|---|---|---|
| RB-01 | Khách báo **đặt trùng sân** | 🔴 P1 |
| RB-02 | Khách đã trả tiền nhưng đơn không được xác nhận *(webhook không về / xử lý lỗi)* | 🔴 P1 |
| RB-03 | Slot bị "khoá ma" — không ai đặt được nhưng không có đơn nào *(`is_active` lệch)* | 🔴 P1 |
| RB-04 | Redis chết | 🟡 P2 |
| RB-05 | RabbitMQ chết / message dồn ứ trong Outbox | 🟡 P2 |
| RB-06 | Cổng thanh toán không phản hồi | 🟡 P2 |
| RB-07 | Job Hangfire không chạy → đơn quá hạn không được giải phóng | 🟡 P2 |
| RB-08 | Truy vấn chậm / CSDL tải cao | 🟡 P2 |
| RB-09 | Migration lỗi khi deploy | 🔴 P1 |
| RB-10 | Nghi ngờ rò rỉ dữ liệu giữa tenant | 🔴 **P0** |

### 4. Câu truy vấn hay dùng
Tìm đơn theo mã / SĐT · **phát hiện slot lệch trạng thái** · đếm message Outbox chưa xử lý · webhook chưa xử lý · top truy vấn chậm.

### 5. Quy trình khẩn cấp
Quay lui bản phát hành · restore backup · tạm dừng job · bật chế độ chỉ đọc.

### 6. Sau sự cố
Bắt buộc viết postmortem **không đổ lỗi**: dòng thời gian → nguyên nhân gốc → hành động khắc phục → cập nhật file này để lần sau xử lý nhanh hơn.

---

## Tiêu chí hoàn thành

- [ ] Mỗi kịch bản có lệnh **copy-paste chạy được ngay**
- [ ] Người chưa từng đọc code vẫn theo được các bước
- [ ] Mỗi sự cố thật đều được bổ sung vào đây trong vòng 48 giờ
