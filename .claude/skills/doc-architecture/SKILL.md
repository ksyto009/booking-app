---
name: doc-architecture
description: Chuẩn viết và rà soát docs/09-architecture.md — kiến trúc C4 Level 2-3 dự án Court Booking (Clean Architecture, chia module, CQRS, cross-cutting). Dùng khi thiết kế cấu trúc solution, khi chia module, hoặc khi cân nhắc tách microservice.
---

# Skill: Kiến trúc hệ thống

## Mục tiêu
Mô tả **cấu trúc bên trong** và — quan trọng hơn — **vì sao chọn cấu trúc đó thay vì cấu trúc khác**.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Quyết định kiến trúc tổng thể** + vì sao **không** chọn phương án phổ biến khác |
| 2 | **C4 Level 2 — Container**: bảng container · công nghệ · trách nhiệm |
| 3 | **C4 Level 3 — Component**: tầng, **quy tắc phụ thuộc**, mỗi tầng chứa gì / KHÔNG chứa gì |
| 4 | **Chia module** theo nghiệp vụ + quy tắc giao tiếp giữa module |
| 5 | **CQRS** — tách ở tầng nào, vì sao đủ ở quy mô này |
| 6 | **Cross-cutting** — mỗi mối quan tâm được cài ở đâu |
| 7 | **Đường cắt sang Microservices** — module nào tách trước, **và vì sao hiện tại KHÔNG tách** |
| 8 | **Sơ đồ triển khai** → link `13-deployment.md` |

## Quy tắc chất lượng

1. 🔥 **Chia module theo NGHIỆP VỤ, không theo tầng kỹ thuật.**
   ❌ `Controllers/`, `Services/`, `Repositories/` → không bao giờ tách được microservice
   ✅ `Identity/`, `Catalog/`, `Booking/`, `Payment/`, `Reporting/`
2. **Quy tắc phụ thuộc phải phát biểu rõ VÀ có architecture test kiểm chứng.** Quy tắc không có test là quy tắc sẽ bị vi phạm.
3. **Mỗi lựa chọn công nghệ phải trỏ về một NFR có số** ở `04-*`. Không trỏ được → over-engineering.
4. **Phải ghi rõ vì sao KHÔNG chọn phương án khác.** Kiến trúc không có phương án bị loại là kiến trúc chưa được cân nhắc.
5. **Module giao tiếp qua interface công khai hoặc domain event**, không join thẳng bảng của nhau.
6. **Nêu điểm gãy đã biết**: "kiến trúc này sẽ không còn phù hợp khi X xảy ra".
7. Quyết định lớn → tạo ADR riêng, ở đây chỉ tóm tắt và link.

## Checklist trước khi đóng

- [ ] Quy tắc phụ thuộc có architecture test tương ứng
- [ ] Module chia theo nghiệp vụ, đặt tên bằng ngôn ngữ nghiệp vụ
- [ ] Mỗi container trỏ được về một NFR
- [ ] Có mục "vì sao không làm microservices/CQRS tách DB/..."
- [ ] Cross-cutting liệt kê đủ: validation, authz, logging, exception, transaction, caching, outbox
- [ ] Có nêu điểm gãy đã biết
- [ ] Mọi quyết định lớn có ADR

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chia module theo tầng kỹ thuật | Monolith rối, không tách được |
| Chọn microservices vì "hiện đại" | Phức tạp phân tán mà không có nhu cầu |
| Quy tắc phụ thuộc chỉ nằm trên giấy | Bị vi phạm trong 2 sprint |
| CQRS tách luôn database khi tải nhỏ | Over-engineering, đồng bộ dữ liệu thành gánh nặng |
| Không ghi phương án bị loại | Không giải thích được khi phỏng vấn |
| Trộn C4 Level 1 vào đây | Nhầm đối tượng đọc |

## Câu hỏi phỏng vấn cần trả lời được

- Vì sao Domain không được biết tới EF Core?
- Modular Monolith khác Microservices ở đâu? Khi nào nên tách?
- CQRS là gì? Có bắt buộc tách database không?
- Làm sao đảm bảo module không phụ thuộc lẫn nhau ngoài ý muốn?

## Liên kết
`04-non-functional-requirements.md` · `07-domain-model.md` · `08-system-context.md` · `16-decision-records/`
