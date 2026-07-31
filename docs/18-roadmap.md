# 18 — Lộ trình & Phạm vi (Roadmap)

> **Nhịp độ:** ~10–15h/tuần · Sprint **1 tuần** · mỗi sprint 3–5 user story nhỏ
> **Nguyên tắc:** mỗi sprint phải ra một **lát cắt dọc chạy được** (từ API xuống DB), không chia theo tầng kỹ thuật.

---

## 1. Phạm vi v1 (MoSCoW)

### 🔴 Must have — không có thì hệ thống vô dụng
- Đăng ký / đăng nhập bằng SĐT, JWT + Refresh Token
- Quản lý chi nhánh, sân, bảng giá
- Xem lịch trống theo ngày / theo sân
- Đặt sân online + thanh toán trước (VNPay sandbox)
- Đặt sân tại quầy bởi Staff
- Hủy đơn + hoàn tiền theo chính sách
- Check-in / No-show
- ⭐ **Chống đặt trùng sân**
- Phân quyền theo vai trò **và theo phạm vi chi nhánh**
- Báo cáo: doanh thu theo chi nhánh, tỉ lệ lấp đầy theo sân & khung giờ

### 🟡 Should have — làm sau khi Must xong
- Đặt định kỳ hàng tuần (recurring)
- Thông báo Zalo/SMS nhắc lịch trước 2 tiếng
- Đóng sân tạm thời + tự hoàn tiền đơn bị ảnh hưởng
- Audit log tra cứu được trên giao diện

### 🔵 Could have — có thì tốt
- Thẻ thành viên / tích điểm
- "Sân nào cũng được" (hệ thống tự gán)
- Đánh giá sau khi chơi
- Xuất báo cáo Excel

### ⚪ Won't have (v1) — **cắt dứt khoát**
| Không làm | Vì sao |
|---|---|
| Bán nước / thuê vợt / quản lý kho | Khách hàng **tự loại** — thu tiền mặt tại quầy |
| Quản lý gửi xe | Miễn phí, không cần hệ thống |
| Chấm công nhân viên | Ngoài phạm vi bài toán đặt sân |
| App mobile native | Web mobile-first là đủ (80% truy cập từ điện thoại) |
| Nhiều môn thể thao mô hình khác | Chỉ cầu lông/pickleball — cùng mô hình slot giờ |

> ⚠️ Danh sách **Won't have** quan trọng ngang Must have. Khi khách hàng đề nghị thêm giữa chừng (rủi ro R-19), đây là tài liệu để nói "không, hoặc để sprint sau".

---

## 2. Lộ trình Sprint

| Sprint | Chủ đề | User story chính | Trọng tâm học | Trạng thái |
|:--:|---|---|---|:--:|
| **0** | Nền móng | Solution skeleton, Docker Compose, migration đầu, logging, exception handler, health check | Clean Architecture, DI, EF Core setup | ⬜ |
| **1** | Danh tính | Đăng ký OTP, đăng nhập, JWT + Refresh Token rotation, rate limit | **Security** | ⬜ |
| **2** | Danh mục & quyền | CRUD chi nhánh/sân/bảng giá, phân quyền theo vai trò + phạm vi chi nhánh | **CQRS, data-scoped authorization** | ⬜ |
| **3** | 🏆 **Đặt sân** | Xem lịch trống, đặt sân, **chống trùng lịch**, job hết hạn giữ chỗ | **Concurrency** — sprint quan trọng nhất | ⬜ |
| **4** | Tiền | Thanh toán VNPay, webhook + idempotency, hủy & hoàn tiền, **Outbox** | **Distributed correctness** | ⬜ |
| **5** | Định kỳ & vận hành | Chuỗi đặt định kỳ, Hangfire, RabbitMQ, thông báo, check-in/no-show | Batch + messaging | ⬜ |
| **6** | Báo cáo & chất lượng | Báo cáo, Redis cache, hoàn thiện test, architecture test | Performance, testing | ⬜ |
| **7** | Đưa lên môi trường thật | CI/CD GitHub Actions, Nginx, deploy, monitoring | **DevOps** | ⬜ |

