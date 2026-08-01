# 01 — Tổng quan dự án

| | |
|---|---|
| **Tên dự án** | Court Booking Platform |
| **Phiên bản tài liệu** | 1.0 |
| **Ngày** | 2026-07-30 |
| **Loại hệ thống** | Web application (SaaS, đa chủ sở hữu) |
| **Kiến trúc** | Modular Monolith → sẵn sàng tách Microservices |

---

## 1. Vấn đề cần giải quyết

Chủ sân sở hữu **3 cụm sân cầu lông, tổng 15 sân** trong cùng một khu vực. Hệ thống vận hành hiện tại: **sổ tay giấy + Zalo + điện thoại**.

| Vấn đề | Nguyên nhân gốc | Thiệt hại |
|---|---|---|
| **Trùng lịch** | 2 nhân viên 2 ca cùng ghi vào một ô sổ; chủ nhắn Zalo mà nhân viên chưa kịp ghi | Khách đến nơi không có sân → cãi nhau, mất uy tín |
| **No-show** | Đặt qua Zalo không thu tiền trước, không có ràng buộc gì | Sân trống giờ cao điểm, mất doanh thu trực tiếp |
| **Không đo được hiệu quả** | Không có dữ liệu, chỉ có sổ giấy | Không biết sân nào ế, khung giờ nào trống để giảm giá kéo khách |
| **Chủ không quản được từ xa** | Thông tin nằm trong sổ ở quầy | Phụ thuộc hoàn toàn vào nhân viên trực |

---

## 2. Mục tiêu

### Mục tiêu kinh doanh

| # | Mục tiêu | Thước đo |
|---|---|---|
| G1 | Loại bỏ hoàn toàn trùng lịch | **0** ca trùng lịch sau khi go-live |
| G2 | Giảm no-show | Từ ~15% xuống < 5% |
| G3 | Giảm tải nhân viên trực | ≥ 60% đơn được khách tự đặt online |
| G4 | Ra quyết định dựa trên dữ liệu | Chủ xem được tỉ lệ lấp đầy theo sân/khung giờ |
| G5 | Mở rộng cho chủ sân khác thuê dùng | Hỗ trợ nhiều tenant ngay từ thiết kế |

### Mục tiêu kỹ thuật (ngoài phạm vi khách hàng)

Đây là dự án học tập nhằm đạt năng lực **Fresher/Junior Backend hoặc Fullstack**, với trọng tâm:
concurrency control · Clean Architecture · CQRS · phân quyền theo phạm vi dữ liệu · tích hợp thanh toán idempotent · Outbox pattern · Docker + CI/CD.

---

## 3. Phạm vi tổng thể

### Trong phạm vi
Đặt sân theo khung giờ · thanh toán trước · hủy & hoàn tiền · check-in / no-show · đặt định kỳ hàng tuần · quản lý chi nhánh–sân–bảng giá · phân quyền nhiều vai trò và nhiều chi nhánh · báo cáo doanh thu & tỉ lệ lấp đầy.

### Ngoài phạm vi (v1)
Bán nước / thuê vợt / quản lý kho *(khách hàng tự loại — thu tiền mặt tại quầy)* · quản lý gửi xe · chấm công nhân viên · app mobile native · nhiều môn thể thao có mô hình đặt khác nhau.

---

## 4. Các bên liên quan (Stakeholders)

| Bên liên quan | Vai trò | Quan tâm chính | Ảnh hưởng |
|---|---|---|---|
| **Chủ sân** | Người bỏ tiền, quyết định cuối | Hết trùng lịch, hết no-show, xem được doanh thu | 🔴 Cao |
| **Đối tác góp vốn** | Đồng sở hữu một phần Cụm 3 | Chỉ xem doanh thu Cụm 3, **không được thấy Cụm 1 và 2** | 🟡 Trung bình — nhưng tạo ràng buộc kiến trúc lớn |
| **Nhân viên quầy** | Người dùng hằng ngày | Thao tác nhanh, đặt hộ khách được, không phải ghi sổ | 🔴 Cao — họ dùng nhiều nhất |
| **Quản lý cụm** | Vận hành một chi nhánh | Xem lịch, đóng sân bảo trì, xem báo cáo cụm mình | 🟡 Trung bình |
| **Khách chơi** | Người dùng cuối | Đặt nhanh trên điện thoại, thấy sân trống, không mất tiền oan | 🔴 Cao |
| **Nhóm thuê cố định** | Khách chiếm ~**40% doanh thu** | Giữ được sân cố định hàng tuần, được giảm giá | 🔴 Cao — nhóm sinh lời nhất |

---

## 5. Ràng buộc

| Loại | Ràng buộc |
|---|---|
| **Thời gian** | Khách kỳ vọng ~2–3 tháng cho bản dùng được |
| **Nguồn lực** | 1 lập trình viên, ~10–15h/tuần → Sprint 1 tuần, 3–5 story nhỏ |
| **Kỹ thuật** | .NET 9, PostgreSQL, Redis, RabbitMQ, Hangfire, Docker, Next.js *(đã chốt từ đầu)* |
| **Nghiệp vụ** | Khách ruột **không** bị ép chuyển khoản trước → sinh ra cờ `CanPayAtCounter` (BR-12) |
| **Ngân sách** | Không có chi phí hạ tầng → dùng VNPay **sandbox**, self-host bằng Docker |

---

## 6. Bối cảnh kỹ thuật tóm tắt

| Hạng mục | Lựa chọn | Xem chi tiết |
|---|---|---|
| Kiến trúc | Modular Monolith, Clean Architecture, CQRS | [09-architecture.md](09-architecture.md) |
| CSDL | PostgreSQL 16 + EF Core 9 | [10-database-design.md](10-database-design.md) |
| Chống trùng lịch | Partial unique index ở tầng CSDL | [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md) |
| Đa chủ sở hữu | Row-level `tenant_id` + Global Query Filter | [design-decisions.md](design-decisions.md) §5 |
| Cache | Redis — **chỉ** cho lịch trống, rate limit, OTP | [design-decisions.md](design-decisions.md) §7 |
| Nhắn tin | RabbitMQ + Outbox pattern | [design-decisions.md](design-decisions.md) §8 |

---

## 7. Bản đồ tài liệu

Xem [README.md](README.md) để biết thứ tự đọc.
