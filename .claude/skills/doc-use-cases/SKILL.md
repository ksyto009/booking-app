---
name: doc-use-cases
description: Chuẩn viết và rà soát docs/05-use-cases.md — use case dự án Court Booking (sơ đồ, bảng tổng hợp, đặc tả chi tiết với luồng chính/thay thế/ngoại lệ). Dùng khi thêm kịch bản người dùng mới hoặc khi cần liệt kê đầy đủ luồng lỗi trước khi code.
---

# Skill: Use Cases

## Mục tiêu
Mô tả **tương tác giữa tác nhân và hệ thống**, đặc biệt là **luồng ngoại lệ** — phần chiếm ~70% công sức code thật nhưng hay bị bỏ quên.

## Cấu trúc bắt buộc

| Phần | Nội dung |
|---|---|
| **1. Tác nhân** | Bảng: tên · chính/phụ · mô tả. **Bắt buộc có tác nhân hệ thống** (Scheduler, PaymentGateway) |
| **2. Sơ đồ Use Case** | mermaid `flowchart`, nhóm UC theo gói chức năng |
| **3. Bảng tổng hợp** | `UC-xx` · tên · tác nhân chính · ưu tiên · `FR` liên quan · độ phức tạp |
| **4. Đặc tả chi tiết** | **Đầy đủ** cho các UC phức tạp nhất (thường 4–5 cái) |
| **5. Ma trận UC × Tác nhân** | Có phân biệt phạm vi dữ liệu (✅ toàn tenant / 🔶 giới hạn chi nhánh) |
| **6. UC cần integration test bắt buộc** | Danh sách + lý do |

## Mẫu đặc tả chi tiết

```
Mã · Tác nhân chính · Tác nhân phụ · Mô tả · Tần suất · Trigger
Tiền điều kiện
Hậu điều kiện (thành công)
Luồng chính          — bảng đánh số: # | Tác nhân | Hành động
Luồng thay thế       — A1, A2… : điều kiện → xử lý
Luồng ngoại lệ       — E1, E2… : điều kiện → xử lý → mã HTTP
Rule liên quan (BR-xx) · NFR liên quan
Ghi chú kỹ thuật     — cạm bẫy, lý do thiết kế
```

## Quy tắc chất lượng

1. 🔥 **Luồng ngoại lệ là phần giá trị nhất.** UC không có ít nhất 3 luồng ngoại lệ là UC viết dối.
2. **Bắt buộc có tác nhân hệ thống** (Scheduler/Job). Bỏ sót loại này = quên toàn bộ mảng background job.
3. **Mỗi bước trong luồng chính phải ghi rõ AI làm** — tác nhân hay hệ thống.
4. **Trích mã `BR-xx` ngay tại bước áp dụng**, không gom cuối bài.
5. Luồng ngoại lệ phải có **mã HTTP** — đây là đầu vào trực tiếp cho `11-api-specification.md`.
6. **Không viết chi tiết UI** ("bấm nút màu xanh"). Use case mô tả *ý định*, không mô tả giao diện.
7. UC có tranh chấp đồng thời **phải** có luồng ngoại lệ mô tả race condition — và luồng đó **phải** có integration test.

## Checklist trước khi đóng

- [ ] Mọi `FR-xx` ưu tiên 🔴 xuất hiện ở ≥1 UC
- [ ] Mọi UC chi tiết có ≥3 luồng ngoại lệ
- [ ] Mọi luồng ngoại lệ có mã HTTP
- [ ] Có UC do **Scheduler** kích hoạt
- [ ] Có UC mô tả **tranh chấp đồng thời**
- [ ] Ma trận UC × Tác nhân phân biệt được phạm vi dữ liệu
- [ ] Danh sách UC cần integration test đã liệt kê

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chỉ viết happy path | Thiếu ~70% code thật; bug lộ ở production |
| Quên tác nhân Scheduler | Quên hết background job |
| Mô tả thao tác UI thay vì ý định | UC chết ngay khi giao diện đổi |
| Gom "các lỗi khác trả 400" | Mã HTTP sai ngữ nghĩa; 409 và 422 bị đánh đồng với 400 |
| Không nêu tần suất | Không biết UC nào cần tối ưu |

## Liên kết
`03-functional-requirements.md` · `06-business-rules.md` · `11-api-specification.md` · `12-sequence-diagrams.md` · `15-testing-strategy.md`
