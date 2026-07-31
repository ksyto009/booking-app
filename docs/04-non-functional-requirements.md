# 04 — Yêu cầu phi chức năng (Non-Functional Requirements)

> **Quy tắc bắt buộc:** mỗi NFR phải có **con số** và **cách đo**. Không chấp nhận "nhanh", "ổn định", "nhiều người dùng".
> NFR là **đầu vào trực tiếp của mọi quyết định kiến trúc** — không có NFR thì kiến trúc chỉ là mê tín.

---

## 1. Cơ sở ước lượng tải

Khách hàng **không có số liệu** ("Ối anh có đếm đâu em"). Dùng **Fermi estimation** từ dữ liệu gián tiếp:

```
Đầu vào đã biết:
  15 sân (cụm A: 6, cụm B: 5, cụm C: 4)
  Giờ mở cửa 05:00–23:00  →  18 slot/ngày/sân

Sức chứa lý thuyết:
  15 sân × 18 slot            =  270 slot-giờ/ngày
                              ≈ 8.100 slot-giờ/tháng

Tỉ lệ lấp đầy ước tính ~40%   ≈  108 lượt đặt/ngày
                              ≈ 3.240 lượt đặt/tháng
                              ≈ 40.000 đơn/năm

Cao điểm 17:00–23:00:
  6 slot × 15 sân = 90 slot, gần như kín
```

> **Kết luận kiến trúc quan trọng nhất rút ra từ đây:** tải **rất nhỏ**. Một instance PostgreSQL là quá đủ. **Không** sharding, **không** read replica, **không** Kafka, **không** microservices. Mọi đề xuất "cho hoành tráng" đều là over-engineering và sẽ bị từ chối.

---

## 2. Hiệu năng (Performance)

| Mã | Yêu cầu | Chỉ tiêu | Cách đo |
|---|---|---|---|
| **NFR-01** | Độ trễ API chung | **p95 < 300ms**, p99 < 800ms | Middleware đo thời gian, xuất metric |
| **NFR-02** | API xem lịch trống (hot path) | **p95 < 200ms** | Đây là API được gọi nhiều nhất |
| **NFR-03** | Tạo đơn (có ghi DB + Outbox) | p95 < 500ms | |
| **NFR-04** | Truy vấn báo cáo | p95 < 2s cho kỳ 1 tháng | Chấp nhận chậm hơn vì không phải hot path |
| **NFR-05** | Không có truy vấn N+1 trên đường đi chính | 0 | Kiểm tra bằng log số câu SQL/request |

## 3. Khả năng chịu tải (Scalability)

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-06** | Người dùng đồng thời lúc cao điểm | ~50 |
| **NFR-07** | Tỉ lệ đọc/ghi | Xem lịch trống gấp **20–50 lần** tạo đơn → đây là chỗ duy nhất đáng cache |
| **NFR-08** | Ứng dụng phải **stateless** để nhân bản ngang khi cần | Không lưu session trong bộ nhớ tiến trình |
| **NFR-09** | Thiết kế phải còn đúng khi chạy nhiều instance | Bất biến nghiệp vụ nằm ở tầng CSDL, không ở bộ nhớ ứng dụng |

## 4. Tính đúng đắn & Nhất quán (Correctness)

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-10** | ⭐ Không bao giờ có hai đơn hiệu lực trên cùng sân + khung giờ | **Tuyệt đối 0**, kể cả khi có bug tầng ứng dụng |
| **NFR-11** | Sự kiện nghiệp vụ không được mất khi RabbitMQ tạm ngừng | Outbox pattern, at-least-once |
| **NFR-12** | Webhook thanh toán xử lý trùng không gây sai lệch tiền | Idempotent |
| **NFR-13** | Số tiền không dùng kiểu dấu phẩy động | `numeric(14,2)` / `decimal` |

> ⚠️ **NFR-10 không phải yêu cầu hiệu năng.** Với 110 đơn/ngày, tải cực thấp — nhưng hai người bấm cách nhau 5ms vẫn ghi đè nhau nếu thiếu ràng buộc. Tải thấp chỉ làm **giảm tần suất**, không làm giảm **rủi ro**. Xem [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md).

## 5. Tính sẵn sàng (Availability)

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-14** | Uptime mục tiêu | ~**99%** (≈7h downtime/tháng). Không có SLA ràng buộc pháp lý |
| **NFR-15** | **Redis chết thì hệ thống vẫn phải chạy** | Cache miss → đọc thẳng CSDL. Redis là cache, **không** phải nguồn sự thật |
| **NFR-16** | RabbitMQ chết → đặt sân vẫn hoạt động | Message nằm lại trong bảng Outbox, đẩy sau |
| **NFR-17** | Cổng thanh toán lỗi → đơn không bị treo | Đơn hết hạn giữ chỗ và tự giải phóng |

