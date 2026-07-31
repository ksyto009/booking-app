---
name: doc-api-specification
description: Chuẩn viết và rà soát docs/11-api-specification.md — đặc tả API dự án Court Booking (REST, mã trạng thái, ProblemDetails, versioning, idempotency). Dùng khi thiết kế endpoint mới hoặc khi rà soát tính nhất quán của API.
---

# Skill: Đặc tả API

## Mục tiêu
Thiết kế **hợp đồng trước khi viết code** (contract-first), nhất quán trên toàn hệ thống.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Quy ước chung** — base path, versioning, định dạng lỗi, phân trang, thời gian, tiền, header |
| 2 | **Bảng mã trạng thái** — mã · dùng khi nào · **ví dụ cụ thể trong dự án** |
| 3 | **Nhóm endpoint** theo tài nguyên |
| 4 | **Đặc tả chi tiết** cho endpoint lõi: request, response, **mọi mã lỗi** |
| 5 | **Ánh xạ FR → Endpoint** |
| 6 | **Chính sách versioning** |

## Quy tắc chất lượng

1. **Tài nguyên là danh từ số nhiều**: `/bookings`, không phải `/getBooking`.
   Ngoại lệ hợp lệ: hành động không CRUD → `/bookings/{id}/cancel`, `/bookings/{id}/check-in`.
2. 🔥 **Mã trạng thái phải đúng ngữ nghĩa:**

   | Tình huống | Đúng | Sai phổ biến |
   |---|---|---|
   | Slot đã bị người khác đặt | **409 Conflict** | 400 |
   | Đúng định dạng, vi phạm nghiệp vụ | **422** | 400 |
   | Đã đăng nhập nhưng ngoài phạm vi dữ liệu | **403** *(hoặc 404 nếu không muốn tiết lộ sự tồn tại)* | 401 |
   | Vượt giới hạn tần suất | **429** | 400 |

3. **Định dạng lỗi thống nhất — RFC 7807 ProblemDetails.** Một định dạng cho toàn hệ thống, không mỗi endpoint một kiểu.
4. **Versioning từ ngày đầu** (`/api/v1`). Thêm version sau khi đã có client là việc rất đau.
5. **Idempotency-Key bắt buộc** cho endpoint tạo tài nguyên có hệ quả tiền bạc.
6. **Không đưa dữ liệu cá nhân vào query string** — nó nằm trong log, lịch sử trình duyệt, referrer.
7. **Mọi luồng ngoại lệ trong `05-use-cases.md` phải xuất hiện ở đặc tả** với mã tương ứng.
8. **Bản chính thức là OpenAPI do Swagger sinh.** File này là bản thiết kế tay để *suy nghĩ* trước — đừng duy trì hai bản chi tiết song song.

## Checklist trước khi đóng

- [ ] Mọi `FR-xx` 🔴 có endpoint tương ứng
- [ ] Mọi luồng ngoại lệ 🔥 trong use case có mã lỗi trong đặc tả
- [ ] Không dùng động từ trong path (trừ hành động không CRUD)
- [ ] Mọi endpoint tạo/sửa có mô tả lỗi validation
- [ ] Endpoint liên quan tiền có `Idempotency-Key`
- [ ] Endpoint có dữ liệu thuộc chi nhánh ghi rõ hành vi khi ngoài phạm vi
- [ ] Phân trang nhất quán trên mọi endpoint danh sách
- [ ] Không có dữ liệu cá nhân trong query string

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Trả 200 kèm `{"success": false}` | Client không xử lý lỗi được bằng cơ chế chuẩn |
| Dùng 400 cho mọi loại lỗi | Mất khả năng phân biệt lỗi client vs xung đột trạng thái |
| Bỏ versioning "để sau" | Breaking change làm hỏng client đang chạy |
| Định dạng lỗi mỗi nơi mỗi kiểu | Frontend phải viết N nhánh xử lý |
| Duy trì OpenAPI thủ công song song với code | Hai bản lệch nhau, tài liệu nói dối |

## Liên kết
`03-functional-requirements.md` · `05-use-cases.md` · `12-sequence-diagrams.md` · `14-security.md`
