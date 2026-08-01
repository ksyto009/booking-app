# 21 — Sổ Change Request

> **Mục đích:** mọi đề nghị thay đổi yêu cầu sau khi baseline đã chốt đều phải đi qua đây — không có thay đổi nào lọt thẳng vào code.
>
> **Nguyên tắc:** thay đổi yêu cầu **không xấu**. Cái xấu là thay đổi **không được ghi lại và không được phân tích tác động**, dẫn tới phạm vi phình âm thầm cho tới khi dự án trượt tiến độ mà không ai biết vì sao.

---

## Quy trình xử lý một CR

```
Đề xuất  →  Phân tích tác động  →  Quyết định  →  Triển khai
   │              │                    │              │
 ghi vào     đối chiếu G/BRQ/BR      Nhận/Sửa/     cập nhật BR, FR,
 sổ này      + ước lượng chi phí     Từ chối       UC, schema, ADR
```

**Trạng thái:**
`📥 Mới` · `🔍 Đang phân tích` · `⏸️ Hoãn` · `✅ Chấp nhận` · `🔄 Chấp nhận có sửa` · `❌ Từ chối` · `🚀 Đã triển khai`

**Quy tắc bắt buộc khi chấp nhận một CR:** phải nêu rõ **cắt gì để đổi lấy nó**. Nguồn lực là ~10–15h/tuần, không co giãn được. Thêm việc mà không bớt việc là tự lừa mình.

---

## Sổ tổng hợp

| Mã | Tiêu đề | Đề xuất bởi | Ngày | Trạng thái | Ưu tiên |
|---|---|---|---|:--:|:--:|
| **CR-01** | Ví trả trước (nạp tiền vào tài khoản) | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-02** | Mã khuyến mãi cho khung giờ thấp điểm | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-03** | Nhường/chuyển đơn cho người khác | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-04** | Đặt sân không cần đăng ký, trả sau | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-05** | Đưa nước / vợt / cầu vào hệ thống | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-06** | Danh sách chờ khi hết sân | Chủ sân | 2026-07-31 | ⏸️ Hoãn | Chưa xếp |
| **CR-07** | **Đổi đơn vị đặt sân: 30 phút / 1,5 tiếng** | Chủ sân | 2026-07-31 | ✅ **Chấp nhận** *(ADR-0002)* | 🔴 Cao |
| **CR-08a** | **Nới chính sách hủy — cho chọn hoàn tiền hoặc dời lịch** | Chủ sân | 2026-07-31 | 🔄 **Chấp nhận có sửa** | 🔴 Cao |
| **CR-08b** | **Cho phép dời lịch (reschedule)** | Chủ sân | 2026-07-31 | ✅ **Chấp nhận** *(ADR-0003)* | 🔴 Cao |

> 📌 CR-07, CR-08a, CR-08b đã được phân tích, quyết định và **triển khai vào tài liệu**. Chi tiết ở phần phân tích bên dưới và **Nhật ký quyết định** ở cuối file.

---

# ✅ Đã quyết định — lưu lại phân tích

> Phần dưới đây giữ nguyên **phân tích tại thời điểm ra quyết định**. Đừng sửa nó khi nghiệp vụ thay đổi lần sau — hãy mở CR mới. Lịch sử phân tích cũng là thông tin.

## CR-07 — Đổi đơn vị đặt sân sang 30 phút / 1,5 tiếng

**Nguyên văn:** *"Cho thuê luôn 30 phút và 1,5 tiếng."* — thay cho quy tắc cũ chỉ cho thuê theo giờ chẵn.

**Tác động:** 🔴 Chạm vào **grain của inventory** — loại thay đổi đắt nhất trong thiết kế CSDL. Đây đúng là *"điểm gãy đã biết"* được ghi trước tại [ADR-0001](16-decision-records/0001-booking-concurrency-strategy.md) §6, mục hệ quả tiêu cực #4.

**Hai kịch bản:**

| | **A — căn mốc `:00` / `:30`** | **B — giờ bắt đầu tự do** |
|---|---|---|
| Grain | Slot 30 phút rời rạc | Khoảng liên tục |
| Chống trùng | ✅ Giữ nguyên partial unique index | ❌ Đổi sang `EXCLUDE` + `tstzrange` |
| ADR-0001 | Còn hiệu lực | Bị lật |
| Chi phí | ~2 giờ | ~1–2 ngày + rủi ro |

**Khuyến nghị:** Kịch bản **A** — không ai đặt sân cầu lông lúc 18:17.

