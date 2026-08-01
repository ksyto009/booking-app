# 16 — Architecture Decision Records (ADR)

> **ADR ghi lại LÝ DO, không ghi lại kết quả.** Kết quả đã nằm trong code và schema.
> Đây là thứ cứu bạn 6 tháng sau khỏi câu *"sao hồi đó mình làm thế này nhỉ?"* — và là thứ bạn kể lại khi phỏng vấn.

---

## Danh sách

| Mã | Tiêu đề | Trạng thái | Ngày |
|---|---|---|---|
| [0001](0001-booking-concurrency-strategy.md) | Chiến lược chống đặt trùng sân | ✅ Accepted | 2026-07-30 |
| [0002](0002-slot-grain-30-minutes.md) | Đổi grain slot từ 60′ xuống 30′ *(CR-07)* | ✅ Accepted | 2026-07-31 |
| [0003](0003-atomic-reschedule.md) | Dời lịch nguyên tử, không dùng "hủy rồi đặt lại" *(CR-08b)* | ✅ Accepted | 2026-07-31 |

> 📌 **ADR-0001 KHÔNG bị lật bởi ADR-0002.** Điểm gãy được ghi trước trong ADR-0001 §6 đã xảy ra, nhưng phương án chọn *(căn mốc `:00`/`:30`)* giữ nguyên hiệu lực của partial unique index. Đây là ví dụ điển hình vì sao mục **"điểm gãy đã biết"** trong ADR là bắt buộc.

### ADR dự kiến

| Mã | Chủ đề | Khi nào viết |
|---|---|---|
| 0004 | Chiến lược đa chủ sở hữu (row-level vs schema vs database) | Trước Sprint 2 |
| 0005 | Vai trò của Redis — dùng ở đâu và **không** dùng ở đâu | Sprint 3 |
| 0006 | Outbox pattern cho tính nhất quán ghi-và-gửi | Sprint 4 |
| 0007 | Chiến lược idempotency cho thanh toán | Sprint 4 |
| 0008 | Cửa sổ trượt cho đặt định kỳ | Sprint 5 |
| 0009 | Ranh giới module & đường cắt sang microservices | Sprint 6 |

---

## Quy tắc

| Quy tắc | Nội dung |
|---|---|
| **Bất biến** | ADR đã `Accepted` thì **không sửa nội dung**. Đổi ý → viết ADR mới, đánh dấu ADR cũ `Superseded by ADR-00XX` |
| **Đánh số tăng dần** | Không tái sử dụng số, kể cả khi ADR bị `Rejected` |
| **Một quyết định một file** | Đừng gộp nhiều quyết định |
| **Phải có phương án bị loại** | ADR chỉ có "tôi chọn X" mà không có "tôi đã loại Y, Z vì…" là ADR vô giá trị |

**Trạng thái:** `Proposed` → `Accepted` → `Superseded` / `Deprecated` / `Rejected`

---

## Khi nào viết ADR?

✅ **Viết** khi quyết định: khó đảo ngược · ảnh hưởng nhiều module · chọn giữa nhiều phương án hợp lý · người mới sẽ thắc mắc *"sao không làm cách kia?"*

❌ **Không viết** cho: quy ước đặt tên · lựa chọn thư viện nhỏ dễ thay · chi tiết cài đặt trong một class

---

## Cấu trúc bắt buộc

`Bối cảnh` → `Vấn đề` → `Các phương án đã cân nhắc (≥3, kèm ưu/nhược)` → `Quyết định` → `Lý do chọn` → `Hệ quả (tích cực **và** tiêu cực)` → `Kiểm chứng bằng test` → `Câu hỏi phỏng vấn liên quan`

> Mục **Hệ quả tiêu cực** và **điểm gãy đã biết** là phần giá trị nhất. ADR chỉ toàn ưu điểm là ADR nói dối.

Dùng skill `/doc-adr` để tạo ADR mới đúng định dạng.
