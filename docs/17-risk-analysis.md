# 17 — Phân tích rủi ro (Risk Analysis)

> **Phân biệt quan trọng:**
> **Assumption** = điều tôi *giả định đúng* để đi tiếp → quản lý bằng cách **xác minh rồi đóng**.
> **Risk** = điều *có thể xảy ra* và gây hại → quản lý bằng **xác suất × tác động → biện pháp giảm thiểu**.

**Thang điểm:** Xác suất (P) và Tác động (I): 1 Thấp · 2 Trung bình · 3 Cao. **Điểm = P × I**.
**Ngưỡng hành động:** ≥6 phải có biện pháp giảm thiểu **trước khi** bước vào giai đoạn liên quan.

---

## 1. Rủi ro kỹ thuật

| Mã | Rủi ro | P | I | Điểm | Biện pháp giảm thiểu | Kế hoạch dự phòng |
|---|---|:-:|:-:|:-:|---|---|
| **R-01** | **Đặt trùng sân** do race condition | 3 | 3 | **9** 🔴 | Partial unique index ở tầng CSDL ([ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md)); integration test bắn 20 request song song | Nếu vẫn xảy ra: job đối soát hằng đêm phát hiện slot trùng và cảnh báo |
| **R-02** | **Rò rỉ dữ liệu giữa tenant** do quên `WHERE tenant_id` | 2 | 3 | **6** 🔴 | EF Core Global Query Filter áp tự động; integration test đăng nhập tenant A rồi thử đọc dữ liệu tenant B | Rà soát toàn bộ raw SQL định kỳ |
| **R-03** | **IDOR** — Partner đổi `branchId` trên URL xem chi nhánh khác | 2 | 3 | **6** 🔴 | Kiểm tra `BranchScope` ở tầng Application, không tin tham số client; test tự động cho mọi endpoint có branch | Audit log ghi mọi truy cập báo cáo |
| **R-04** | **Test xanh giả** do dùng EF Core InMemory (không có unique index, không có transaction thật) | 3 | 3 | **9** 🔴 | **Cấm InMemory provider**; dùng Testcontainers PostgreSQL cho mọi integration test | Architecture test chặn tham chiếu tới InMemory package |
| **R-05** | `booking_slot.is_active` **lệch** với `booking.status` → slot bị khoá ma | 2 | 3 | **6** 🔴 | Đặt logic trong Domain Model (`Booking.Cancel()` tự gọi `slot.Release()`), không rải ở Handler | Job đối soát hằng đêm phát hiện lệch |
| **R-06** | **Webhook giả mạo** — kẻ tấn công tự gửi "đã thanh toán" | 2 | 3 | **6** 🔴 | Bắt buộc xác thực chữ ký trước khi xử lý; ghi log Warning kèm IP khi chữ ký sai | Đối soát doanh thu với báo cáo cổng thanh toán hằng ngày |
| **R-07** | **Mất event** khi RabbitMQ ngừng → khách không nhận thông báo | 2 | 2 | 4 🟡 | Outbox pattern — ghi event cùng transaction với booking | Worker retry có backoff |
| **R-08** | **Job sinh buổi định kỳ chạy 2 lần** → sinh trùng | 2 | 2 | 4 🟡 | Idempotent dựa vào `generated_until` + unique index | Hangfire distributed lock |
| **R-09** | **Migration làm hỏng production** | 2 | 3 | **6** 🔴 | Expand → migrate → contract; không `DROP COLUMN` cùng lúc deploy code; luôn có script rollback | Backup trước mỗi lần deploy + đã diễn tập restore |
| **R-10** | **Ranh giới module bị xói mòn** — module join thẳng bảng của nhau, không tách được microservice | 3 | 2 | **6** 🔴 | Architecture test tự động trong CI kiểm tra dependency giữa module | Refactor định kỳ, ghi vào [20-tech-debt.md](20-tech-debt.md) |
| **R-11** | **Anemic Domain Model** — logic tràn vào Handler, Clean Architecture chỉ còn là thư mục | 3 | 2 | **6** 🔴 | Viết [07-domain-model.md](07-domain-model.md) **trước** Sprint 0; architecture test chặn Domain tham chiếu EF Core | Code review theo checklist |
| **R-12** | **N+1 query** trên API xem lịch trống → vi phạm NFR-02 | 2 | 2 | 4 🟡 | Projection thay vì `Include`; log đếm số câu SQL mỗi request | `EXPLAIN ANALYZE` trước khi merge |
| **R-13** | Redis chết kéo sập luồng đặt sân | 1 | 3 | 3 🟢 | Redis **chỉ** là cache; cache miss → đọc thẳng CSDL. Không dùng Redis để giữ chỗ | Health check phân biệt liveness/readiness |
| **R-14** | **Log lộ dữ liệu cá nhân** (SĐT, token) | 2 | 2 | 4 🟡 | Serilog destructuring policy mask SĐT; cấm log object request thô | Rà log định kỳ |
| **R-25** | **Phân mảnh lịch** — nhiều khoảng 30 phút rời rạc, không ai đặt được khối liên tục *(hệ quả của CR-07)* | 2 | 2 | 4 🟡 | **BR-33** — tối thiểu 60 phút ở khung cao điểm, chỉ cho 30 phút ở khung thấp điểm | Báo cáo tỉ lệ lấp đầy theo khung giờ; nếu phân mảnh vẫn cao thì nâng ngưỡng tối thiểu |
| **R-26** | **Lạm dụng dời lịch** — khách dời liên tục để giữ chỗ miễn phí | 2 | 2 | 4 🟡 | **BR-38** — giới hạn 2 lần/đơn, cấu hình theo tenant; phải bù tiền khi slot mới đắt hơn | Theo dõi `reschedule_count` trung bình; thu hồi `CanCancelLate` nếu lạm dụng |
| **R-27** | **Ghi đè hoàn tiền bị lạm dụng** — quản lý tự ý hoàn 100% cho người quen | 2 | 2 | 4 🟡 | **BR-40** bắt buộc nhập lý do + audit log (BR-32); phạm vi giới hạn theo chi nhánh (BR-29) | Báo cáo định kỳ số lần ghi đè theo từng quản lý |

