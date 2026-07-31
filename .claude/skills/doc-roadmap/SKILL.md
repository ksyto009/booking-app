---
name: doc-roadmap
description: Chuẩn viết và rà soát docs/18-roadmap.md — phạm vi MoSCoW và lộ trình sprint dự án Court Booking. Dùng khi lập kế hoạch sprint, khi khách hàng đề nghị thêm tính năng giữa chừng, hoặc khi cần cắt phạm vi.
---

# Skill: Lộ trình & Phạm vi

## Mục tiêu
Quyết định **làm gì trước, làm gì sau, và làm gì KHÔNG BAO GIỜ** — rồi bảo vệ quyết định đó khi bị đề nghị thêm việc.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Phạm vi MoSCoW** — Must / Should / Could / **Won't** |
| 2 | **Lộ trình Sprint** — bảng: sprint · chủ đề · story chính · **trọng tâm học** · trạng thái |
| 3 | **Chi tiết sprint kế tiếp** — task, ước lượng, Definition of Done |
| 4 | **Các mốc lớn** — mốc · nội dung · **chứng minh được điều gì** |
| 5 | **Sau v1** — hạng mục + **điều kiện kích hoạt** |
| 6 | **Quy trình theo dõi** |

## Quy tắc chất lượng

1. 🔥 **Chia sprint theo LÁT CẮT DỌC, không theo tầng kỹ thuật.**
   ❌ "Sprint này làm hết Repository" → cuối sprint không có gì chạy được, không demo được
   ✅ "Sprint này làm xong luồng đặt sân từ API xuống DB"
2. 🔴 **Mục "Won't have" quan trọng ngang "Must have"** — nó là tài liệu để nói "không" khi khách đề nghị thêm giữa chừng. Ghi cả **lý do** loại.
3. **Mỗi sprint phải ra được một thứ demo được.**
4. **Ước lượng theo giờ thật**, dựa trên năng lực thật, không dựa trên mong muốn.
5. **Mỗi sprint có "trọng tâm học"** — dự án này có mục tiêu kép: ra sản phẩm **và** ra năng lực.
6. **Mục "Sau v1" phải có ĐIỀU KIỆN KÍCH HOẠT**, không phải "khi rảnh". Điều kiện dạng: *"khi > 10 triệu dòng"*, *"khi có khách yêu cầu cách ly vật lý"*.
   → Nó ghi lại rằng các phương án "hoành tráng" đã được **cân nhắc và cố ý hoãn**, không phải bỏ sót.
7. **Sprint quan trọng nhất phải được đánh dấu** — với dự án này là sprint concurrency.
8. **Cập nhật trạng thái sau mỗi sprint.** Roadmap không cập nhật là roadmap nói dối.

## Checklist trước khi đóng

- [ ] Có đủ 4 nhóm MoSCoW, **kể cả Won't have kèm lý do**
- [ ] Mỗi sprint ra được sản phẩm demo được (lát cắt dọc)
- [ ] Sprint kế tiếp có task chi tiết + ước lượng + DoD
- [ ] Mỗi mốc nêu rõ "chứng minh được điều gì"
- [ ] Mục "Sau v1" có điều kiện kích hoạt cụ thể
- [ ] Có ghi điều kiện tiên quyết giữa các sprint (cái gì chặn cái gì)
- [ ] Trạng thái sprint đã cập nhật

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chia sprint theo tầng | Không demo được, không đo được tiến độ |
| Không có Won't have | Phạm vi phình vô hạn |
| Ước lượng theo mong muốn | Trượt liên tục, mất động lực |
| Lên kế hoạch chi tiết 6 sprint sau | Lãng phí — sprint 3 trở đi chắc chắn đổi |
| Bỏ qua điều kiện tiên quyết | Bắt đầu sprint rồi mới phát hiện bị chặn |
| Không cập nhật trạng thái | Tài liệu chết |

## Liên kết
`03-functional-requirements.md` · `17-risk-analysis.md` · `20-tech-debt.md` · `sdlc-playbook.md`
