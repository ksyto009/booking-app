---
name: doc-system-context
description: Chuẩn viết và rà soát docs/08-system-context.md — sơ đồ bối cảnh C4 Level 1 dự án Court Booking (hệ thống ngoài, ranh giới tin cậy, chế độ suy giảm). Dùng khi tích hợp hệ thống bên thứ ba hoặc khi cần giải thích hệ thống cho người không kỹ thuật.
---

# Skill: Bối cảnh hệ thống (C4 Level 1)

## Mục tiêu
Trả lời **một** câu hỏi: *hệ thống nói chuyện với AI và CÁI GÌ bên ngoài?*
Đây là sơ đồ dành cho **khách hàng và người mới** — tuyệt đối không có chi tiết bên trong.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Sơ đồ Context** — một hộp duy nhất là hệ thống ta, xung quanh là người dùng và hệ thống ngoài |
| 2 | **Bảng mô tả tương tác** — ai gọi ai · dữ liệu gì · đồng bộ hay bất đồng bộ · giao thức |
| 3 | **Ranh giới tin cậy** — chỗ dữ liệu đi từ vùng không tin được vào vùng tin được |
| 4 | 🔴 **Chế độ suy giảm** — mỗi hệ thống ngoài chết thì ta còn làm được gì |

## Quy tắc chất lượng

1. **Chỉ MỘT hộp cho hệ thống của bạn.** Nếu vẽ container bên trong → đó là C4 Level 2, sai file.
2. **Mọi hệ thống ngoài phải có mục "khi nó chết thì sao".** Không trả lời được nghĩa là chưa thiết kế xong.
3. **Ghi rõ chiều dữ liệu** — ta gọi họ, hay họ gọi ta, hay cả hai. Chiều "họ gọi ta" (webhook) là chiều nguy hiểm nhất về bảo mật.
4. **Ranh giới tin cậy phải được đánh dấu.** Đây là đầu vào trực tiếp cho `14-security.md` — mọi thứ vượt ranh giới đều phải xác thực.
5. **Người không biết lập trình phải đọc hiểu được.** Nếu có chữ "repository", "middleware" → sai tầng trừu tượng.
6. Ghi cả hệ thống ngoài **dự kiến trong tương lai**, đánh dấu rõ là chưa có.

## Checklist trước khi đóng

- [ ] Đúng một hộp cho hệ thống của ta
- [ ] Mọi hệ thống ngoài có mô tả chế độ suy giảm
- [ ] Mọi tương tác ghi rõ chiều, giao thức, đồng bộ/bất đồng bộ
- [ ] Ranh giới tin cậy được đánh dấu
- [ ] Mọi vai trò trong `06-business-rules.md` §8 xuất hiện như một tác nhân
- [ ] Không có thuật ngữ kỹ thuật nội bộ

## Lỗi thường gặp

| Lỗi | Cách sửa |
|---|---|
| Vẽ lẫn container bên trong | Chuyển sang `09-architecture.md` |
| Bỏ qua chế độ suy giảm | Hệ quả: cổng thanh toán chết là cả hệ thống chết |
| Quên hệ thống ngoài "vô hình" (SMS, email, monitoring) | Chúng vẫn là phụ thuộc thật |
| Không phân biệt ta gọi họ vs họ gọi ta | Bỏ sót bề mặt tấn công của webhook |

## Liên kết
`09-architecture.md` · `13-deployment.md` · `14-security.md` · `04-non-functional-requirements.md`
