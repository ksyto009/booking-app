---
name: doc-functional-requirements
description: Chuẩn viết và rà soát docs/03-functional-requirements.md — yêu cầu chức năng dự án Court Booking (mã FR, ưu tiên MoSCoW, ma trận truy vết). Dùng khi thêm/sửa tính năng hoặc khi kiểm tra xem có yêu cầu nào bị bỏ quên.
---

# Skill: Yêu cầu chức năng

## Mục tiêu
Liệt kê **hệ thống phải làm được gì**, có mã tham chiếu, có ưu tiên, và **truy vết được** ngược lên yêu cầu nghiệp vụ.

## Cấu trúc bắt buộc

1. Ghi chú cách đọc + ký hiệu ưu tiên (🔴 Must · 🟡 Should · 🔵 Could · ⚪ Won't)
2. **Nhóm theo miền chức năng** (`FR-A` Tài khoản, `FR-B` Danh mục, …), mỗi nhóm một bảng:
   `Mã` · `Yêu cầu` · `Ưu tiên` · `Rule liên quan (BR-xx)` · `UC liên quan`
3. **Ma trận truy vết** cuối file: `BRQ-xx` → các `FR-xx` phủ nó

## Quy tắc chất lượng

1. **Một FR = một hành vi kiểm chứng được.** Nếu không nghĩ ra được cách test → viết lại.
2. **Cấm "và" nối hai chức năng** trong một FR. Tách ra.
3. FR mô tả **cái gì**, không mô tả **làm thế nào**.
   ❌ "Dùng Redis cache lịch trống" · ✅ "Xem lịch trống trả về trong < 200ms" *(và cái sau thuộc NFR)*
4. **Đánh số liên tục, không tái sử dụng.** FR bị bỏ thì đánh dấu ⚪ Won't, **không** xoá — người đọc cần biết nó đã được cân nhắc.
5. Mỗi FR ưu tiên 🔴 **phải** gắn với ≥1 `UC-xx`.
6. Ma trận truy vết **không được có ô trống** — ô trống = yêu cầu nghiệp vụ bị bỏ quên.

## Checklist trước khi đóng

- [ ] Mọi `BRQ-xx` trong `02-*` xuất hiện ở ma trận truy vết với ≥1 FR
- [ ] Mọi FR 🔴 có `UC-xx` tương ứng trong `05-*`
- [ ] Mọi FR có ràng buộc phức tạp trỏ tới `BR-xx` trong `06-*`
- [ ] Không có FR nào chứa từ chỉ công nghệ (Redis, RabbitMQ, EF Core…)
- [ ] Không có FR nào không thể viết test
- [ ] Nhóm chức năng phủ hết: tài khoản, danh mục, tra cứu, giao dịch lõi, tiền, hủy, vận hành, quản trị, báo cáo, thông báo

## Lỗi thường gặp

| Lỗi | Ví dụ | Cách sửa |
|---|---|---|
| Viết giải pháp thay vì yêu cầu | "Có nút xuất Excel" | "Chủ sân lấy được doanh thu tháng" |
| FR quá to | "Quản lý booking" | Tách thành tạo / xem / hủy / check-in |
| FR mơ hồ | "Hệ thống thân thiện" | Không kiểm chứng được → bỏ hoặc chuyển sang NFR |
| Bỏ quên nhóm thông báo & quản trị | | Hai nhóm bị quên nhiều nhất |
| Xoá FR bị loại | | Đánh dấu ⚪ Won't thay vì xoá |

## Liên kết
`02-business-requirements.md` · `05-use-cases.md` · `06-business-rules.md` · `11-api-specification.md`
