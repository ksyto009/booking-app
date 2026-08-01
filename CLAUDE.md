# CLAUDE.md — Ngữ cảnh dự án

> File này được nạp tự động mỗi phiên làm việc. Đọc hết trước khi làm bất cứ việc gì.

---

## 1. Dự án này là gì

**Court Booking Platform** — hệ thống đặt sân thể thao theo khung giờ (cầu lông / pickleball).
Kiến trúc **Modular Monolith** trên .NET 9, thiết kế sẵn đường cắt để tách Microservices về sau.

**Trạng thái:** đang ở giai đoạn thiết kế. **Chưa có dòng code nào.** Tài liệu phân tích & thiết kế đã hoàn thành phần lớn.

**Ngôn ngữ làm việc:** tiếng Việt. Thuật ngữ kỹ thuật giữ nguyên tiếng Anh (aggregate, idempotent, grain, outbox…).

---

## 2. Nghiệp vụ cốt lõi — đủ để làm việc mà chưa cần đọc hết docs

**Bài toán:** Chủ sân có 3 cụm sân (15 sân), đang vận hành bằng sổ tay + Zalo. Ba nỗi đau: **trùng lịch** · **no-show mất doanh thu** · **không có dữ liệu để ra quyết định**.

**Vai trò:** Guest · Customer · Staff · BranchManager · **Partner** *(người góp vốn — chỉ đọc báo cáo chi nhánh được cấp)* · Owner · PlatformAdmin

**Bốn ràng buộc định hình toàn bộ thiết kế:**

| # | Ràng buộc | Hệ quả kỹ thuật |
|---|---|---|
| 1 | **BR-06** — một sân + một khung giờ ⇒ tối đa MỘT đơn hiệu lực | Partial unique index ở tầng CSDL ([ADR-0001](docs/16-decision-records/0001-booking-concurrency-strategy.md)) |
| 2 | Partner chỉ được xem chi nhánh mình góp vốn | **Data-scoped authorization**, không chỉ RBAC |
| 3 | Khách thuê định kỳ chiếm **~40% doanh thu** | Recurring booking **không phải** tính năng phụ |
| 4 | Muốn thu tiền trước nhưng không ép được khách ruột | Hai cờ **độc lập** `CanPayAtCounter` + `CanCancelLate`, thu hồi tự động (BR-12, BR-22, BR-35) |
| 5 | Slot = **30 phút**, căn mốc `:00`/`:30`; tối thiểu 60′ ở khung cao điểm | Chống phân mảnh lịch ([ADR-0002](docs/16-decision-records/0002-slot-grain-30-minutes.md), BR-01, BR-33) |
| 6 | Dời lịch phải **nguyên tử** — khách không bao giờ mất cả hai slot | Một transaction, dùng lại chính unique index của ADR-0001 ([ADR-0003](docs/16-decision-records/0003-atomic-reschedule.md), BR-37) |

**Quy mô thật (ước lượng Fermi):** ~110 lượt đặt/ngày, ~50 người đồng thời lúc cao điểm.
👉 **Tải rất nhỏ.** Không sharding, không read replica, không Kafka, không microservices. Mọi đề xuất "cho hoành tráng" đều bị từ chối.

**Định danh chung, không dùng tên riêng:** `Chủ sân` · `Đối tác góp vốn` · `Nhân viên quầy` · `Khách hàng` · `Cụm 1/2/3` · `Chủ sân khác`.

---

## 3. Nguyên tắc kỹ thuật bất di bất dịch

1. **Bất biến nghiệp vụ quan trọng phải do CSDL bảo đảm** — không phụ thuộc lập trình viên nhớ. Kiểm tra ở tầng ứng dụng luôn có khe hở TOCTOU.
2. **Dữ liệu giao dịch là bất biến.** Giá/tên/địa chỉ phải được snapshot tại thời điểm phát sinh, không join lấy giá trị hiện tại.
3. **Thêm hạ tầng = thêm một chế độ hỏng hóc.** Chỉ thêm khi lợi ích vượt cái giá đó. *(Đây là lý do Redis KHÔNG được dùng để giữ chỗ.)*
4. **Trong hệ phân tán: thiết kế cho at-least-once + consumer idempotent.** Đừng mơ exactly-once.
5. **Biết trước điểm gãy của thiết kế mình.** Nói được *"cái này sẽ hỏng khi X xảy ra"* giá trị hơn *"thiết kế này hoàn hảo"*.
6. **Mọi lựa chọn hạ tầng phải trỏ được về một NFR có số.** Không trỏ được → over-engineering, loại bỏ.

