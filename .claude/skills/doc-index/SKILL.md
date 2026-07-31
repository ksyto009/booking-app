---
name: doc-index
description: Điều phối bộ tài liệu dự án Court Booking. Dùng khi người dùng hỏi "tài liệu nào còn thiếu", "giờ nên viết file nào", "kiểm tra tính nhất quán của docs", hoặc khi không rõ nên dùng skill doc-* nào.
---

# Skill: Điều phối tài liệu

## Mục tiêu
Xác định người dùng đang cần làm việc với tài liệu nào, kiểm tra tính toàn vẹn của bộ tài liệu, và chuyển sang skill chuyên biệt.

## Bước 1 — Đọc trạng thái
Đọc `docs/README.md` (bảng trạng thái) và liệt kê `docs/`. File có dấu `🚧 CHƯA VIẾT` là file chưa hoàn thành.

## Bước 2 — Ánh xạ sang skill

| Người dùng nhắc tới | Skill | File |
|---|---|---|
| thuật ngữ, từ vựng, đặt tên | `doc-glossary` | `00-glossary.md` |
| tổng quan, mục tiêu, stakeholder | `doc-project-overview` | `01-*` |
| nghiệp vụ, as-is/to-be, BRQ | `doc-business-requirements` | `02-*` |
| chức năng, FR, tính năng | `doc-functional-requirements` | `03-*` |
| hiệu năng, tải, bảo mật ở mức chỉ tiêu, NFR | `doc-nfr` | `04-*` |
| use case, kịch bản, luồng người dùng, UC | `doc-use-cases` | `05-*` |
| quy tắc nghiệp vụ, BR, phân quyền | `doc-business-rules` | `06-*` |
| domain, aggregate, entity, DDD | `doc-domain-model` | `07-*` |
| bối cảnh, hệ thống ngoài, C4 L1 | `doc-system-context` | `08-*` |
| kiến trúc, Clean Architecture, module, CQRS | `doc-architecture` | `09-*` |
| schema, bảng, index, migration | `doc-database-design` | `10-*` |
| API, endpoint, REST, OpenAPI | `doc-api-specification` | `11-*` |
| luồng, sequence, sơ đồ tuần tự | `doc-sequence-diagrams` | `12-*` |
| docker, CI/CD, deploy, backup | `doc-deployment` | `13-*` |
| bảo mật, STRIDE, auth, OWASP | `doc-security` | `14-*` |
| test, kiểm thử, coverage | `doc-testing-strategy` | `15-*` |
| quyết định kỹ thuật, ADR, "chọn X hay Y" | `doc-adr` | `16-decision-records/` |
| rủi ro, giả định | `doc-risk-analysis` | `17-*` |
| lộ trình, sprint, phạm vi, MoSCoW | `doc-roadmap` | `18-*` |
| sự cố, vận hành, khắc phục | `doc-runbook` | `19-*` |
| nợ kỹ thuật, TODO, tạm thời | `doc-tech-debt` | `20-*` |

## Bước 3 — Kiểm tra toàn vẹn (khi được yêu cầu rà soát)

Chuỗi truy vết bắt buộc: **`BRQ → FR → UC → BR → Test`**

| Kiểm tra | Vi phạm nghĩa là |
|---|---|
| Mọi `BRQ-xx` được phủ bởi ≥1 `FR-xx` | Yêu cầu nghiệp vụ bị bỏ quên |
| Mọi `FR-xx` ưu tiên 🔴 gắn với ≥1 `UC-xx` | Chức năng không có kịch bản |
| Mọi `BR-xx` được nhắc ở ≥1 use case hoặc FR | Rule mồ côi — hoặc thừa, hoặc chưa dùng |
| Mọi `NFR-xx` trỏ được về một quyết định kiến trúc | NFR trang trí |
| Mọi thuật ngữ trong docs có trong `00-glossary.md` | Từ vựng trôi dạt |
| Mọi quyết định lớn có ADR | Mất lý do |

## Quy tắc chung cho MỌI tài liệu

1. **Tiếng Việt**, thuật ngữ kỹ thuật giữ nguyên tiếng Anh.
2. **Ưu tiên bảng hơn đoạn văn.** Bảng ép ra cấu trúc; đoạn văn giấu chỗ mơ hồ.
3. **Mọi khẳng định phải kiểm chứng được.** Cấm "nhanh", "ổn định", "nhiều người dùng".
4. **Liên kết chéo bằng đường dẫn tương đối** tới file và mã tham chiếu.
5. **Cái gì sinh tự động được thì đừng viết tay** (OpenAPI, ERD, CHANGELOG).
6. **Ghi cả nhược điểm.** Tài liệu chỉ toàn ưu điểm là tài liệu nói dối.
7. Chỉ sửa **phần bị ảnh hưởng**, không viết lại cả file.
8. Sửa xong → cập nhật bảng trạng thái trong `docs/README.md`.
