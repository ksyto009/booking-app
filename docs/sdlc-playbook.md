# Quy trình phát triển phần mềm — từ yêu cầu đến sản phẩm

> Bản đồ đầy đủ 7 giai đoạn, kèm **vai trò · đầu vào · đầu ra · tiêu chí hoàn thành · lỗi thường gặp**.
> Đây là quy trình thực tế của một đội sản phẩm, không phải waterfall trong sách giáo khoa.

---

## ⚠️ Đọc cái này trước: quy trình thật KHÔNG tuyến tính

Sơ đồ 21 bước xếp thẳng hàng là **cách trình bày**, không phải **cách thực thi**. Thực tế:

```
Giai đoạn 0–1 (Discovery + Requirements)   ──► làm 1 lần "vừa đủ", rồi cập nhật liên tục
Giai đoạn 2   (Design)                     ──► đi TRƯỚC build đúng 1 sprint, không thiết kế hết 1 lần
Giai đoạn 3–4 (Plan + Build)               ──► lặp mỗi sprint
Giai đoạn 5   (Release)                    ──► dựng 1 lần ở Sprint 0, sau đó chạy tự động mỗi lần merge
Giai đoạn 6   (Operate)                    ──► liên tục, đẻ ngược yêu cầu về giai đoạn 1
```

Hai nguyên tắc chi phối toàn bộ:

1. **Just Enough Up-Front Design (JEDUF).** Thiết kế đủ để không phải đập đi làm lại ở tuần thứ 5 — không hơn. Thiết kế thừa cũng lãng phí y như thiếu.
2. **Tài liệu là sản phẩm phụ của quyết định, không phải nghi lễ.** Viết tài liệu vì có người sẽ đọc nó (kể cả chính bạn 6 tháng sau). Tài liệu không ai đọc thì đừng viết.

---

## Giai đoạn 0 — Discovery (Khám phá)

| | |
|---|---|
| **Bước tương ứng** | 1–2 |
| **Vai trò chính** | Product Owner, Business Analyst |
| **Đầu vào** | Nhu cầu mơ hồ của khách hàng ("làm cho anh cái web đặt sân") |
| **Thời lượng điển hình** | 1–5% tổng dự án |

### Hoạt động
1. **Xác định stakeholder** — ai bỏ tiền, ai dùng, ai bị ảnh hưởng, ai có quyền phủ quyết
2. **Mô tả hiện trạng (as-is)** — hôm nay họ làm việc thế nào **khi chưa có** phần mềm
3. **Tìm vấn đề gốc** — dùng 5 Whys. Khách nói "cần web đặt sân"; vấn đề gốc là "trùng lịch + no-show mất doanh thu"
4. **Định nghĩa thành công đo được** — "giảm trùng lịch về 0", "giảm no-show từ 15% xuống 5%"
5. **Lập glossary sơ bộ** — thống nhất từ vựng: "slot", "cụm sân", "khách ruột" nghĩa là gì

### Đầu ra
- `vision.md` — vấn đề, đối tượng, thước đo thành công
- `stakeholders.md` — ai là ai, quan tâm điều gì
- `glossary.md` — từ điển nghiệp vụ *(sống suốt dự án)*

### ✅ Tiêu chí hoàn thành
Trả lời được 3 câu: **Ai đau? Đau ở đâu? Đo bằng con số nào?**

### ⚠️ Lỗi thường gặp
- Nhảy thẳng vào giải pháp mà chưa hiểu vấn đề
- Chỉ nói chuyện với người bỏ tiền, không nói chuyện với người **dùng thật** (nhân viên quầy)
- Bỏ qua glossary → 3 tháng sau mỗi người hiểu "booking" một kiểu

---

## Giai đoạn 1 — Requirements (Đặc tả yêu cầu)

| | |
|---|---|
| **Bước tương ứng** | 3–4 |
| **Vai trò chính** | Business Analyst, Solution Architect (cho NFR) |
| **Đầu vào** | Kết quả Discovery |
| **Thời lượng** | 5–10% |