**Câu hỏi còn mở:**
1. Giá slot 30 phút = 1/2 giá giờ, hay tỉ lệ khác?
2. Thời lượng tối thiểu — 30 phút, hay 1 tiếng và 30 phút chỉ để nối thêm?
3. Thời lượng tối đa một lần đặt?

**Nếu chấp nhận, phải cập nhật:** BR-01 · thêm BR mới về min/max · `PriceRule` · [10-database-design.md](10-database-design.md) · [04-non-functional-requirements.md](04-non-functional-requirements.md) §1 · **viết ADR-0002**

---

## CR-08a — Nới chính sách hoàn tiền về 1 tiếng

**Nguyên văn:** *"Nếu khách báo trước 1 tiếng (hoặc cấu hình 30 phút) thì được hoàn tiền."*

**⚠️ Xung đột đã phát hiện:**

| Đã chốt | CR-08a |
|---|---|
| **G2** — giảm no-show 15% → <5% | Hủy trước 1 tiếng vẫn hoàn tiền ⇒ đặt sân trở lại gần như **miễn phí về mặt kinh tế** |
| **BRQ-02** — thanh toán trước để ràng buộc trách nhiệm | Mất tác dụng ràng buộc |
| **BR-16** — hủy <4h hoàn 0% | Bị thay thế hoàn toàn |

Slot 19:00 bị hủy lúc 18:00 gần như **không thể bán lại** — khách đã có kế hoạch từ trước.

**💡 Phương án dung hoà đề xuất:** tách **dời lịch** khỏi **hoàn tiền**

| Hành động | Cửa sổ | Lý do |
|---|---|---|
| Dời lịch | Trước 1 tiếng *(cấu hình theo tenant)* | Doanh thu **giữ nguyên** |
| Hoàn tiền | Giữ bậc cũ: ≥24h → 100% · 4–24h → 50% · <4h → 0% | Bảo vệ doanh thu giờ cao điểm |

**Cấu hình ở cấp nào:** theo **tenant** *(mỗi chủ sân một chính sách)*, không phải toàn hệ thống.

---

## CR-08b — Cho phép dời lịch (Reschedule)

**Đây là tính năng hoàn toàn mới**, không phải chỉ sửa tham số.

**🔴 Ràng buộc kỹ thuật cốt lõi — hoán đổi nguyên tử:**

```
Dời 19:00 → 20:00:
  Hủy slot cũ VÀ chiếm slot mới trong CÙNG một transaction.
  Nếu tách rời: hủy xong mà slot mới bị người khác cướp
  → khách mất cả hai. Không chấp nhận được.
```

**7 câu hỏi nghiệp vụ chưa có lời đáp:**

| # | Câu hỏi | Vì sao quan trọng |
|---|---|---|
| 1 | Dời tối đa mấy lần? | Không giới hạn = giữ chỗ vô hạn miễn phí |
| 2 | Slot mới giá khác thì sao? | Bù thêm hay giữ giá cũ? |
| 3 | Dời sang ngày khác / chi nhánh khác được không? | |
| 4 | Dời sang sân khác được không? | |
| 5 | Dời có tính vào thống kê no-show không? | Ảnh hưởng BR-22 (thu hồi `IsTrusted`) |
| 6 | Đơn định kỳ dời một buổi được không? | Đụng BR-26 |
| 7 | Slot mới có phải cách hiện tại ≥1 tiếng? | Tránh lách chính sách |

---

# ⏸️ Hoãn — xử lý sau

> Các CR dưới đây đã được ghi nhận nhưng **chưa phân tích tác động**. Khi quay lại, bắt đầu từ cột "Cần đối chiếu" — đó là nơi tôi đã đánh dấu sẵn chỗ cần kiểm tra.

## CR-01 — Ví trả trước

**Nguyên văn:** Khách nạp tiền vào tài khoản *(nạp 2 triệu tặng 200 nghìn)*, mỗi lần đặt sân trừ dần. Hủy đơn hoàn về ví. **Không cho rút tiền mặt.**

| | |
|---|---|
| **Cần đối chiếu** | Bài toán trừ số dư đồng thời — **cùng bản chất với BR-06**, chỉ khác vỏ. Hai request cùng trừ ví một lúc = double-spend |
| **Câu hỏi mở** | Tiền tặng thêm có được hoàn không? · Ví có hạn sử dụng? · Đối soát sổ sách ra sao? · Tenant khác nhau có dùng chung ví không? |
| **Ảnh hưởng dự kiến** | Bảng `wallet` + `wallet_transaction`, ràng buộc số dư không âm, luồng hoàn tiền rẽ nhánh |

