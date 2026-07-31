---
name: doc-business-requirements
description: Chuẩn viết và rà soát docs/02-business-requirements.md — yêu cầu nghiệp vụ dự án Court Booking (quy trình As-Is/To-Be, mã BRQ, chỉ số thành công). Dùng khi khai thác yêu cầu từ khách hàng hoặc khi nghiệp vụ thay đổi.
---

# Skill: Yêu cầu nghiệp vụ

## Mục tiêu
Trả lời **"vì sao doanh nghiệp cần hệ thống này"** bằng ngôn ngữ của khách hàng — không phải ngôn ngữ kỹ thuật.

> Ranh giới: file này nói **vì sao**; `03-functional-requirements.md` nói **hệ thống làm gì**. Nếu bạn đang viết "hệ thống phải có nút X" thì bạn đang viết nhầm file.

## Cấu trúc bắt buộc

| Phần | Nội dung |
|---|---|
| **1. Quy trình hiện tại (As-Is)** | Sơ đồ mermaid + bảng **điểm gãy** (`Pn`: điểm gãy · cơ chế gây lỗi) |
| **2. Quy trình mong muốn (To-Be)** | Sơ đồ mermaid + **điểm khác biệt cốt lõi** so với As-Is |
| **3. Yêu cầu nghiệp vụ** | Bảng `BRQ-xx` · yêu cầu · mục tiêu `Gx` · ưu tiên |
| **4. Quy tắc kinh doanh do khách đặt ra** | Bảng: nội dung · chi tiết · **trở thành rule nào** (`BR-xx`) |
| **5. Rủi ro nghiệp vụ** | Link sang `17-risk-analysis.md` |
| **6. Chỉ số đo lường thành công** | Bảng: chỉ số · hiện tại · mục tiêu sau N tháng |

## Quy tắc chất lượng

1. **Bắt buộc có As-Is.** Không mô tả được hiện trạng nghĩa là chưa hiểu nghiệp vụ — đang thiết kế trong chân không.
2. **Mỗi điểm gãy As-Is phải được một `BRQ-xx` xử lý.** Ánh xạ 1-1 là dấu hiệu phân tích tốt.
3. **Viết bằng ngôn ngữ khách hàng.** Nếu xuất hiện "index", "cache", "API" → sai file.
4. Mỗi `BRQ-xx` trỏ về một mục tiêu `Gx` ở `01-*`.
5. Ghi lại **nguyên văn** những câu khách hàng nói tạo ra ràng buộc lớn — chúng là bằng chứng khi tranh cãi phạm vi sau này.
6. **Mâu thuẫn trong lời khách hàng phải được nêu rõ, không được giấu.** Ví dụ: "thu tiền trước để hết no-show" vs "bắt khách ruột chuyển khoản thì kỳ" → nêu ra và chỉ rõ rule nào hoà giải.

## Checklist trước khi đóng

- [ ] Có sơ đồ As-Is **và** To-Be
- [ ] Mỗi điểm gãy As-Is có `BRQ-xx` tương ứng
- [ ] Mỗi `BRQ-xx` được phủ bởi ≥1 `FR-xx` (kiểm ở ma trận truy vết `03-*`)
- [ ] Mọi mâu thuẫn trong yêu cầu khách hàng đã được nêu và giải quyết
- [ ] Chỉ số thành công có **giá trị hiện tại** để so sánh
- [ ] Không có thuật ngữ kỹ thuật trong §1–§4

## Lỗi thường gặp

| Lỗi | Cách sửa |
|---|---|
| Chép lại lời khách hàng thành yêu cầu | Khách mô tả **giải pháp**; việc của bạn là tìm **vấn đề** phía sau |
| Bỏ qua As-Is vì "sắp bỏ rồi" | Quy trình cũ chính là đặc tả nghiệp vụ đang chạy |
| Giấu mâu thuẫn để tài liệu "sạch" | Mâu thuẫn không biến mất, nó nổ lúc code |
| Chỉ mô tả happy path | Luồng ngoại lệ chiếm ~70% công sức thật |

## Liên kết
`01-project-overview.md` · `03-functional-requirements.md` · `06-business-rules.md` · `17-risk-analysis.md`
