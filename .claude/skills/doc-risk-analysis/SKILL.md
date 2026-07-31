---
name: doc-risk-analysis
description: Chuẩn viết và rà soát docs/17-risk-analysis.md — phân tích rủi ro và giả định dự án Court Booking (ma trận xác suất × tác động, biện pháp giảm thiểu). Dùng khi bắt đầu giai đoạn mới, khi phát hiện rủi ro, hoặc khi rà soát cuối sprint.
---

# Skill: Phân tích rủi ro

## Mục tiêu
Nhận diện điều **có thể hỏng** trước khi nó hỏng, và chuẩn bị sẵn biện pháp.

## 🔴 Phân biệt Assumption và Risk

| | Assumption | Risk |
|---|---|---|
| **Là gì** | Điều tôi *giả định đúng* để đi tiếp | Điều *có thể xảy ra* và gây hại |
| **Ví dụ** | "Mọi chi nhánh cùng múi giờ" | "Cổng thanh toán có thể không phản hồi khi demo" |
| **Quản lý bằng** | Xác minh → đóng | P × I → biện pháp giảm thiểu |

Trộn hai thứ này là lỗi phổ biến. Assumption nằm ở mục riêng cuối file.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Rủi ro kỹ thuật** |
| 2 | **Rủi ro nghiệp vụ** |
| 3 | **Rủi ro dự án** |
| 4 | **Bảng nhiệt** (heat map) 3×3 |
| 5 | **Giả định** — bảng riêng, có trạng thái mở/đóng |
| 6 | **Quy trình theo dõi** |

Mỗi rủi ro: `Mã` · `Rủi ro` · `P` · `I` · `Điểm` · **`Biện pháp giảm thiểu`** · `Kế hoạch dự phòng`

**Thang điểm:** P và I từ 1–3. Điểm = P × I. **Ngưỡng hành động: ≥6** phải có biện pháp đã triển khai trước khi vào giai đoạn liên quan.

## Quy tắc chất lượng

1. **Rủi ro phải cụ thể và có thể xảy ra thật.** "Server có thể cháy" là vô ích. "Job sinh booking chạy 2 lần cùng lúc sinh dữ liệu trùng" là hữu ích.
2. **Biện pháp giảm thiểu phải là hành động, không phải mong muốn.**
   ❌ "cẩn thận hơn khi code" · ✅ "architecture test chặn tham chiếu InMemory trong CI"
3. **Phân biệt giảm thiểu (giảm P hoặc I) và dự phòng (làm gì khi nó đã xảy ra).** Cả hai đều cần cho rủi ro điểm cao.
4. **Phải có cả 3 nhóm.** Chỉ liệt kê rủi ro kỹ thuật là bỏ sót — rủi ro *dự án* (không đủ thời gian, tài liệu mục rữa) thường mới là thứ giết dự án cá nhân.
5. **Rủi ro điểm 9 phải được nêu bật** và xử lý trước tiên.
6. **Rà soát cuối mỗi sprint.** Rủi ro biến thành sự cố thật → viết postmortem + chuyển thành mục trong `20-tech-debt.md`.
7. **Assumption phải có mục "xác minh khi nào"** và trạng thái mở/đóng — nếu không nó sẽ nằm đó mãi.

## Checklist trước khi đóng

- [ ] Có đủ 3 nhóm: kỹ thuật, nghiệp vụ, dự án
- [ ] Mọi rủi ro ≥6 có biện pháp giảm thiểu **cụ thể, đã hoặc sẽ triển khai**
- [ ] Rủi ro điểm 9 được nêu bật riêng
- [ ] Biện pháp là hành động kiểm chứng được, không phải lời hứa
- [ ] Có bảng nhiệt
- [ ] Assumption tách riêng khỏi risk, có mốc xác minh và trạng thái
- [ ] Rủi ro trỏ tới tài liệu/ADR liên quan

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Trộn assumption vào bảng risk | Hai cách quản lý khác nhau bị lẫn lộn |
| Biện pháp chung chung ("review kỹ hơn") | Không ai làm, không ai kiểm được |
| Chỉ có rủi ro kỹ thuật | Bỏ sót thứ hay giết dự án nhất |
| Viết một lần rồi bỏ | Rủi ro thay đổi theo giai đoạn |
| Cho mọi rủi ro điểm cao | Mất khả năng ưu tiên |

## Liên kết
`01-project-overview.md` · `02-business-requirements.md` · `18-roadmap.md` · `20-tech-debt.md`
