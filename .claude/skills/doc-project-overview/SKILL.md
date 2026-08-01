---
name: doc-project-overview
description: Chuẩn viết và rà soát docs/01-project-overview.md — tổng quan dự án Court Booking (vấn đề, mục tiêu, stakeholder, ràng buộc). Dùng khi khởi tạo dự án, khi mục tiêu kinh doanh thay đổi, hoặc khi cần onboard người mới.
---

# Skill: Tổng quan dự án

## Mục tiêu
Trả lời trong **một trang** cho người mới hoàn toàn: hệ thống này giải quyết vấn đề gì, cho ai, đo thành công bằng gì, và bị ràng buộc bởi cái gì.

## Cấu trúc bắt buộc

| Phần | Nội dung | Quy tắc |
|---|---|---|
| **Metadata** | Tên, phiên bản, ngày, loại hệ thống, kiến trúc | |
| **1. Vấn đề** | Bảng: `Vấn đề` · `Nguyên nhân gốc` · `Thiệt hại` | Nguyên nhân gốc phải qua **5 Whys**, không dừng ở triệu chứng |
| **2. Mục tiêu** | `G1…Gn`, mỗi mục tiêu có **thước đo** | ❌ "cải thiện trải nghiệm" · ✅ "giảm no-show từ 15% → <5%" |
| **3. Phạm vi tổng thể** | Trong / Ngoài phạm vi | Phần "ngoài phạm vi" quan trọng ngang phần "trong" |
| **4. Stakeholder** | Bảng: `Bên liên quan` · `Vai trò` · `Quan tâm chính` · `Mức ảnh hưởng` | Phải có cả người **dùng** lẫn người **trả tiền** — họ khác nhau |
| **5. Ràng buộc** | Thời gian, nguồn lực, kỹ thuật, nghiệp vụ, ngân sách | |
| **6. Bối cảnh kỹ thuật** | Bảng tóm tắt + link sang tài liệu chi tiết | Chỉ tóm tắt, **không** giải thích ở đây |

## Quy tắc chất lượng

1. **Vấn đề trước, giải pháp sau.** Nếu §1 mô tả tính năng thay vì nỗi đau → viết lại.
2. **Mỗi mục tiêu phải đo được**, có giá trị hiện tại và giá trị mục tiêu.
3. **Stakeholder phải có người bị ảnh hưởng gián tiếp** — họ hay bị quên và hay tạo ràng buộc bất ngờ (ví dụ: Đối tác góp vốn chỉ được xem một chi nhánh).
4. Ràng buộc là **sự thật**, không phải mong muốn.
5. Không đưa chi tiết kỹ thuật vào đây — chỉ link.

## Checklist trước khi đóng

- [ ] Người ngoài đọc xong hiểu được hệ thống làm gì trong 3 phút
- [ ] Mọi mục tiêu `Gx` có thước đo bằng số
- [ ] Mọi `BRQ-xx` trong `02-*` truy ngược được về một mục tiêu `Gx`
- [ ] Có ít nhất một stakeholder "khó chịu" tạo ràng buộc thật
- [ ] Không có thuật ngữ nào chưa nằm trong `00-glossary.md`

## Lỗi thường gặp

| Lỗi | Cách sửa |
|---|---|
| Mục tiêu kiểu "xây hệ thống đặt sân" | Đó là **giải pháp**. Mục tiêu là kết quả kinh doanh mong muốn |
| Chỉ liệt kê người trả tiền làm stakeholder | Người dùng hằng ngày (nhân viên quầy) thường ảnh hưởng nhiều hơn |
| Nhồi kiến trúc vào tổng quan | Tổng quan là cho người **không** biết kỹ thuật |
| Phạm vi chỉ có "trong", không có "ngoài" | Không có ranh giới thì phạm vi phình vô hạn |

## Liên kết
`02-business-requirements.md` · `17-risk-analysis.md` · `18-roadmap.md`
