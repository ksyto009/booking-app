---
name: doc-glossary
description: Chuẩn viết và rà soát docs/00-glossary.md — từ điển nghiệp vụ dự án Court Booking. Dùng khi thêm thuật ngữ mới, khi phát hiện hai chỗ gọi cùng một thứ bằng hai tên khác nhau, hoặc khi đặt tên class/bảng/API.
---

# Skill: Từ điển nghiệp vụ (Glossary)

## Mục tiêu
Đảm bảo **một khái niệm = một tên duy nhất** ở mọi nơi: lời nói của khách hàng, tài liệu, tên class, tên bảng, tên endpoint.

## Vì sao quan trọng
Đây là nền tảng của **Ubiquitous Language** trong DDD. Từ vựng trôi dạt là nguyên nhân âm thầm của phần lớn hiểu lầm trong dự án — và nó không bao giờ tự lộ ra, nó chỉ tích tụ.

## Cấu trúc bắt buộc

| Phần | Nội dung |
|---|---|
| **1. Thuật ngữ nghiệp vụ** | Bảng 4 cột: `Thuật ngữ (VN)` · `Trong code` · `Định nghĩa` · `Đừng nhầm với` |
| **2. Thuật ngữ kỹ thuật** | Các khái niệm kỹ thuật dùng trong tài liệu (invariant, grain, idempotent…) |
| **3. Từ KHÔNG được dùng** | Bảng: `❌ Tránh` · `✅ Dùng thay` · `Vì sao` |

## Quy tắc chất lượng

1. Cột **"Đừng nhầm với"** là bắt buộc — chính chỗ này ngăn nhầm lẫn, không phải cột định nghĩa.
2. Cột **"Trong code"** phải là tên **thật** đang dùng. Nếu code khác glossary → sửa **code**, không sửa glossary.
3. Định nghĩa viết bằng **ngôn ngữ nghiệp vụ**, không dùng thuật ngữ kỹ thuật.
4. Mục §3 quan trọng ngang §1: liệt kê từ đồng nghĩa bị cấm để không ai vô tình dùng lại.
5. Thuật ngữ mới xuất hiện trong bất kỳ tài liệu nào → **phải** bổ sung vào đây.

## Checklist trước khi đóng

- [ ] Mọi Entity/Aggregate trong `07-domain-model.md` có mặt ở đây
- [ ] Mọi bảng chính trong `10-database-design.md` có mặt ở đây
- [ ] Không có thuật ngữ nào chỉ xuất hiện đúng một lần trong toàn bộ docs (dấu hiệu từ mồ côi)
- [ ] Không có hai dòng cùng cột "Trong code"
- [ ] Từ đồng nghĩa bị cấm đã nằm ở §3

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Định nghĩa vòng tròn ("Booking là một lần booking") | Vô nghĩa |
| Thêm từ mới mà không kiểm tra từ đồng nghĩa đã có | Hai tên cho một thứ |
| Glossary viết một lần rồi bỏ | Sau 3 sprint thành tài liệu nói dối |
| Định nghĩa bằng cách mô tả cấu trúc bảng | Đây là glossary nghiệp vụ, không phải data dictionary |

## Liên kết
`07-domain-model.md` (Ubiquitous Language) · `10-database-design.md` · `05-use-cases.md`
