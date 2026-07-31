# 09 — Kiến trúc hệ thống (C4 Level 2–3)

> 🚧 **CHƯA VIẾT.** Viết trước Sprint 1. Dùng skill `/doc-architecture`.

---

## Dàn ý bắt buộc

### 1. Quyết định kiến trúc tổng thể
**Modular Monolith**, thiết kế sẵn để tách Microservices.
Nêu rõ: vì sao **không** làm microservices ngay *(tải 110 đơn/ngày, 1 lập trình viên — xem [04-non-functional-requirements.md](04-non-functional-requirements.md))*.

### 2. C4 Level 2 — Container
| Container | Công nghệ | Trách nhiệm |
|---|---|---|
| Web App | Next.js | Giao diện mobile-first |
| API | ASP.NET Core 9 | REST API, xác thực, nghiệp vụ |
| Background Worker | Hangfire (trong API hoặc tách) | Job hết hạn giữ chỗ, sinh buổi định kỳ, đẩy Outbox |
| Database | PostgreSQL 16 | Nguồn sự thật duy nhất |
| Cache | Redis 7 | Lịch trống, rate limit, OTP |
| Message Broker | RabbitMQ | Sự kiện nghiệp vụ |
| Reverse Proxy | Nginx | TLS, định tuyến |

### 3. C4 Level 3 — Component (bên trong API)
Clean Architecture 4 tầng + **quy tắc phụ thuộc**:

```
Api ──► Application ──► Domain
 │           │
 └──► Infrastructure ──┘

Domain KHÔNG phụ thuộc bất cứ tầng nào.   ← kiểm tra bằng architecture test (NFR-30)
```

Với mỗi tầng: chứa gì, **không** được chứa gì.

### 4. Chia module (nghiệp vụ, KHÔNG phải kỹ thuật)
`Identity` · `Catalog` · `Booking` · `Payment` · `Reporting` · `Notification`

| Quy tắc | Nội dung |
|---|---|
| Sở hữu dữ liệu | Mỗi module sở hữu bảng của mình, module khác **không** join thẳng |
| Giao tiếp | Qua interface công khai hoặc domain event |
| Kiểm tra | Architecture test tự động trong CI (NFR-31) |

> ❌ **Sai:** `Controllers/`, `Services/`, `Repositories/` — chia theo tầng kỹ thuật thì không bao giờ tách được microservice.

### 5. CQRS
Tách Command/Query ở **tầng code**, **không** tách database. Giải thích vì sao đủ ở quy mô này.

### 6. Xử lý xuyên suốt (Cross-cutting)
Validation · Authorization · Logging + CorrelationId · Exception → ProblemDetails · Transaction (`IUnitOfWork`) · Caching · Outbox
Nêu rõ mỗi thứ được cài ở đâu (pipeline behavior của MediatR, middleware, interceptor của EF Core).

### 7. Đường cắt sang Microservices
Module nào tách trước nếu cần, cần thay đổi gì, **và vì sao hiện tại KHÔNG nên tách**.

### 8. Sơ đồ triển khai
Container nào chạy ở đâu → nối sang [13-deployment.md](13-deployment.md).

---

## Tiêu chí hoàn thành

- [ ] Quy tắc phụ thuộc được phát biểu rõ **và có architecture test kiểm chứng**
- [ ] Mỗi lựa chọn công nghệ trỏ được về một NFR có số
- [ ] Ranh giới module giải thích được bằng ngôn ngữ nghiệp vụ
- [ ] Mọi quyết định lớn đều có ADR tương ứng trong [16-decision-records/](16-decision-records/)