### Hoạt động
1. **Elicitation vòng 2** — khai thác chi tiết: luồng chính, **luồng ngoại lệ**, quy tắc nghiệp vụ
2. **Viết Business Rules có mã** (`BR-01`…) — mỗi rule phải **kiểm chứng được**, không mơ hồ
3. **Ma trận phân quyền** — hàng = vai trò, cột = hành động, ô = phạm vi dữ liệu
4. **Yêu cầu phi chức năng (NFR) kèm SỐ** — tải, độ trễ, uptime, thiết bị, lưu trữ, bảo mật
5. **Phân loại phạm vi (MoSCoW)** — và ghi rõ **Won't have**, phần này quan trọng ngang Must
6. **Ghi giả định + rủi ro** — cái gì bạn tự chốt vì khách không trả lời được

### Đầu ra
- `requirements.md` — BR + phân quyền + NFR + MoSCoW + assumptions
- Cập nhật `glossary.md`

### ✅ Tiêu chí hoàn thành
- Mỗi BR có mã và **có thể viết được ít nhất một test** cho nó
- NFR có **con số**, không có chữ "nhanh", "ổn định", "nhiều người dùng"
- Danh sách **Won't have** tồn tại và đã được khách xác nhận

### ⚠️ Lỗi thường gặp
- NFR viết kiểu "hệ thống phải nhanh" → vô nghĩa, không kiểm chứng được
- Chỉ đặc tả happy path (chiếm ~30% code thật)
- Không có Won't have → phạm vi phình vô hạn
- Đặc tả **giải pháp** thay vì **yêu cầu**: "phải có nút xuất Excel" thay vì "chủ sân cần biết doanh thu tháng"

> 💡 **Mẹo:** khi khách không có số liệu, dùng **Fermi estimation** từ dữ liệu gián tiếp. Xem `01-requirements/requirements.md` §5.

---

## Giai đoạn 2 — Design (Thiết kế)

| | |
|---|---|
| **Bước tương ứng** | 5–9 |
| **Vai trò chính** | Solution Architect, Senior Engineer |
| **Thời lượng** | 10–15% |

Đây là giai đoạn có **giá trị phỏng vấn cao nhất**. Gồm 5 hoạt động, **theo đúng thứ tự này**:

### 2.1 Domain Model *(bước 5)*
Từ Business Rules → mô hình nghiệp vụ.

- **Entity** — có định danh, thay đổi theo thời gian (`Booking`)
- **Value Object** — không có định danh, bất biến (`Money`, `TimeSlot`)
- **Aggregate** — cụm object có **một** gốc, đảm bảo bất biến bên trong nó (`Booking` + `BookingSlot`)
- **Aggregate boundary** — 🔥 quyết định khó nhất. Quy tắc: *một transaction chỉ sửa một aggregate*
- **Domain Event** — chuyện đã xảy ra (`BookingConfirmed`)
- **Invariant** — điều kiện **luôn đúng** (BR-06)

→ Đầu ra: `domain-model.md`

### 2.2 Database Design *(bước 6)*
- Chọn **grain** cho từng bảng
- Chuẩn hoá → rồi **phá chuẩn có lý do được ghi lại**
- Ràng buộc: PK, FK, UNIQUE, CHECK, partial index
- Chiến lược index dựa trên **truy vấn thật**, không đoán
- Kiểu dữ liệu: tiền (`numeric`), thời gian (`timestamptz` UTC), khoá chính
- Soft delete, audit column, chiến lược migration

→ Đầu ra: `database-schema.md` + migration đầu tiên

### 2.3 API Design *(bước 7)*
**Contract-first**: thiết kế hợp đồng trước khi code.

- Tài nguyên và quan hệ (danh từ, không phải động từ)
- HTTP method + status code đúng ngữ nghĩa (`409` cho trùng lịch, không phải `400`)
- Định dạng lỗi thống nhất — **RFC 7807 ProblemDetails**
- Phân trang, lọc, sắp xếp
- **API versioning** — quyết định ngay từ đầu (`/api/v1/`)
- **Idempotency** cho các endpoint tạo/thanh toán

→ Đầu ra: `openapi.yaml` (hoặc sinh từ Swagger)

### 2.4 Architecture *(bước 8)*
Dùng **mô hình C4**, vẽ 3 mức là đủ:

| Mức | Trả lời câu hỏi | Cho ai xem |
|---|---|---|
| **C1 Context** | Hệ thống nói chuyện với ai bên ngoài? | Khách hàng, PO |
| **C2 Container** | Gồm những tiến trình/CSDL nào? | Cả đội |
| **C3 Component** | Bên trong một container có gì? | Dev |
| ~~C4 Code~~ | *Bỏ qua* — code chính là tài liệu | — |

