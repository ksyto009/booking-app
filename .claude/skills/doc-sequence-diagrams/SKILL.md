---
name: doc-sequence-diagrams
description: Chuẩn viết và rà soát docs/12-sequence-diagrams.md — sơ đồ tuần tự dự án Court Booking (luồng đặt sân tranh chấp, thanh toán webhook, Outbox, job nền). Dùng khi cần mô tả luồng đi qua nhiều thành phần hoặc có nhánh lỗi phức tạp.
---

# Skill: Sơ đồ tuần tự

## Mục tiêu
Vẽ những luồng mà **mô tả bằng chữ thì không ai hiểu nổi** — luồng đi qua nhiều thành phần, có nhánh lỗi, có yếu tố thời gian.

> Vẽ được sơ đồ tuần tự nghĩa là **hiểu**, không phải thuộc lòng. Đây là thứ gây ấn tượng mạnh nhất khi phỏng vấn.

## Chọn luồng nào để vẽ?

Vẽ khi luồng thoả **ít nhất 2** điều kiện sau:

| Điều kiện | Ví dụ |
|---|---|
| Đi qua ≥3 thành phần | Client → API → Gateway → Webhook → Worker → Broker |
| Có nhánh lỗi cần xử lý khác nhau | Webhook trùng vs chữ ký sai vs đơn đã hết hạn |
| Có tranh chấp đồng thời | Hai người đặt cùng slot |
| Có phần bất đồng bộ | Outbox → RabbitMQ |
| Được kích hoạt bởi thời gian | Job hết hạn giữ chỗ |

❌ **Đừng vẽ** CRUD đơn giản — lãng phí và sẽ mục rữa.

## Quy ước bắt buộc

1. **Dùng mermaid `sequenceDiagram`**, không dùng ảnh — để diff được bằng git.
2. **Đánh dấu ranh giới transaction** bằng `rect` hoặc `note over`. Đây là thông tin quan trọng nhất mà sơ đồ truyền tải.
3. **Nhánh lỗi vẽ bằng `alt`/`else`, không được bỏ qua.** Sơ đồ chỉ có happy path là sơ đồ vô dụng.
4. **Phân biệt rõ đồng bộ (`->>`) và bất đồng bộ (`-)`)**.
5. **Ghi chú chỗ có khe hở thời gian** (TOCTOU) — đó thường là chỗ sinh ra bug.
6. Mỗi sơ đồ có phần **giải thích ngắn phía dưới**: điểm mấu chốt là gì, cạm bẫy ở đâu.

## Checklist trước khi đóng

- [ ] Mỗi sơ đồ có ≥1 nhánh lỗi
- [ ] Ranh giới transaction được đánh dấu
- [ ] Phân biệt được đồng bộ / bất đồng bộ
- [ ] Mọi luồng ngoại lệ 🔥 trong `05-use-cases.md` xuất hiện ở ≥1 sơ đồ
- [ ] Có sơ đồ cho luồng **tranh chấp đồng thời**
- [ ] Có sơ đồ cho luồng do **job nền** kích hoạt
- [ ] Không vẽ CRUD tầm thường

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chỉ vẽ happy path | Mất toàn bộ giá trị — phần khó nằm ở nhánh lỗi |
| Không đánh dấu transaction | Người đọc không biết chỗ nào nguyên tử |
| Vẽ bằng ảnh PNG | Không diff được, mục rữa ngay |
| Vẽ quá chi tiết (từng lời gọi hàm) | Sai tầng trừu tượng, sửa code là hỏng sơ đồ |
| Bỏ qua yếu tố thời gian trong luồng có hết hạn | Không thấy được race condition |

## Liên kết
`05-use-cases.md` · `11-api-specification.md` · `09-architecture.md` · `16-decision-records/`