## CR-02 — Mã khuyến mãi

**Nguyên văn:** Mã `GIOTRONG30` giảm 30% cho khung 9h–16h ngày thường. Mỗi khách tối đa 2 lần/tháng. Tổng phát 200 lượt.

| | |
|---|---|
| **Cần đối chiếu** | *"Tổng 200 lượt"* và *"2 lần/khách"* đều là **bộ đếm có giới hạn** → lại là bài toán tranh chấp đồng thời |
| **Câu hỏi mở** | Hủy đơn có trả lại lượt dùng không? · Cộng dồn với giảm giá định kỳ 15% (BR-23) không? · Ai được tạo mã? |
| **Ảnh hưởng dự kiến** | Bảng `promotion` + `promotion_usage`, thay đổi logic tính giá |

## CR-03 — Nhường / chuyển đơn

**Nguyên văn:** Khách tự chuyển đơn sang một số điện thoại khác.

| | |
|---|---|
| **Cần đối chiếu** | Đơn đã trả tiền — chuyển đơn thì tiền thuộc về ai? Người nhận có phải trả lại người chuyển không, hay hai bên tự lo? |
| **Câu hỏi mở** | Người nhận chưa có tài khoản thì sao? · Chuyển rồi có hủy được không, ai hủy? · Có tính vào no-show của ai nếu không đến? |
| **Ảnh hưởng dự kiến** | Đổi chủ sở hữu đơn + audit log bắt buộc (BR-32) |

## CR-04 — Đặt không cần đăng ký, trả sau

**Nguyên văn:** Khách lớn tuổi ngại đăng ký — chỉ cần nhập tên + SĐT là đặt được, trả tiền tại sân.

| | |
|---|---|
| **⚠️ Cần đối chiếu** | **G2** (giảm no-show) · **BRQ-02** (thanh toán trước để ràng buộc) · **BR-10** · **BR-12** — hãy kiểm tra kỹ trước khi quyết |
| **Câu hỏi mở** | Có cách nào đạt được mục tiêu của Chủ sân *(khách lớn tuổi đặt được dễ dàng)* mà **không** phá cơ chế chống no-show không? |
| **Gợi ý hướng suy nghĩ** | Nhớ rằng đã có UC-08 *(nhân viên đặt hộ)* và BR-12 *(`IsTrusted`)* — hai cơ chế này có sẵn để giải quyết cùng nỗi đau |

## CR-05 — Nước / vợt / cầu

**Nguyên văn:** *"Hồi trước tôi bảo thôi, nhưng giờ nghĩ lại thấy nên có."*

| | |
|---|---|
| **⚠️ Cần đối chiếu** | [18-roadmap.md](18-roadmap.md) §1 — mục **Won't have**. Đây là hạng mục **chính khách hàng đã tự cắt**, nay quay lại |
| **Câu hỏi mở** | Nếu nhận thì **cắt gì để đổi lấy**? · Có phải chỉ là ghi nhận bán hàng, hay kéo theo cả quản lý kho? |
| **Ghi chú** | Đây là ví dụ điển hình của **scope creep** — nhận nó thì phải nhận cả hệ quả |

## CR-06 — Danh sách chờ (Waitlist)

**Nguyên văn:** Khách đăng ký chờ một khung giờ. Có người hủy thì hệ thống báo cho người chờ đầu tiên, ai nhanh tay thì được.

| | |
|---|---|
| **Cần đối chiếu** | Khi slot được giải phóng, nhiều người chờ cùng nhận thông báo → **lại là tranh chấp đồng thời**. Có giữ chỗ ưu tiên cho người đầu hàng không, giữ bao lâu? |
| **Câu hỏi mở** | Thông báo qua kênh nào? · Xếp theo thứ tự đăng ký hay ai nhanh tay? · Một người chờ được mấy slot? |
| **Ghi chú** | 🔗 Liên quan chặt với **CR-08b** — dời lịch cũng giải phóng slot, cùng cơ chế |

---

## 📌 Quan sát xuyên suốt

**4 trong 6 CR đang hoãn (CR-01, CR-02, CR-06, và cả CR-08b) đều chứa một bài toán tranh chấp đồng thời — cùng bản chất với BR-06, chỉ khác vỏ ngoài.**

Đây không phải trùng hợp. Mọi hệ thống có **tài nguyên hữu hạn được nhiều người tranh giành** đều quy về cùng một dạng bài: slot sân, số dư ví, lượt khuyến mãi, suất trong hàng chờ. Giải được một cái là giải được cả nhóm.