Kèm: dependency rule của Clean Architecture, xử lý cross-cutting concerns.

→ Đầu ra: `architecture.md` + **ADR cho mọi quyết định lớn**

### 2.5 Module Boundary *(bước 9)*
Chia Modular Monolith. Nguyên tắc: **cắt theo nghiệp vụ, không cắt theo tầng kỹ thuật**.

❌ Sai: `Controllers`, `Services`, `Repositories`
✅ Đúng: `Identity`, `Catalog`, `Booking`, `Payment`, `Reporting`

Mỗi module: có schema/bảng riêng, giao tiếp với module khác **qua interface hoặc event**, không join thẳng bảng của nhau. Đây chính là "đường cắt" (seam) để sau này tách microservice.

→ Đầu ra: `module-map.md`

### ✅ Tiêu chí hoàn thành giai đoạn 2
- Đi được mạch **BR → Aggregate → Bảng → API endpoint** không đứt đoạn
- Mọi quyết định "tại sao chọn X thay vì Y" đều có **ADR**
- Biết trước **điểm gãy** của thiết kế: "cái này sẽ hỏng khi Z xảy ra"

### ⚠️ Lỗi thường gặp
- Vẽ hết mọi sơ đồ UML → 2 tuần sau lỗi thời, không ai đọc
- Thiết kế cho quy mô 1 triệu user khi đang có 100 → **over-engineering**
- Chia module theo tầng kỹ thuật → monolith rối, không tách được
- Không viết ADR → 3 tháng sau không ai nhớ vì sao làm thế

---

## Giai đoạn 3 — Planning (Lập kế hoạch)

| | |
|---|---|
| **Bước tương ứng** | 10–12 |
| **Vai trò chính** | Tech Lead, Product Owner |

### Hoạt động
1. **Product Backlog** — toàn bộ việc cần làm, xếp theo giá trị
2. **Chia Sprint** — nhóm story thành các lát cắt **chạy được**
3. **User Story** — `Là <vai trò>, tôi muốn <việc>, để <giá trị>`
4. **Acceptance Criteria** — dạng **Given / When / Then**, kiểm chứng được
5. **Task breakdown** — mỗi task < 1 ngày công
6. **Definition of Ready / Definition of Done**

### Ví dụ story đạt chuẩn

```
US-14: Đặt sân online có thanh toán trước

Là khách hàng, tôi muốn đặt sân và thanh toán online,
để chắc chắn giữ được sân trước khi tới nơi.

Acceptance Criteria:
  AC1 — Given sân 3 lúc 19:00 ngày mai còn trống
        When tôi đặt và thanh toán thành công
        Then đơn chuyển sang Confirmed và slot bị khoá        (BR-06, BR-10)

  AC2 — Given tôi tạo đơn nhưng không thanh toán
        When quá 10 phút
        Then đơn chuyển Expired và slot được giải phóng        (BR-11)

  AC3 — Given hai người cùng đặt sân 3 lúc 19:00 cùng lúc
        When cả hai gửi request
        Then đúng một người thành công, người kia nhận 409     (BR-06)

Definition of Done:
  ☑ Unit test cho domain logic
  ☑ Integration test với Testcontainers (bao gồm AC3)
  ☑ Log có CorrelationId
  ☑ Lỗi trả về đúng ProblemDetails
  ☑ Đã qua cross-cutting checklist
  ☑ docs/ đã cập nhật
```

### ⚠️ Lỗi thường gặp
- Story quá lớn ("làm module booking") → không đo được tiến độ
- AC viết kiểu "hoạt động đúng" → không kiểm chứng được
- Sprint chia theo **tầng** ("sprint này làm hết repository") → cuối sprint không có gì chạy được. Phải chia theo **lát cắt dọc**: mỗi sprint ra một tính năng hoàn chỉnh từ API xuống DB.

---

## Giai đoạn 4 — Build (Triển khai)

| | |
|---|---|
| **Bước tương ứng** | 13–17 |
| **Vai trò chính** | Engineer, Code Reviewer, QA |
| **Thời lượng** | 50–60% |

### Vòng lặp mỗi story

```
1. Viết test trước cho AC quan trọng nhất  (không cần TDD thuần, nhưng test AC3 phải có trước)
2. Code từ trong ra ngoài: Domain → Application → Infrastructure → API
3. Chạy cross-cutting checklist
4. Self-review diff của chính mình trước khi mở PR
5. Mở PR → Code review
6. CI xanh → merge
7. Cập nhật docs nếu quyết định thay đổi
```