## 6. Bảo mật (Security)

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-18** | Mật khẩu băm bằng thuật toán chậm có salt | BCrypt / Argon2 |
| **NFR-19** | Access Token thời hạn ngắn | **15 phút** — để thu hồi quyền có hiệu lực nhanh |
| **NFR-20** | Refresh Token lưu dạng **hash**, có xoay vòng, phát hiện tái sử dụng | |
| **NFR-21** | Không lộ dữ liệu giữa các tenant | Ép ở tầng hạ tầng (Global Query Filter), không phụ thuộc lập trình viên |
| **NFR-22** | Chống IDOR — kiểm tra phạm vi dữ liệu, không chỉ vai trò | 100% endpoint có dữ liệu thuộc chi nhánh |
| **NFR-23** | Log **không được** chứa mật khẩu, token, hoặc SĐT dạng thô | SĐT phải mask: `09xxxx1234` |
| **NFR-24** | Secrets không nằm trong mã nguồn hay Docker image | Biến môi trường / secret store |
| **NFR-25** | Giới hạn tần suất endpoint công khai | Đăng nhập: 5 lần/phút/IP; OTP: 3 lần/giờ/SĐT |
| **NFR-26** | Webhook phải xác thực chữ ký trước khi xử lý | Không tin payload chưa xác thực |

## 7. Khả năng bảo trì & Kiểm thử

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-27** | Độ phủ test cho tầng Domain + Application | **≥ 70%** |
| **NFR-28** | Mỗi `BR-xx` có ít nhất một test tham chiếu mã rule trong tên test | 100% |
| **NFR-29** | Integration test chạy trên **PostgreSQL thật** | Testcontainers — **cấm** EF Core InMemory |
| **NFR-30** | Tầng Domain **không** tham chiếu EF Core, ASP.NET, hay thư viện hạ tầng nào | Kiểm tra tự động bằng architecture test |
| **NFR-31** | Module chỉ giao tiếp qua interface hoặc event, không join thẳng bảng của nhau | Architecture test |

## 8. Khả năng quan sát (Observability)

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-32** | Log có cấu trúc (structured), không phải chuỗi ghép | Serilog + JSON |
| **NFR-33** | Mọi request có **CorrelationId** xuyên suốt log, audit và message | 100% |
| **NFR-34** | Health check phân biệt **liveness** và **readiness** | `/health/live`, `/health/ready` |
| **NFR-35** | Lỗi trả về theo chuẩn thống nhất | **RFC 7807 ProblemDetails** |

## 9. Vận hành & Dữ liệu

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-36** | Lưu dữ liệu giao dịch | **3 năm** |
| **NFR-37** | Backup CSDL hằng ngày, **và phải diễn tập restore ít nhất 1 lần** | Backup chưa từng restore = không có backup |
| **NFR-38** | Migration phải tương thích ngược (expand → migrate → contract) | Không `DROP COLUMN` cùng lúc deploy code mới |
| **NFR-39** | Toàn bộ hệ thống chạy được bằng một lệnh trên máy mới | `docker compose up` |

## 10. Trải nghiệm & Tương thích

| Mã | Yêu cầu | Chỉ tiêu |
|---|---|---|
| **NFR-40** | **~80% truy cập từ điện thoại** → thiết kế mobile-first | |
| **NFR-41** | Ngôn ngữ giao diện: tiếng Việt | Chuẩn bị cấu trúc i18n nhưng chưa đa ngữ ở v1 |
| **NFR-42** | Múi giờ hiển thị theo `Asia/Ho_Chi_Minh`; CSDL lưu **UTC** | |
| **NFR-43** | Trình duyệt hỗ trợ | 2 phiên bản gần nhất của Chrome, Safari, Edge |

---

## 11. Bảng đối chiếu NFR → Quyết định kiến trúc

Đây là bảng chứng minh **NFR không phải giấy tờ trang trí**:

| NFR | Dẫn tới quyết định |
|---|---|
| NFR-10 (không trùng lịch) | **Partial unique index** ở tầng CSDL → [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md) |
| NFR-07 (đọc gấp 20–50 lần ghi) | Redis **chỉ** cache lịch trống, không dùng để giữ chỗ |
| Tải 110 đơn/ngày | **Không** sharding, **không** read replica, **không** microservices |
| NFR-11 (không mất event) | **Outbox pattern** |
| NFR-12 (webhook trùng) | **Idempotency key** + unique constraint |
| NFR-19, NFR-22 (thu hồi quyền nhanh, chống IDOR) | Access token 15 phút + phân quyền theo phạm vi dữ liệu |
| NFR-21 (cách ly tenant) | Row-level `tenant_id` + **Global Query Filter** |
| NFR-29 (test trên DB thật) | **Testcontainers**, cấm InMemory provider |
| NFR-30, NFR-31 | **Architecture test** tự động trong CI |
