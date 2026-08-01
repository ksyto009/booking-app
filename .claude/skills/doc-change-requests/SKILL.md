---
name: doc-change-requests
description: Chuẩn viết và rà soát docs/21-change-requests.md — sổ Change Request dự án Court Booking (ghi nhận đề nghị thay đổi, phân tích tác động, quyết định, đánh đổi). Dùng khi khách hàng đề nghị thay đổi yêu cầu sau khi baseline đã chốt, hoặc khi quay lại xử lý CR đang hoãn.
---

# Skill: Sổ Change Request

## Mục tiêu
Đảm bảo **không thay đổi yêu cầu nào lọt thẳng vào code** mà chưa qua phân tích tác động và chưa nêu được cái phải cắt để đổi lấy nó.

> Thay đổi yêu cầu **không xấu** — nghiệp vụ luôn tiến hoá. Cái xấu là thay đổi **không ghi lại**, khiến phạm vi phình âm thầm cho tới lúc dự án trượt mà không ai truy được nguyên nhân.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Quy trình xử lý** + bảng trạng thái |
| 2 | **Sổ tổng hợp** — bảng mọi CR: mã · tiêu đề · người đề xuất · ngày · trạng thái · ưu tiên |
| 3 | **Đang phân tích** — chi tiết đầy đủ |
| 4 | **Hoãn** — ghi nhận kèm "cần đối chiếu" và câu hỏi mở |
| 5 | **Nhật ký quyết định** — ngày · CR · quyết định · người quyết · **đánh đổi** |

**Trạng thái:** `📥 Mới` · `🔍 Đang phân tích` · `⏸️ Hoãn` · `✅ Chấp nhận` · `🔄 Chấp nhận có sửa` · `❌ Từ chối` · `🚀 Đã triển khai`

## Mẫu một CR đang phân tích

```
Nguyên văn        — lời khách hàng, KHÔNG diễn giải lại
Tác động          — chạm vào tầng nào: tham số / rule / grain dữ liệu / kiến trúc
Xung đột phát hiện— đối chiếu với G-x, BRQ-xx, BR-xx, Won't-have
Phương án         — ≥2 kịch bản kèm chi phí so sánh
Khuyến nghị       — chọn cái nào, vì sao
Câu hỏi còn mở    — điều gì cần khách trả lời trước khi làm
Nếu chấp nhận     — danh sách tài liệu phải cập nhật + có cần ADR không
```

## Quy tắc chất lượng

1. 🔥 **Chấp nhận một CR thì phải nêu rõ CẮT GÌ để đổi lấy nó.** Nguồn lực không co giãn. Thêm việc mà không bớt việc là tự lừa mình — và là nguyên nhân số một khiến dự án trượt.
2. **Ghi nguyên văn lời khách hàng**, không diễn giải lại. Diễn giải làm mất thông tin, và khi tranh cãi sau này bạn cần bằng chứng.
3. **Bắt buộc đối chiếu với mục tiêu và Won't-have.** CR mâu thuẫn với một `G-x` đã chốt phải được **nêu thẳng**, không im lặng nhận rồi để mâu thuẫn nổ lúc code.
4. **Phân biệt CR sửa THAM SỐ với CR đổi GRAIN dữ liệu.** Đổi ngưỡng hoàn tiền là rẻ; đổi đơn vị inventory là đắt gấp nhiều lần. Đừng gộp hai loại vào một mức ưu tiên.
5. **Tách CR gộp.** Khách thường nói một câu chứa hai yêu cầu khác nhau *(ví dụ "hoàn tiền hoặc dời lịch" = đổi chính sách + tính năng mới)*. Tách ra mới ước lượng đúng.
6. **CR hoãn phải hoãn CÓ NGỮ CẢNH**, không chỉ có tiêu đề. Ghi sẵn "cần đối chiếu gì" và "câu hỏi mở" để khi quay lại không phải phân tích lại từ đầu.
7. **CR lật một quyết định cũ → bắt buộc viết ADR mới**, đánh dấu ADR cũ `Superseded`.
8. **Ghi cả CR bị từ chối.** Nếu không, 3 tháng sau khách đề nghị lại và cả đội phân tích lại từ đầu.
9. **Tìm mẫu chung giữa các CR.** Nhiều CR trông khác nhau nhưng cùng một dạng bài — nhận ra sớm thì giải một lần được cả nhóm.

## Checklist trước khi đóng

- [ ] Mọi CR có nguyên văn lời khách hàng
- [ ] Mọi CR đang phân tích có ≥2 phương án kèm chi phí
- [ ] Mọi xung đột với `G-x` / `BRQ-xx` / Won't-have được nêu thẳng
- [ ] CR được chấp nhận có ghi rõ **đánh đổi** trong nhật ký quyết định
- [ ] CR hoãn có "cần đối chiếu" + câu hỏi mở
- [ ] CR bị từ chối vẫn nằm trong sổ kèm lý do
- [ ] CR lật quyết định cũ đã có ADR tương ứng
- [ ] Đã cập nhật `18-roadmap.md` nếu phạm vi thay đổi

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Nhận CR mà không cắt gì | Trượt tiến độ, và không ai biết vì sao |
| Không ghi CR bị từ chối | Phân tích lại từ đầu khi khách đề nghị lần hai |
| Diễn giải lại lời khách | Mất thông tin gốc, mất bằng chứng khi tranh cãi |
| Gộp CR đổi tham số với CR đổi cấu trúc dữ liệu | Ước lượng sai nghiêm trọng |
| Hoãn CR chỉ với một dòng tiêu đề | Quay lại phải làm lại toàn bộ phân tích |
| Im lặng nhận CR mâu thuẫn với mục tiêu | Mâu thuẫn không biến mất — nó nổ lúc code hoặc lúc go-live |
| Sửa thẳng vào tài liệu baseline mà không qua CR | Mất dấu vết vì sao yêu cầu thay đổi |

## Liên kết
`01-project-overview.md` (mục tiêu G-x) · `02-business-requirements.md` (BRQ) · `06-business-rules.md` · `18-roadmap.md` (Won't have) · `16-decision-records/` · `17-risk-analysis.md`