### 🔴 Cross-cutting checklist — chạy trước khi đóng **mỗi** story

Đây là danh sách các mảng **dễ quên nhất**, và cũng là thứ phân biệt code sinh viên với code sản phẩm:

| # | Hạng mục | Câu hỏi tự kiểm |
|---|---|---|
| 1 | **Validation** | Input đã validate ở biên chưa? Domain có tự bảo vệ bất biến không? |
| 2 | **Authentication** | Endpoint này có cần đăng nhập? |
| 3 | **Authorization** | Đúng vai trò **và** đúng phạm vi dữ liệu chưa? Có IDOR không? |
| 4 | **Exception handling** | Lỗi có bị nuốt không? Có map sang HTTP status đúng không? |
| 5 | **Logging** | Có log điểm vào/ra? Có CorrelationId? **Có lỡ log dữ liệu nhạy cảm không?** |
| 6 | **Transaction** | Ranh giới transaction ở đâu? Có gọi API ngoài **bên trong** transaction không? |
| 7 | **Concurrency** | Hai người làm cùng lúc thì sao? |
| 8 | **Database index** | Truy vấn mới có index chưa? Đã xem `EXPLAIN ANALYZE` chưa? |
| 9 | **N+1 query** | Có `Include`/projection đúng chưa? |
| 10 | **Caching** | Có nên cache? Cache **invalidate** ở đâu? |
| 11 | **Rate limiting** | Endpoint công khai/tốn kém đã giới hạn chưa? |
| 12 | **Idempotency** | Gọi 2 lần có hỏng không? |
| 13 | **Audit log** | Hành động nhạy cảm đã ghi vết chưa? |
| 14 | **Soft delete** | Có ảnh hưởng unique constraint / FK không? |
| 15 | **API versioning** | Thay đổi này có **breaking** không? |
| 16 | **Test** | Unit cho domain, integration cho luồng có DB. Test có chạy trên DB thật không? |
| 17 | **Security** | Secrets có nằm trong code không? Input có bị SQL injection / mass assignment không? |
| 18 | **Documentation** | Swagger, `docs/`, ADR đã cập nhật chưa? |

### ⚠️ Lỗi thường gặp
- Test dùng **EF Core InMemory** → không có unique index, không có transaction thật → **test xanh giả**
- Gọi HTTP bên ngoài **bên trong** transaction DB → giữ khoá lâu, dễ timeout
- Log cả số điện thoại, token, mật khẩu
- "Sẽ thêm test sau" → không bao giờ có sau

---

## Giai đoạn 5 — Release (Đóng gói & Triển khai)

| | |
|---|---|
| **Bước tương ứng** | 18–20 |
| **Vai trò chính** | DevOps Engineer |

### Hoạt động
1. **Docker hoá** — multi-stage build, non-root user, health check, image nhỏ
2. **Môi trường** — `dev` / `staging` / `production`, cấu hình qua biến môi trường, secrets **không** nằm trong image
3. **CI pipeline** — build → test → phân tích tĩnh → quét bảo mật → đóng gói image
4. **CD pipeline** — deploy staging tự động, deploy production cần duyệt tay
5. **Chiến lược migration CSDL** — 🔥 mục dễ gây sự cố nhất:
   - Migration phải **tương thích ngược** (expand → migrate → contract)
   - Không bao giờ `DROP COLUMN` cùng lúc với deploy code mới
   - Luôn có đường **rollback**
6. **Smoke test sau deploy** + kế hoạch quay lui
7. **Monitoring & Alerting** — metric (Prometheus), log tập trung, cảnh báo có ngưỡng
8. **Backup & Restore** — 🔥 **và phải diễn tập restore ít nhất một lần.** Backup chưa từng restore được coi như không có backup.

### Đầu ra
- `Dockerfile`, `docker-compose.yml`
- `.github/workflows/*.yml`
- `deployment.md` — cách deploy, cách rollback
- `runbook.md` — xử lý sự cố thường gặp
- `observability.md` — SLI/SLO, dashboard, alert