## 2. Rủi ro nghiệp vụ

| Mã | Rủi ro | P | I | Điểm | Biện pháp giảm thiểu |
|---|---|:-:|:-:|:-:|---|
| **R-15** | **Khách quen không dùng web**, vẫn nhắn Zalo → mục tiêu G3 không đạt | 3 | 2 | **6** 🔴 | Nhân viên nhập hộ vào hệ thống (UC-08) — hệ thống vẫn là nguồn sự thật duy nhất |
| **R-16** | **Nhân viên vẫn ghi sổ song song** → quay lại hai nguồn sự thật, trùng lịch tái diễn | 2 | 3 | **6** 🔴 | Đào tạo + bỏ hẳn sổ giấy khi go-live; báo cáo chỉ lấy từ hệ thống |
| **R-17** | Chính sách hoàn tiền gây tranh cãi với khách | 2 | 2 | 4 🟡 | Hiển thị rõ số tiền hoàn **trước khi** khách xác nhận (FR-32); ghi audit log |
| **R-18** | Khách ruột lạm dụng `PayAtCounter` rồi no-show | 2 | 2 | 4 🟡 | Tự động thu hồi cờ `CanPayAtCounter` sau 2 lần no-show trong 90 ngày (BR-22) |
| **R-19** | Chủ sân đổi yêu cầu giữa chừng (thêm bán nước, gọi món…) | 3 | 2 | **6** 🔴 | Danh sách **Won't have** đã chốt ở [18-roadmap.md](18-roadmap.md); thay đổi phải vào backlog sprint sau |

## 3. Rủi ro dự án

