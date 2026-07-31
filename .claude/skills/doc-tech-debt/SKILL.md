---
name: doc-tech-debt
description: Chuẩn viết và rà soát docs/20-tech-debt.md — sổ nợ kỹ thuật dự án Court Booking. Dùng khi cố ý làm tắt để đi nhanh, khi phát hiện TODO/hack trong code, hoặc khi rà soát cuối sprint.
---

# Skill: Sổ nợ kỹ thuật

## Mục tiêu
Biến những chỗ "làm tạm" thành **khoản nợ có kỳ hạn**, thay vì để chúng biến mất khỏi trí nhớ.

> Nợ kỹ thuật **không xấu** — vay có ý thức để đi nhanh là quyết định hợp lệ.
> Cái xấu là **vay mà không ghi sổ**.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Cách ghi một khoản nợ** — giải thích 5 trường |
| 2 | **Nợ đang mở** — bảng đầy đủ |
| 3 | **Nợ đã trả** — lưu lại làm lịch sử |
| 4 | 🔴 **Nợ cố ý giữ lại (Won't fix)** |
| 5 | **Quy trình** rà soát |

Mỗi khoản nợ: `Mã` · `Nợ gì` · `Vì sao vay` · **`Lãi suất`** · `Điều kiện trả` · `Ước lượng` · `Trạng thái`

## Quy tắc chất lượng

1. 🔥 **Trường "lãi suất" là bắt buộc và là trường quan trọng nhất.** Nó trả lời: *khoản nợ này làm chậm việc gì, hoặc tăng rủi ro gì, mỗi ngày còn tồn tại?*
   → **Nợ không nêu được lãi suất thì không phải nợ kỹ thuật — đó là sở thích cá nhân. Đừng ghi vào đây.**
2. **"Vì sao vay" phải là lý do chính đáng tại thời điểm đó**, không phải "vì lười".
3. **"Điều kiện trả" phải là sự kiện cụ thể**, không phải "khi rảnh".
   ✅ *"Trước Sprint 0 task S0-03"* · *"Khi số dòng vượt 1 triệu"*
4. 🔴 **Mục "Nợ cố ý giữ lại" quan trọng ngang mục nợ đang mở.** Nó ghi lại những thứ **trông giống nợ nhưng thực ra là quyết định đúng ở quy mô hiện tại** — để người sau (hoặc chính bạn) không "sửa" nhầm và tạo ra over-engineering.
5. **Nợ đã trả thì chuyển mục, không xoá.** Lịch sử cho thấy đội có thật sự trả nợ hay chỉ tích luỹ.
6. **Rà soát cuối mỗi sprint**, dành ~10% thời lượng trả nợ 🔴 Cao.
7. **Nợ tồn quá 3 sprint** → hoặc trả, hoặc chuyển sang *Won't fix* kèm lý do. Không để lơ lửng.
8. **Nợ nghiêm trọng phải có mã rủi ro tương ứng** trong `17-risk-analysis.md`.

## Checklist trước khi đóng

- [ ] Mọi khoản nợ có "lãi suất" cụ thể
- [ ] Mọi khoản nợ có "điều kiện trả" là sự kiện, không phải thời gian mơ hồ
- [ ] Có mục "nợ cố ý giữ lại" với lý do
- [ ] Nợ 🔴 Cao có mã rủi ro tương ứng trong `17-*`
- [ ] Không có khoản nào tồn quá 3 sprint mà chưa quyết
- [ ] Nợ đã trả được chuyển mục, không xoá

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Ghi nợ không có lãi suất | Không biết ưu tiên cái nào |
| Điều kiện trả kiểu "khi có thời gian" | Không bao giờ trả |
| Để TODO trong code thay vì ghi vào sổ | Không ai thấy, không ai theo dõi |
| Không có mục "cố ý giữ lại" | Người sau "sửa" nhầm → over-engineering |
| Xoá nợ đã trả | Mất bằng chứng đội có kỷ luật |
| Ghi cả sở thích cá nhân ("code này xấu") | Sổ nợ loãng, mất tín nhiệm |

## Liên kết
`17-risk-analysis.md` · `18-roadmap.md` · `16-decision-records/`
