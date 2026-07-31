# 12 — Sơ đồ tuần tự (Sequence Diagrams)

> 🚧 **CHƯA VIẾT.** Viết trước Sprint 3. Dùng skill `/doc-sequence-diagrams`.

---

## Vì sao file này quan trọng hơn vẻ ngoài của nó

Dự án có 5 luồng mà **mô tả bằng chữ thì không ai hiểu nổi** — chúng đi qua nhiều thành phần, có nhánh lỗi, và có yếu tố thời gian. Đây cũng chính là **những luồng gây ấn tượng mạnh nhất khi phỏng vấn**, vì vẽ được nghĩa là hiểu, không phải thuộc lòng.

---

## 5 sơ đồ bắt buộc

### SD-01 — 🔥 Đặt sân có tranh chấp (UC-06/E1)
Hai khách bấm đặt cùng slot cùng lúc. Phải thể hiện:
- Cả hai đều **đọc thấy trống** (khe hở TOCTOU)
- Cả hai cùng `INSERT`
- PostgreSQL từ chối cái thứ hai bằng `uq_slot_no_double_booking`
- Ứng dụng bắt `UniqueViolation` → trả **409**

> Đây là sơ đồ quan trọng nhất của cả dự án. Vẽ được nó là chứng minh hiểu [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md).

### SD-02 — 🔥 Thanh toán trọn vòng (UC-10)
`Client → API → VNPay → (khách thanh toán) → webhook → xác thực chữ ký → transaction → Outbox → worker → RabbitMQ → thông báo`
Phải có nhánh: **webhook trùng**, **chữ ký sai**, **webhook đến sau khi đơn đã hết hạn**.

### SD-03 — Hết hạn giữ chỗ (UC-07)
`Hangfire → quét đơn PendingPayment quá hạn → transaction → Expired + giải phóng slot → Outbox`
Thể hiện rõ **vì sao không cần Redis** cho việc này.

### SD-04 — Hủy đơn & hoàn tiền bất đồng bộ (UC-12)
Thể hiện việc **tách rời** hủy đơn (đồng bộ, xong ngay) và hoàn tiền (bất đồng bộ, có thể retry) — kể cả khi cổng thanh toán lỗi.

### SD-05 — Sinh buổi định kỳ với xung đột cục bộ (UC-17)
Vòng lặp qua N buổi, một buổi bị trùng → **bỏ qua và đi tiếp**, không rollback cả chuỗi.

---

## Quy ước vẽ

- Dùng mermaid `sequenceDiagram` để diff được bằng git *(không dùng ảnh)*
- Ghi rõ ranh giới **transaction** bằng `rect` hoặc `note`
- Nhánh lỗi vẽ bằng `alt` / `else`, **không** bỏ qua
- Đánh dấu rõ chỗ nào **đồng bộ**, chỗ nào **bất đồng bộ**

---

## Tiêu chí hoàn thành

- [ ] Mỗi sơ đồ có ít nhất một nhánh lỗi
- [ ] Ranh giới transaction được đánh dấu rõ
- [ ] Mọi luồng ngoại lệ 🔥 trong [05-use-cases.md](05-use-cases.md) đều xuất hiện ở ít nhất một sơ đồ