### ⚠️ Lỗi thường gặp
- Chạy migration tự động lúc app khởi động với nhiều instance → đua nhau migrate
- Không có rollback plan
- Alert quá nhiều → cả đội tê liệt cảnh báo (alert fatigue), bỏ qua cả cảnh báo thật
- Chưa bao giờ thử restore backup

---

## Giai đoạn 6 — Operate & Evolve (Vận hành & Tiến hoá)

| | |
|---|---|
| **Bước tương ứng** | 21 |
| **Vai trò chính** | Toàn đội |

### Hoạt động
1. **Theo dõi SLI/SLO** — số liệu thật thay cho phỏng đoán
2. **Xử lý sự cố** → viết **Postmortem không đổ lỗi** (blameless): dòng thời gian, nguyên nhân gốc, hành động khắc phục
3. **Sổ nợ kỹ thuật (Technical Debt Register)** — ghi rõ: nợ gì, vì sao vay, lãi suất (nó làm chậm việc gì), khi nào trả
4. **Vòng phản hồi** — dữ liệu vận hành đẻ ra yêu cầu mới → quay về giai đoạn 1
5. **Vòng đời API** — thêm `v2`, đánh dấu `v1` deprecated, có lịch khai tử

### Đầu ra
- `postmortems/YYYY-MM-DD-<sự-cố>.md`
- `tech-debt.md`
- `CHANGELOG.md`
- ADR mới khi đổi quyết định *(ADR cũ đánh dấu `Superseded by ADR-00XX`, **không sửa nội dung**)*

---

## 📚 Bộ tài liệu trọn vẹn

Một dự án có tài liệu "đầy đủ" gồm đúng những file sau — không hơn:

| Tài liệu | Giai đoạn | Tần suất cập nhật | Bắt buộc? |
|---|---|---|---|
| `README.md` | — | Khi đổi cách chạy | ✅ |
| `00-process/sdlc-playbook.md` | — | Hiếm | 🔷 |
| `vision.md` | 0 | Gần như không | ✅ |
| `glossary.md` | 0 | Liên tục | ✅ |
| `stakeholders.md` | 0 | Hiếm | 🔷 |
| `01-requirements/requirements.md` | 1 | Khi khách đổi ý *(có version)* | ✅ |
| `02-design/domain-model.md` | 2 | Theo sprint | ✅ |
| `02-design/database-schema.md` | 2 | Theo migration | ✅ |
| `02-design/api-spec` (OpenAPI) | 2 | **Sinh tự động từ code** | ✅ |
| `02-design/architecture.md` (C4) | 2 | Hiếm | ✅ |
| `02-design/module-map.md` | 2 | Khi thêm module | 🔷 |
| `02-design/design-decisions.md` | 2 | Theo quyết định | 🔷 |
| `adr/NNNN-*.md` | 2+ | **Chỉ thêm, không sửa** | ✅ |
| `03-sprints/sprint-N.md` | 3–4 | Mỗi sprint | ✅ |
| `04-quality/test-strategy.md` | 4 | Hiếm | 🔷 |
| `04-quality/code-review-checklist.md` | 4 | Hiếm | 🔷 |
| `05-ops/deployment.md` | 5 | Khi đổi hạ tầng | ✅ |
| `05-ops/runbook.md` | 5 | Sau mỗi sự cố | ✅ |
| `05-ops/observability.md` | 5 | Khi thêm metric | 🔷 |
| `05-ops/security.md` (threat model) | 2/5 | Định kỳ | 🔷 |
| `CHANGELOG.md` | 5+ | Mỗi release | ✅ |
| `tech-debt.md` | 6 | Liên tục | ✅ |
| `postmortems/*.md` | 6 | Khi có sự cố | ✅ |

**Nguyên tắc vàng về tài liệu:**
> Tài liệu nào **không được cập nhật khi code đổi** thì tệ hơn là không có tài liệu — vì nó nói dối người đọc.
> Vì vậy: cái gì **sinh tự động được** (OpenAPI, ERD, CHANGELOG) thì đừng viết tay.

---

## 🎯 Ánh xạ 21 bước gốc → 7 giai đoạn

| Giai đoạn | Bước gốc |
|---|---|
| 0. Discovery | 1, 2 |
| 1. Requirements | 3, 4 |
| 2. Design | 5, 6, 7, 8, 9 |
| 3. Planning | 10, 11, 12 |
| 4. Build | 13, 14, 15, 16 |
| 5. Release | 17, 18, 19, 20 |
| 6. Operate | 21 |