Mỗi quyết định kỹ thuật khi đưa ra phải kèm đủ: **vì sao · các phương án khác · trade-off · ưu điểm · nhược điểm · khi nào nên và khi nào KHÔNG nên dùng.**

---

## 4. Bản đồ tài liệu

Toàn bộ nằm trong `docs/`, đánh số phẳng 00→21. Xem [docs/README.md](docs/README.md).

**Đọc trước khi làm việc kỹ thuật:**
[06-business-rules.md](docs/06-business-rules.md) *(BR-01…32)* · [10-database-design.md](docs/10-database-design.md) · ⭐ [design-decisions.md](docs/design-decisions.md) *(13 quyết định + ~45 câu phỏng vấn)*

**Hệ thống mã — chuỗi truy vết `BRQ → FR → UC → BR → Test`:**

| Tiền tố | Nghĩa | File |
|---|---|---|
| `BRQ-xx` | Yêu cầu nghiệp vụ | 02 |
| `FR-xx` | Yêu cầu chức năng | 03 |
| `NFR-xx` | Yêu cầu phi chức năng | 04 |
| `UC-xx` | Use case | 05 |
| `BR-xx` | Quy tắc nghiệp vụ | 06 |
| `R-xx` / `A-x` | Rủi ro / Giả định | 17 |
| `TD-xx` | Nợ kỹ thuật | 20 |
| `CR-xx` | Đề nghị thay đổi | 21 |
| `ADR-xxxx` | Quyết định kiến trúc | 16 |

**23 skill `doc-*`** trong `.claude/skills/` — mỗi tài liệu một skill chuẩn hoá. Gõ `/doc-index` để điều phối và kiểm tra tính toàn vẹn của chuỗi truy vết.

---

## 5. Quy ước

| Hạng mục | Quy ước |
|---|---|
| **Thời gian** | CSDL luôn lưu **UTC** (`timestamptz`). Chuyển sang giờ VN ở tầng hiển thị. Code dùng `TimeProvider`, **cấm** `DateTime.Now` |
| **Tiền** | `numeric(14,2)` / `decimal`. **Không bao giờ** `float`/`double` |
| **Khoá chính** | UUID v7 (`Guid.CreateVersion7()`) |
| **Tên test** | Trích mã rule: `Booking_ShouldRejectOverlappingSlot_BR06()` |
| **Test CSDL** | 🔴 **Cấm EF Core InMemory.** Dùng Testcontainers PostgreSQL |
| **Tầng Domain** | Không tham chiếu EF Core / ASP.NET / thư viện hạ tầng nào |
| **Chia module** | Theo **nghiệp vụ** (`Identity`, `Catalog`, `Booking`, `Payment`, `Reporting`), **không** theo tầng kỹ thuật |
| **ADR** | Bất biến sau khi `Accepted`. Đổi ý → ADR mới + đánh dấu cũ `Superseded` |
| **Lỗi API** | RFC 7807 ProblemDetails. Trùng lịch = **409**, không phải 400 |
| **Tài liệu** | Cái gì sinh tự động được (OpenAPI, ERD, CHANGELOG) thì **đừng viết tay** |
| **Commit** | Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`, `test:`) |

---

## 6. Trạng thái & việc kế tiếp

### Đang chặn

| Việc | Vì sao chặn |
|---|---|
| 🔴 **[07-domain-model.md](docs/07-domain-model.md) chưa viết** | Chặn task S0-03 (migration đầu tiên). Bỏ qua sẽ dẫn tới **Anemic Domain Model** — nợ **TD-01**, rủi ro **R-11** |
| ✅ ~~CR-07 / CR-08a / CR-08b~~ | **Đã quyết định và triển khai vào tài liệu (2026-07-31).** Sinh ra ADR-0002, ADR-0003, BR-33, BR-34…BR-42, R-25…R-27. CR-01…06 đang hoãn — xem [21-change-requests.md](docs/21-change-requests.md) |
| 🟡 **`README.md` ở thư mục gốc chưa có** | GitHub đang hiển thị repo không có mô tả |

### Lộ trình

Sprint 0 (nền móng) → 1 (auth) → 2 (danh mục + phân quyền) → 🏆 **3 (đặt sân + chống trùng)** → 4 (thanh toán + Outbox) → 5 (định kỳ + messaging) → 6 (báo cáo + test) → 7 (CI/CD + deploy).
Chi tiết: [18-roadmap.md](docs/18-roadmap.md). Nhịp độ ~10–15h/tuần, sprint 1 tuần.

### Môi trường

.NET SDK 9.0.300 · Docker 29.5.3 · Node v22.17.1 · Git 2.49 · Windows 11 · PowerShell