---

## 3. Chi tiết Sprint 0 — Nền móng

| Task | Nội dung | Ước lượng |
|---|---|---|
| S0-01 | Tạo solution Clean Architecture: `Domain` / `Application` / `Infrastructure` / `Api` + `.editorconfig` + `Directory.Build.props` | 2h |
| S0-02 | `docker-compose.yml`: PostgreSQL 16 + Redis 7 + pgAdmin | 1.5h |
| S0-03 | EF Core `DbContext` + migration đầu tiên (`tenant`, `branch`, `court`) | 3h |
| S0-04 | Serilog structured logging + middleware CorrelationId | 2h |
| S0-05 | Global exception handler → RFC 7807 ProblemDetails | 1.5h |
| S0-06 | Health check `/health/live`, `/health/ready` | 1h |
| S0-07 | Architecture test: Domain không tham chiếu EF Core / ASP.NET | 1.5h |

**Definition of Done Sprint 0:** `docker compose up` trên máy trắng → API chạy, `/health/ready` trả 200, migration áp thành công, log ra JSON có CorrelationId.

> ⚠️ **Điều kiện tiên quyết:** phải viết xong [07-domain-model.md](07-domain-model.md) **trước** S0-03. Không có domain model thì bảng sẽ được thiết kế theo tư duy data-first và tầng Domain sẽ thành anemic (rủi ro R-11).

---

## 4. Các mốc lớn (Milestones)

| Mốc | Nội dung | Sau sprint | Chứng minh được điều gì |
|---|---|:--:|---|
| **M1** | Nền móng chạy được bằng một lệnh | 0 | Biết dựng dự án chuẩn công nghiệp |
| **M2** | Đăng nhập an toàn + phân quyền theo phạm vi | 2 | Hiểu security thật, không chỉ `[Authorize]` |
| **M3** | 🏆 **Đặt sân không bao giờ trùng** | 3 | **Điểm mạnh nhất khi phỏng vấn** |
| **M4** | Dòng tiền hoàn chỉnh, chịu được webhook trùng | 4 | Hiểu tính đúng đắn trong hệ phân tán |
| **M5** | Hệ thống vận hành đầy đủ, có báo cáo | 6 | Sản phẩm dùng được thật |
| **M6** | CI/CD + deploy + monitoring | 7 | Có thể nói chuyện với DevOps |

---

## 5. Sau v1 (chưa cam kết)

| Hạng mục | Điều kiện kích hoạt |
|---|---|
| Prometheus + Grafana | Sau khi có traffic thật để đo |
| Deploy AWS/Azure | Khi cần demo public cho nhà tuyển dụng |
| Tách microservice module `Payment` | Chỉ khi có lý do thật — hiện **không** có |
| Read replica / partition bảng booking | Khi > 10 triệu dòng (hiện ~40k/năm — **còn rất xa**) |
| Thẻ thành viên / tích điểm | Khi khách hàng thực sự yêu cầu |

> Ba dòng đầu là những thứ nghe "ngầu" nhưng **hiện tại không có NFR nào đòi hỏi**. Ghi ở đây để nhớ rằng chúng đã được cân nhắc và **cố ý hoãn** — đó là quyết định kiến trúc, không phải bỏ sót.

---

## 6. Theo dõi tiến độ

Sau mỗi sprint, cập nhật cột **Trạng thái** ở §2 và tạo `03-sprints/sprint-N.md` gồm:
việc đã hoàn thành · kiến thức mới học được · lỗi và khó khăn gặp phải · bài học kinh nghiệm · việc còn nợ sang sprint sau · cập nhật [20-tech-debt.md](20-tech-debt.md) và [17-risk-analysis.md](17-risk-analysis.md).