| Mã | Rủi ro | P | I | Điểm | Biện pháp giảm thiểu |
|---|---|:-:|:-:|:-:|---|
| **R-20** | **Không đủ thời gian** — 10–15h/tuần, 1 người, phạm vi lớn | 3 | 3 | **9** 🔴 | MoSCoW nghiêm ngặt; Must-have trước; cắt Should/Could không thương tiếc |
| **R-21** | **Tài liệu mục rữa** — viết xong không cập nhật, thành tài liệu nói dối | 3 | 2 | **6** 🔴 | Cái gì sinh tự động được thì đừng viết tay; DoD của mỗi story bao gồm "docs đã cập nhật" |
| **R-22** | **Over-engineering** — nhồi công nghệ cho đủ bộ dù tải chỉ 110 đơn/ngày | 3 | 2 | **6** 🔴 | Mọi quyết định hạ tầng phải trỏ được về một NFR có số |
| **R-23** | VNPay sandbox không phản hồi khi demo | 2 | 2 | 4 🟡 | Có chế độ giả lập cổng thanh toán cho môi trường dev/demo |
| **R-24** | Học lan man, mất trọng tâm phỏng vấn | 2 | 2 | 4 🟡 | Bám Tier S trong `design-decisions.md`; ôn bằng skill `interview-drill` |

---

## 4. Bảng nhiệt rủi ro

| | I=1 Thấp | I=2 Trung bình | I=3 Cao |
|---|---|---|---|
| **P=3 Cao** | 🟢 | 🔴 R-10, R-11, R-15, R-19, R-21, R-22 | 🔴 **R-01, R-04, R-20** |
| **P=2 TB** | 🟢 | 🟡 R-07, R-08, R-12, R-14, R-17, R-18, R-23, R-24 | 🔴 R-02, R-03, R-05, R-06, R-09, R-16 |
| **P=1 Thấp** | 🟢 | 🟢 | 🟢 R-13 |

**3 rủi ro điểm 9 phải xử lý trước tiên:** R-01 (đặt trùng), R-04 (test xanh giả), R-20 (không đủ thời gian).

---

## 5. Giả định (Assumptions)

| # | Giả định | Rủi ro nếu sai | Xác minh khi nào | Trạng thái |
|---|---|---|---|---|
| **A1** | Mọi chi nhánh cùng múi giờ Việt Nam | Thấp — schema đã lưu `time_zone` mỗi branch để phòng | Ngay | 🟡 Mở |
| **A2** | Chính sách hoàn 24h/4h là chấp nhận được | Trung bình — chỉ là tham số cấu hình, đổi dễ | Sprint 3 | 🟡 Mở |
| **A3** | VNPay sandbox đủ để demo, không cần merchant thật | Thấp | Sprint 4 | 🟡 Mở |
| **A4** | Hai cờ `CanPayAtCounter` / `CanCancelLate` do Staff/Manager đánh dấu thủ công | Thấp — có thể tự động hoá sau | Sprint 2 | 🟡 Mở |
| **A5** | Một số điện thoại = một tài khoản, không chia sẻ | **Trung bình — nhóm chơi chung hay dùng một số** | Sprint 1 | 🟡 Mở |
| **A6** | Cửa sổ sinh buổi định kỳ 8 tuần là đủ | Thấp — tham số cấu hình | Sprint 5 | 🟡 Mở |
| **A7** | Tỉ lệ lấp đầy ~40% dùng để ước lượng tải | Thấp — nếu sai gấp 3 lần thì tải vẫn nhỏ | Sau go-live | 🟡 Mở |

---

## 6. Quy trình theo dõi

- Rà soát rủi ro **cuối mỗi sprint**, cập nhật điểm và trạng thái
- Rủi ro **≥6** phải có biện pháp giảm thiểu **đã triển khai** trước khi vào giai đoạn liên quan
- Rủi ro trở thành sự cố thật → viết postmortem và chuyển thành mục trong [20-tech-debt.md](20-tech-debt.md)
- Assumption được xác minh → đổi trạng thái ✅ **Đóng**, ghi kết quả
