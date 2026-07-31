---
name: doc-database-design
description: Chuẩn viết và rà soát docs/10-database-design.md — thiết kế CSDL dự án Court Booking (schema, DDL, index, ràng buộc, state machine). Dùng khi thêm bảng, viết migration, thiết kế index, hoặc khi rà soát ràng buộc toàn vẹn dữ liệu.
---

# Skill: Thiết kế cơ sở dữ liệu

## Mục tiêu
Thiết kế schema **bảo vệ được bất biến nghiệp vụ ở tầng dữ liệu**, không phó thác cho tầng ứng dụng.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Sơ đồ quan hệ** (mermaid `erDiagram`) |
| 2 | **Nhóm bảng theo module**, kèm DDL đầy đủ + comment giải thích cột không hiển nhiên |
| 3 | 🔒 **Ràng buộc bảo vệ bất biến sống còn** — tách riêng, giải thích kỹ |
| 4 | **Chiến lược index** — bảng: index · phục vụ truy vấn nào · ghi chú |
| 5 | **State machine** (mermaid `stateDiagram-v2`) cho entity có vòng đời |
| 6 | 🔴 **Những gì cố tình KHÔNG làm** + vì sao |

## Checklist thiết kế — chạy cho MỌI bảng mới

| # | Hạng mục | Câu hỏi |
|---|---|---|
| 1 | **Grain** | Một dòng đại diện cho **cái gì**? Phát biểu được thành một câu không? |
| 2 | **Khoá chính** | UUID v7 (sắp theo thời gian) hay bigint? Có lộ số lượng nghiệp vụ không? |
| 3 | **Ràng buộc** | PK, FK, UNIQUE, CHECK, NOT NULL đã đủ chưa? |
| 4 | **Bất biến** | Rule nào **phải** được CSDL bảo đảm, không phó thác cho app? |
| 5 | **Partial index** | Ràng buộc chỉ áp cho tập con? *(`WHERE is_active`, `WHERE deleted_at IS NULL`)* |
| 6 | **Kiểu tiền** | `numeric`, **không bao giờ** `float`/`double` |
| 7 | **Kiểu thời gian** | `timestamptz` lưu UTC, **không** `timestamp` |
| 8 | **Multi-tenant** | Có `tenant_id` chưa? Có nằm trong Global Query Filter chưa? |
| 9 | **Soft delete** | Có cần không? Nếu có, unique index đã thành **partial** chưa? |
| 10 | **Audit** | `created_at/by`, `updated_at/by` |
| 11 | **Snapshot** | Dữ liệu giao dịch có chốt giá trị tại thời điểm phát sinh không? |
| 12 | **Concurrency** | Cần `row_version` không? |
| 13 | **Index** | Truy vấn thật nào dùng bảng này? Đã `EXPLAIN ANALYZE` chưa? |
| 14 | **Kích thước** | Bao nhiêu dòng/năm? Có cần partition **chưa**? *(thường là chưa)* |

## Quy tắc chất lượng

1. 🔥 **Bất biến nghiệp vụ sống còn phải do CSDL bảo đảm.** Kiểm tra ở tầng ứng dụng có khe hở TOCTOU — luôn luôn.
2. **Chuẩn hoá là mặc định; phi chuẩn hoá là quyết định có lý do được GHI LẠI.** Mỗi chỗ phi chuẩn hoá phải giải thích được vì sao bắt buộc.
3. **Dữ liệu giao dịch là bất biến.** Giá, tên, địa chỉ tại thời điểm giao dịch phải được snapshot — không join lấy giá trị hiện tại.
4. **Partial index cho ràng buộc có điều kiện.** Vừa đúng nghiệp vụ, vừa nhỏ hơn nhiều.
5. **Index phải xuất phát từ truy vấn thật**, không đoán. Mỗi index ghi rõ phục vụ truy vấn nào.
6. **Soft delete có chọn lọc**: master data thì có, dữ liệu giao dịch thì không *(chúng đã có `status`)*.
7. **Mục §6 "cố tình không làm" là bắt buộc** — nó chống over-engineering và chứng minh bạn đã cân nhắc.

## Checklist trước khi đóng

- [ ] Mọi bảng qua đủ 14 mục checklist
- [ ] Bất biến sống còn có ràng buộc CSDL, không chỉ code
- [ ] Mọi phi chuẩn hoá có lý do ghi rõ
- [ ] Không có cột tiền dùng `float`; không có cột thời gian dùng `timestamp` không tz
- [ ] Mọi bảng nghiệp vụ có `tenant_id`
- [ ] State machine khớp với trạng thái trong `05-use-cases.md`
- [ ] Có mục "cố tình không làm"

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chỉ kiểm tra trùng ở tầng app | Race condition — dữ liệu sai |
| Unique index không partial khi có soft delete | Không tạo lại được bản ghi đã xoá mềm |
| Join lấy giá hiện tại cho đơn cũ | Doanh thu quá khứ tự thay đổi |
| Index bừa "cho chắc" | Chậm ghi, tốn dung lượng |
| Partition/sharding khi mới vài chục nghìn dòng | Over-engineering |
| Quên `tenant_id` một bảng | Rò rỉ dữ liệu giữa khách hàng |

## Liên kết
`07-domain-model.md` · `06-business-rules.md` · `16-decision-records/` · `15-testing-strategy.md`
