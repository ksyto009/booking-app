# 13 — Triển khai (Deployment)

> 🚧 **CHƯA VIẾT.** Phần Docker viết ở Sprint 0; phần CI/CD và deploy ở Sprint 7. Dùng skill `/doc-deployment`.

---

## Dàn ý bắt buộc

### 1. Môi trường
| Môi trường | Mục đích | Dữ liệu | Ai truy cập |
|---|---|---|---|
| `local` | Phát triển | Seed giả | Lập trình viên |
| `staging` | Kiểm thử trước khi phát hành | Ẩn danh hoá | Đội dự án |
| `production` | Chạy thật | Thật | Người dùng cuối |

### 2. Docker
Multi-stage build · chạy bằng **non-root user** · `HEALTHCHECK` · `.dockerignore` · ghim phiên bản base image *(không dùng `latest`)*.
`docker-compose.yml`: API + PostgreSQL + Redis + RabbitMQ + Nginx, có volume và healthcheck phụ thuộc.

**Mục tiêu NFR-39:** máy trắng chỉ cần `docker compose up` là chạy được toàn hệ thống.

### 3. Cấu hình & bí mật
Cấu hình qua biến môi trường · secrets **không** nằm trong image hay repo · liệt kê đầy đủ biến bắt buộc, app **fail fast** nếu thiếu.

### 4. CI Pipeline (GitHub Actions)
`restore → build → unit test → integration test (Testcontainers) → architecture test → phân tích tĩnh → quét lỗ hổng → build & push image`

### 5. CD Pipeline
Staging tự động khi merge `main` · production cần **duyệt tay** · smoke test sau deploy · quay lui tự động nếu smoke test hỏng.

### 6. 🔥 Chiến lược Migration — mục dễ gây sự cố nhất
| Nguyên tắc | Nội dung |
|---|---|
| **Expand → Migrate → Contract** | Thêm cột mới (tương thích ngược) → deploy code dùng cả hai → dọn cột cũ ở lần sau |
| Cấm | `DROP COLUMN` / `RENAME` **cùng lúc** với deploy code mới |
| Ai chạy migration | **Không** chạy tự động lúc app khởi động khi có nhiều instance — dùng job riêng hoặc init container |
| Rollback | Mọi migration phải có đường lui, hoặc được chứng minh là an toàn khi rollback code |

### 7. Backup & Phục hồi
Lịch backup · nơi lưu · thời gian giữ · **RTO/RPO** · 🔴 **bắt buộc diễn tập restore ít nhất một lần và ghi lại kết quả** *(NFR-37 — backup chưa từng restore coi như không có backup)*.

### 8. Nginx & TLS
Reverse proxy, chứng chỉ, giới hạn kích thước request, header bảo mật.

---

## Tiêu chí hoàn thành

- [ ] Máy trắng chạy được toàn hệ thống bằng **một lệnh**
- [ ] Có kịch bản rollback đã được thử thật
- [ ] Đã diễn tập restore backup và ghi lại kết quả
- [ ] Không có secret nào trong repo hay image