Ghi lại quan sát này — nó là câu trả lời rất mạnh khi phỏng vấn hỏi *"em học được gì từ dự án?"*

---

## Nhật ký quyết định

| Ngày | CR | Quyết định | Người quyết | Đánh đổi |
|---|---|---|---|---|
| 2026-07-31 | CR-01…06 | ⏸️ Hoãn — ghi nhận, chưa phân tích tác động | Chủ dự án | Không ảnh hưởng phạm vi v1 |
| 2026-07-31 | **CR-07** | ✅ **Chấp nhận — kịch bản A** (grain 30′, căn mốc `:00`/`:30`) · giá 30′ = 50% giá giờ, cấu hình theo tenant · **thời lượng tối thiểu 60′ cao điểm / 30′ thấp điểm**, tối đa 240′ | Chủ dự án | Không phải cắt gì — chi phí ~2h sửa tài liệu. Đổi lại nhận thêm rủi ro **R-25** (phân mảnh lịch), đã có BR-33 giảm thiểu |
| 2026-07-31 | **CR-08a** | 🔄 **Chấp nhận có sửa** — khách **tự chọn** hủy hoặc dời, hệ thống xử lý **tự động** · giữ nguyên bậc hoàn tiền BR-16 · `BranchManager` **ghi đè** được (ngoại lệ, không phải duyệt từng đơn) · **mọi khách đều hủy được**, khách thân thiết hưởng **ưu đãi thêm** · tách `IsTrusted` → `CanPayAtCounter` + `CanCancelLate` | Chủ dự án | Bỏ phương án "quản lý duyệt từng ca" *(sẽ phá mục tiêu **G3**)* và bỏ phương án "chỉ khách thân thiết mới được hủy" *(chặn nhầm khách mới)*. "Trừ cọc" = chính bậc BR-16, **không** thêm khái niệm cọc riêng |
| 2026-07-31 | **CR-08b** | ✅ **Chấp nhận — dời lịch NGUYÊN TỬ** (không dùng "hủy rồi đặt lại") · tối đa 2 lần/đơn · bù tiền khi đắt hơn, không hoàn khi rẻ hơn · ngày khác ✅ sân khác ✅ chi nhánh khác ❌ · không tính vào no-show | Chủ dự án | **+3–4 giờ** code cho một command handler mới. Đổi lại: khách không bao giờ mất cả hai slot, và giữ được doanh thu thay vì hoàn ra |

### Ghi chú về CR-08b

Đánh giá ban đầu của Solution Architect là *"dời lịch đắt hơn vẻ ngoài"* — **nhận định đó đã được rút lại** sau khi rà lại schema. Với cấu trúc `booking_slot` sẵn có, hoán đổi nguyên tử chỉ cần một transaction và **dùng lại đúng partial unique index của ADR-0001**, không thêm cơ chế nào. Ngược lại, phương án "hủy rồi đặt lại" tưởng đơn giản nhưng đẩy rủi ro mất cả hai slot sang khách hàng.

---

## Đã triển khai vào tài liệu

| Tài liệu | Thay đổi |
|---|---|
| [00-glossary.md](00-glossary.md) | Slot = 30′ · thêm `CanPayAtCounter`, `CanCancelLate`, `Reschedule`, `RefundOverride`, "phân mảnh lịch" · đánh dấu `IsTrusted` không dùng nữa |
| [03-functional-requirements.md](03-functional-requirements.md) | Thêm **FR-62…FR-67** |
| [05-use-cases.md](05-use-cases.md) | UC-12 thêm bước chọn hủy/dời · thêm **UC-26** (dời lịch), **UC-27** (ghi đè hoàn tiền) |
| [06-business-rules.md](06-business-rules.md) | Sửa BR-01, BR-12, BR-14, BR-22 · thêm **BR-33, BR-14b, BR-34…BR-42** · cập nhật ma trận phân quyền |
| [10-database-design.md](10-database-design.md) | `tenant` +4 cột tham số · `price_rule` +min/max duration · `customer_profile` tách 2 cờ · `booking` +reschedule & refund override |
| [16-decision-records/](16-decision-records/) | **ADR-0002** (grain 30′) · **ADR-0003** (dời lịch nguyên tử) |
| [17-risk-analysis.md](17-risk-analysis.md) | Thêm **R-25** (phân mảnh lịch), **R-26** (lạm dụng dời lịch), **R-27** (lạm dụng ghi đè) |
