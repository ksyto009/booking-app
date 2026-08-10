# 📚 Tài liệu dự án — Court Booking Platform

Hệ thống đặt sân thể thao theo khung giờ (cầu lông / pickleball), kiến trúc **Modular Monolith** trên .NET 9, thiết kế sẵn để tách Microservices về sau.

---

## Bản đồ tài liệu

| # | Tài liệu | Nội dung | Trạng thái |
|---|---|---|:--:|
| 00 | [Glossary](00-glossary.md) | Từ điển nghiệp vụ — tên trong code phải trùng tên ở đây | ✅ |
| 01 | [Project Overview](01-project-overview.md) | Vấn đề, mục tiêu, stakeholder, ràng buộc | ✅ |
| 02 | [Business Requirements](02-business-requirements.md) | As-Is / To-Be, `BRQ-01…13`, chỉ số thành công | ✅ |
| 03 | [Functional Requirements](03-functional-requirements.md) | `FR-01…61` + ma trận truy vết | ✅ |
| 04 | [Non-Functional Requirements](04-non-functional-requirements.md) | `NFR-01…43` **có số** + ánh xạ sang quyết định kiến trúc | ✅ |
| 05 | [Use Cases](05-use-cases.md) | `UC-01…25`, đặc tả chi tiết 4 luồng khó nhất | ✅ |
| 06 | [Business Rules](06-business-rules.md) | `BR-01…32` + ma trận phân quyền | ✅ |
| 07 | [Domain Model](07-domain-model.md) | Aggregate, Value Object, Domain Event, bất biến xuyên aggregate | ✅ |
| 08 | [System Context](08-system-context.md) | C4 Level 1 | 🚧 |
| 09 | [Architecture](09-architecture.md) | C4 Level 2–3, Clean Architecture, chia module | 🚧 |
| 10 | [Database Design](10-database-design.md) | Schema, DDL, index, state machine | ✅ |
| 11 | [API Specification](11-api-specification.md) | Contract-first, REST, ProblemDetails | 🚧 |
| 12 | [Sequence Diagrams](12-sequence-diagrams.md) | 5 luồng lõi | 🚧 |
| 13 | [Deployment](13-deployment.md) | Docker, CI/CD, migration, backup | 🚧 |
| 14 | [Security](14-security.md) | STRIDE, auth, cách ly tenant | 🚧 |
| 15 | [Testing Strategy](15-testing-strategy.md) | Kim tự tháp test, Testcontainers | 🚧 |
| 16 | [Decision Records](16-decision-records/) | ADR — **lý do** của mọi quyết định lớn | ✅ 3/9 |
| 17 | [Risk Analysis](17-risk-analysis.md) | `R-01…24` + assumptions | ✅ |
| 18 | [Roadmap](18-roadmap.md) | MoSCoW + lộ trình Sprint 0–7 | ✅ |
| 19 | [Runbook](19-runbook.md) | Xử lý sự cố | 🚧 |
| 20 | [Tech Debt](20-tech-debt.md) | Sổ nợ kỹ thuật | ✅ |
| 21 | [Change Requests](21-change-requests.md) | Sổ đề nghị thay đổi yêu cầu — `CR-01…08` | ✅ |
| ⭐ | [Design Decisions](design-decisions.md) | **13 quyết định kỹ thuật + ~45 câu hỏi phỏng vấn** | ✅ |
| 📖 | [SDLC Playbook](sdlc-playbook.md) | Quy trình 7 giai đoạn từ yêu cầu tới vận hành | ✅ |

---

## Đọc theo thứ tự nào?

**Nếu bạn muốn hiểu nghiệp vụ:** `01 → 02 → 05 → 06`

**Nếu bạn muốn hiểu kỹ thuật:** `04 → 07 → 09 → 10 → ADR-0001`

**Nếu bạn đang ôn phỏng vấn:** ⭐ [design-decisions.md](design-decisions.md) — đọc phần câu hỏi ở cuối mỗi mục **trước**, tự trả lời ra giấy, rồi mới đọc lời giải.

**Nếu bạn sắp viết code:** `18-roadmap.md` §3 (Sprint 0) — nhưng **phải hoàn thành `07-domain-model.md` trước**.

---

## Hệ thống mã tham chiếu

| Tiền tố | Nghĩa | Ở đâu |
|---|---|---|
| `BRQ-xx` | Yêu cầu nghiệp vụ | 02 |
| `FR-xx` | Yêu cầu chức năng | 03 |
| `NFR-xx` | Yêu cầu phi chức năng | 04 |
| `UC-xx` | Use case | 05 |
| `BR-xx` | Quy tắc nghiệp vụ | 06 |
| `R-xx` / `A-x` | Rủi ro / Giả định | 17 |
| `TD-xx` | Nợ kỹ thuật | 20 |
| `CR-xx` | Đề nghị thay đổi yêu cầu | 21 |
| `ADR-xxxx` | Quyết định kiến trúc | 16 |

**Chuỗi truy vết:** `BRQ → FR → UC → BR → Test`
Mỗi mắt xích đứt là một yêu cầu bị bỏ quên.

---

## Quy ước

- **Thời gian** lưu trong CSDL luôn là **UTC** (`timestamptz`). Chuyển sang giờ Việt Nam ở tầng hiển thị.
- **Tiền** dùng `numeric(14,2)`, đơn vị VND. Không bao giờ dùng `float`/`double`.
- **Business rule** được trích dẫn trực tiếp trong tên test: `Booking_ShouldRejectOverlappingSlot_BR06()`
- **ADR** không sửa nội dung cũ. Đổi quyết định → ADR mới, ADR cũ đánh dấu `Superseded`.
- Cái gì **sinh tự động được** (OpenAPI, ERD, CHANGELOG) thì **đừng viết tay**.

---

## Skill chuẩn hoá tài liệu

Mỗi tài liệu có một skill riêng trong `.claude/skills/` để đảm bảo chất lượng và định dạng nhất quán.
Gõ `/doc-index` để xem danh sách, hoặc `/doc-<tên>` để làm việc với một tài liệu cụ thể.

---

## Trạng thái dự án

| Giai đoạn | Trạng thái |
|---|:--:|
| 0. Discovery | ✅ |
| 1. Requirements | ✅ |
| 2. Design | 🚧 40% *(schema + ADR xong; thiếu domain model, architecture, API)* |
| 3. Planning | ✅ *(roadmap + MoSCoW)* |
| 4. Build | ⬜ Chưa bắt đầu |
| 5. Release | ⬜ |
| 6. Operate | ⬜ |
