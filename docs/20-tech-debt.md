# 20 — Sổ nợ kỹ thuật (Technical Debt Register)

> **Nguyên tắc:** nợ kỹ thuật **không xấu** — vay có ý thức để đi nhanh là quyết định hợp lệ.
> Cái xấu là **vay mà không ghi sổ**. Nợ không ghi lại là nợ không bao giờ trả.

---

## Cách ghi một khoản nợ

| Trường | Ý nghĩa |
|---|---|
| **Nợ gì** | Mô tả cụ thể, kèm đường dẫn file |
| **Vì sao vay** | Lý do chính đáng tại thời điểm đó *(deadline, chưa đủ thông tin, chờ xác nhận)* |
| **Lãi suất** | Nó làm **chậm việc gì** hoặc **tăng rủi ro gì** mỗi ngày còn tồn tại |
| **Điều kiện trả** | Sự kiện nào kích hoạt việc phải trả |
| **Ước lượng** | Công sức để trả |

> ⚠️ Khoản nợ nào không nêu được **lãi suất** thì không phải nợ kỹ thuật — đó chỉ là sở thích cá nhân. Đừng ghi vào đây.

---

## Nợ đang mở

| Mã | Nợ gì | Vì sao vay | Lãi suất | Điều kiện trả | Ước lượng | Trạng thái |
|---|---|---|---|---|---|:--:|
| **TD-02** | Chưa có [11-api-specification.md](11-api-specification.md) — API sẽ được thiết kế lúc code | Ưu tiên hoàn thiện tài liệu phân tích trước | 🟡 TB — dễ ra API không nhất quán giữa các module | Trước Sprint 1 | 3h | 🟡 Mở |
| **TD-03** | Chưa có [12-sequence-diagrams.md](12-sequence-diagrams.md) | Chưa tới giai đoạn | 🟡 TB — luồng thanh toán và Outbox khó truyền đạt bằng chữ | Trước Sprint 4 | 2h | 🟡 Mở |

---

## Nợ đã trả

| Mã | Nợ gì | Trả ngày | Ghi chú |
|---|---|---|---|
| **TD-01** | [07-domain-model.md](07-domain-model.md) chưa viết; schema thiết kế theo tư duy data-first | **2026-07-31** | Trả bằng một buổi phân tích ranh giới aggregate. Kết quả ngoài mong đợi: quá trình này **phát hiện lại** rằng BR-06 là bất biến xuyên aggregate — chứng minh độc lập cho [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md) từ hướng domain, và lộ ra một **ngoại lệ có chủ đích** (Payment + Booking cùng transaction) trước đó chưa ai ghi lại. Rủi ro **R-11** (Anemic Domain Model) hạ từ 6 xuống 2. |

---

## Nợ **cố ý giữ lại** (Won't fix)

Đây là những thứ trông giống nợ nhưng thực ra là **quyết định kiến trúc đúng ở quy mô hiện tại**. Ghi lại để không bị "sửa" nhầm:

| Nội dung | Vì sao cố ý giữ |
|---|---|
| Không tách CSDL đọc/ghi cho CQRS | Tải ~110 đơn/ngày. CQRS ở đây chỉ tách **code**, tách CSDL là over-engineering (R-22) |
| Không partition bảng `booking` | ~40k dòng/năm. Cân nhắc khi > 10 triệu dòng |
| `booking_slot` phi chuẩn hoá `court_id` + `is_active` | **Bắt buộc** để partial unique index hoạt động — xem [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md) §6 |
| Không dùng Redis để giữ chỗ | Đã có trạng thái `PendingPayment` trong CSDL. Thêm Redis tạo hai nguồn sự thật — xem [design-decisions.md](design-decisions.md) §7 |
| Không event sourcing cho `Booking` | `audit_log` đã đủ truy vết ở quy mô này |

---

## Quy trình

1. Phát hiện nợ khi code hoặc review → ghi ngay vào đây, **không** để trong đầu
2. Rà soát sổ nợ vào **cuối mỗi sprint**
3. Mỗi sprint dành ~10% thời lượng để trả nợ 🔴 Cao
4. Nợ tồn quá **3 sprint** → hoặc trả, hoặc chuyển sang mục *Won't fix* kèm lý do
