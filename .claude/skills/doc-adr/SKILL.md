---
name: doc-adr
description: Tạo và rà soát Architecture Decision Record trong docs/16-decision-records/ cho dự án Court Booking. Dùng ngay khi vừa chốt một quyết định kỹ thuật lớn, khi cân nhắc "chọn X hay Y", hoặc khi đảo ngược một quyết định cũ.
---

# Skill: Architecture Decision Record (ADR)

## Mục tiêu
Ghi lại **LÝ DO** của một quyết định — không ghi lại kết quả. Kết quả đã nằm trong code và schema.

> ADR là thứ cứu bạn 6 tháng sau khỏi câu *"sao hồi đó mình làm thế này nhỉ?"*, và là kho chất liệu trực tiếp cho buổi phỏng vấn.

## Khi nào viết ADR?

| ✅ Viết khi | ❌ Không viết cho |
|---|---|
| Quyết định khó đảo ngược | Quy ước đặt tên |
| Ảnh hưởng nhiều module | Thư viện nhỏ dễ thay |
| Chọn giữa nhiều phương án đều hợp lý | Chi tiết cài đặt trong một class |
| Người mới sẽ hỏi *"sao không làm cách kia?"* | Việc chỉ có một cách làm |

## Cấu trúc bắt buộc

```
Bảng metadata: Trạng thái · Ngày · Người quyết định · Liên quan (BR/NFR/ADR khác)

1. Bối cảnh          — tình huống, ràng buộc, số liệu liên quan
2. Vấn đề            — phát biểu chính xác cái cần giải quyết
3. Phương án đã cân nhắc  — 🔴 TỐI THIỂU 3, mỗi cái có bảng Ưu/Nhược + lý do loại
4. Quyết định        — chọn gì, kèm đoạn code/DDL minh hoạ nếu có
5. Lý do chọn        — đánh số, mỗi lý do trỏ về một ràng buộc/NFR thật
6. Hệ quả            — ✅ Tích cực VÀ ⚠️ Tiêu cực (bắt buộc có cả hai)
7. Kiểm chứng bằng test  — test nào chứng minh quyết định này đúng
8. Câu hỏi phỏng vấn liên quan
```

**Trạng thái:** `Proposed` → `Accepted` → `Superseded` / `Deprecated` / `Rejected`

## Quy tắc chất lượng

1. 🔥 **Tối thiểu 3 phương án, mỗi phương án có lý do bị loại cụ thể.** ADR chỉ có "tôi chọn X" là ADR vô giá trị — nó không chứng minh được là bạn đã cân nhắc.
2. **Mục "Hệ quả tiêu cực" là bắt buộc.** ADR chỉ toàn ưu điểm là ADR nói dối.
3. 🔥 **Phải nêu ĐIỂM GÃY ĐÃ BIẾT**: *"thiết kế này sẽ không còn phù hợp khi X xảy ra, lúc đó chuyển sang phương án Y"*. Đây là dấu hiệu rõ nhất của người có kinh nghiệm.
4. **Bất biến sau khi `Accepted`.** Đổi ý → viết ADR mới, đánh dấu ADR cũ `Superseded by ADR-00XX`. **Không sửa nội dung cũ** — lịch sử quyết định cũng là thông tin.
5. **Đánh số tăng dần, không tái sử dụng**, kể cả khi ADR bị `Rejected`.
6. **Một quyết định một file.** Đừng gộp.
7. **Lý do phải trỏ về số liệu thật** (NFR, tải, ràng buộc nghiệp vụ), không phải "vì best practice".

## Checklist trước khi đóng

- [ ] Có ≥3 phương án, mỗi phương án có bảng ưu/nhược
- [ ] Mỗi phương án bị loại có lý do cụ thể, không phải "không phù hợp"
- [ ] Có mục hệ quả **tiêu cực**
- [ ] Có nêu điểm gãy đã biết
- [ ] Có mục kiểm chứng bằng test
- [ ] Lý do chọn trỏ về NFR/BR có mã, không phải cảm tính
- [ ] Đã cập nhật bảng danh sách trong `16-decision-records/README.md`

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chỉ ghi phương án được chọn | Không chứng minh được đã cân nhắc; phỏng vấn hỏi là bí |
| Sửa ADR cũ khi đổi ý | Mất lịch sử quyết định |
| Viết ADR sau khi code xong 3 tháng | Lý do thật đã quên, chỉ còn hợp lý hoá |
| Lý do kiểu "vì đây là best practice" | Best practice của ai, trong hoàn cảnh nào, với tải bao nhiêu? |
| Không có hệ quả tiêu cực | Người đọc không tin |

## Liên kết
`09-architecture.md` · `10-database-design.md` · `04-non-functional-requirements.md` · `design-decisions.md`
