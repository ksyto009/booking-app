---
name: doc-deployment
description: Chuẩn viết và rà soát docs/13-deployment.md — triển khai dự án Court Booking (Docker, môi trường, CI/CD, chiến lược migration, backup/restore). Dùng khi dựng Docker Compose, viết pipeline, hoặc lên kế hoạch deploy và rollback.
---

# Skill: Triển khai (Deployment)

## Mục tiêu
Đảm bảo hệ thống **dựng lại được từ số 0**, **deploy an toàn**, và **quay lui được**.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Môi trường** — mục đích · dữ liệu · ai truy cập |
| 2 | **Docker** — Dockerfile, compose, healthcheck |
| 3 | **Cấu hình & bí mật** — danh sách biến bắt buộc |
| 4 | **CI Pipeline** — các bước và điều kiện chặn merge |
| 5 | **CD Pipeline** — staging tự động, production duyệt tay, smoke test |
| 6 | 🔥 **Chiến lược Migration CSDL** |
| 7 | **Backup & Phục hồi** — RTO/RPO + **kết quả diễn tập restore** |
| 8 | **Reverse proxy & TLS** |

## Quy tắc chất lượng

1. **Máy trắng phải chạy được toàn hệ thống bằng một lệnh.** Nếu cần "cài thêm cái này trước" thì tài liệu chưa xong.
2. 🔥 **Migration theo Expand → Migrate → Contract:**
   - Bước 1: thêm cột/bảng mới, tương thích ngược
   - Bước 2: deploy code dùng được cả cũ và mới
   - Bước 3: **lần deploy sau** mới dọn cột cũ

   **Cấm** `DROP COLUMN` / `RENAME` cùng lúc với deploy code mới — trong khoảng thời gian hai phiên bản cùng chạy, phiên bản cũ sẽ vỡ.
3. **Không chạy migration tự động lúc app khởi động khi có nhiều instance** — chúng sẽ đua nhau. Dùng job riêng hoặc init container.
4. **Mọi deploy phải có đường quay lui đã được thử thật**, không phải "về lý thuyết thì rollback được".
5. 🔴 **Backup chưa từng restore = không có backup.** Phải diễn tập restore ít nhất một lần và **ghi lại kết quả kèm thời gian thực tế**.
6. **Secrets không nằm trong repo, không nằm trong image.** App phải **fail fast** khi thiếu biến bắt buộc, không chạy với giá trị mặc định âm thầm.
7. **Ghim phiên bản base image**, không dùng `latest` — build hôm nay và build tháng sau phải cho kết quả giống nhau.
8. **Container chạy bằng non-root user.**

## Checklist trước khi đóng

- [ ] `docker compose up` trên máy trắng → hệ thống chạy
- [ ] Có danh sách đầy đủ biến môi trường bắt buộc
- [ ] CI chặn merge khi test đỏ
- [ ] Có smoke test sau deploy
- [ ] Chiến lược migration ghi rõ, có ví dụ expand/contract
- [ ] Có kịch bản rollback **đã thử thật**
- [ ] Đã diễn tập restore backup và ghi kết quả
- [ ] Không có secret trong repo/image
- [ ] Base image ghim phiên bản, chạy non-root

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Migration tự chạy lúc khởi động, nhiều instance | Đua nhau migrate, hỏng schema |
| `DROP COLUMN` cùng lúc deploy | Phiên bản cũ đang chạy bị vỡ |
| Không có rollback plan | Sự cố kéo dài hàng giờ |
| Backup không bao giờ thử restore | Phát hiện backup hỏng đúng lúc cần nhất |
| Secret trong `appsettings.json` commit lên git | Lộ vĩnh viễn trong lịch sử git |
| Dùng `image: postgres:latest` | Build không tái lập được |

## Liên kết
`09-architecture.md` · `19-runbook.md` · `14-security.md` · `04-non-functional-requirements.md`
