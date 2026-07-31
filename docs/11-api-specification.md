# 11 — Đặc tả API

> 🚧 **CHƯA VIẾT.** Viết trước Sprint 1 (thiết kế tay), sau đó Swagger sinh bản chính thức. Dùng skill `/doc-api-specification`.

---

## Nguyên tắc: Contract-First

Thiết kế hợp đồng **trước khi** viết code. File này là bản thiết kế tay; `openapi.json` do Swagger sinh là bản chính thức đồng bộ với code.

> Cái gì **sinh tự động được** thì đừng viết tay — nhưng bản thiết kế tay ban đầu vẫn cần, vì nó là nơi bạn *suy nghĩ* về hợp đồng trước khi bị code dẫn dắt.

---

## Dàn ý bắt buộc

### 1. Quy ước chung
| Hạng mục | Quyết định |
|---|---|
| Base path | `/api/v1` — versioning từ đầu, không chờ đến lúc cần |
| Định dạng lỗi | **RFC 7807 ProblemDetails** (NFR-35) |
| Phân trang | `?page=1&pageSize=20`, trả kèm `totalCount` |
| Sắp xếp / lọc | `?sort=-startUtc&status=Confirmed` |
| Thời gian | ISO-8601 **UTC** (`2026-08-05T12:00:00Z`) |
| Tiền | Số nguyên VND, kèm trường `currency` |
| Idempotency | Header `Idempotency-Key` cho `POST /bookings` và `POST /payments` |
| CorrelationId | Header `X-Correlation-Id`, sinh nếu client không gửi |

### 2. Bảng mã trạng thái — **đúng ngữ nghĩa**
| Mã | Dùng khi | Ví dụ trong dự án |
|---|---|---|
| 200 / 201 | Thành công | |
| 400 | Dữ liệu vào sai định dạng | Slot không liên tiếp |
| 401 | Chưa xác thực | |
| 403 | Đã xác thực nhưng **ngoài phạm vi** | Partner xem chi nhánh khác |
| 404 | Không tồn tại **hoặc** không thuộc phạm vi *(không tiết lộ sự tồn tại)* | |
| **409** | **Xung đột trạng thái** | 🔥 **Slot đã bị đặt (BR-06)** · hủy đơn đã `Completed` |
| 422 | Đúng định dạng nhưng vi phạm nghiệp vụ | Đặt quá 60 ngày |
| 429 | Vượt giới hạn tần suất | Đăng nhập sai nhiều lần |

> ⚠️ Trả `400` cho trùng lịch là **sai** — dữ liệu client gửi lên hoàn toàn hợp lệ, chỉ là trạng thái tài nguyên đã đổi. Đây là `409`.

### 3. Nhóm endpoint
`/auth` · `/branches` · `/courts` · `/price-rules` · `/availability` · `/bookings` · `/payments` · `/webhooks/vnpay` · `/recurring-series` · `/reports` · `/admin/members`

### 4. Đặc tả chi tiết cho các endpoint lõi
Ưu tiên viết đầy đủ (request, response, mọi mã lỗi) cho:
`GET /availability` · `POST /bookings` · `POST /payments` · `POST /webhooks/vnpay` · `POST /bookings/{id}/cancel` · `GET /reports/*`

### 5. Ánh xạ FR → Endpoint
Bảng đối chiếu với [03-functional-requirements.md](03-functional-requirements.md) — **ô trống = FR chưa có API**.

### 6. Chính sách versioning
Thế nào là breaking change, quy trình phát hành `v2`, thời hạn hỗ trợ `v1`.

---

## Tiêu chí hoàn thành

- [ ] Mọi FR ưu tiên 🔴 đều có endpoint tương ứng
- [ ] Mọi mã lỗi trong luồng ngoại lệ của [05-use-cases.md](05-use-cases.md) đều xuất hiện trong đặc tả
- [ ] Không có endpoint nào dùng động từ trong đường dẫn *(trừ hành động không CRUD như `/cancel`, `/check-in`)*
- [ ] Dữ liệu cá nhân **không** nằm trong query string (NFR-23)
