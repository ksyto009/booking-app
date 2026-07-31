---
name: doc-domain-model
description: Chuẩn viết và rà soát docs/07-domain-model.md — mô hình miền dự án Court Booking (Aggregate, Entity, Value Object, Domain Event, invariant). Dùng khi thiết kế tầng Domain, khi chọn ranh giới aggregate, hoặc khi nghi ngờ Anemic Domain Model.
---

# Skill: Mô hình miền (Domain Model)

## Mục tiêu
Thiết kế tầng `Domain` **trước** khi thiết kế bảng. Đây là mắt xích bị bỏ qua nhiều nhất, và bỏ qua nó dẫn thẳng tới **Anemic Domain Model**.

> Thứ tự đúng: `Business Rules → Domain Model → Database Schema`
> Bảng chỉ là **cách lưu** của domain, không phải ngược lại.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Ubiquitous Language** — đối chiếu `00-glossary.md` |
| 2 | **Bounded Context / Module** — trách nhiệm, dữ liệu sở hữu, cách giao tiếp |
| 3 | **Aggregate & ranh giới** — bảng: root · thành phần bên trong · **bất biến phải bảo vệ** |
| 4 | **Entity vs Value Object** — bảng VO kèm lý do tồn tại |
| 5 | **Trạng thái & hành vi** — liệt kê **method**, mỗi method có tiền/hậu điều kiện + event phát ra |
| 6 | **Domain Event** — ai phát, ai nghe, có đi qua Outbox không |
| 7 | **Domain Service** — chỉ khi logic không thuộc aggregate nào |
| 8 | **Sơ đồ lớp** (mermaid `classDiagram`) |
| 9 | **Ánh xạ Domain → Bảng** — nêu rõ chỗ phi chuẩn hoá có chủ đích và vì sao |

## Quy tắc chất lượng

1. 🔥 **Quy tắc vàng về aggregate: một transaction chỉ sửa MỘT aggregate.** Nếu buộc phải sửa hai → ranh giới sai, hoặc cần domain event.
2. **Ranh giới aggregate quyết định bởi BẤT BIẾN, không phải bởi quan hệ dữ liệu.** Câu hỏi đúng: *"cái gì phải luôn nhất quán ngay lập tức?"*
3. **Aggregate càng nhỏ càng tốt.** Aggregate to gây tranh chấp khoá và transaction dài.
4. **Không có setter công khai trên aggregate root.** Chỉ có method mang tên nghiệp vụ (`Cancel()`, `CheckIn()`), không phải `SetStatus()`.
5. **Value Object là nơi tốt nhất để nhốt rule.** VO `TimeSlot` khiến việc tạo slot 18:30 trở thành **không thể**, thay vì phải nhớ validate ở 5 chỗ.
6. **Tầng Domain không tham chiếu EF Core / ASP.NET / thư viện hạ tầng nào** — kiểm bằng architecture test.
7. **Logic đồng bộ trạng thái phải nằm trong domain**, không rải ở Handler. Ví dụ: `Booking.Cancel()` tự gọi `slot.Release()` — Handler không thể quên.
8. Mỗi `BR-xx` phải được bảo vệ ở **một chỗ xác định**; ghi rõ class + method.

## Checklist trước khi đóng

- [ ] Mỗi `BR-xx` ánh xạ tới một class + method cụ thể
- [ ] Mỗi ranh giới aggregate giải thích được: *"vì sao X ở trong, Y ở ngoài?"*
- [ ] Không có setter công khai trên aggregate root
- [ ] Mọi Value Object có lý do tồn tại (nhốt rule gì)
- [ ] Domain Event liệt kê đủ người nghe
- [ ] Có ánh xạ sang schema, kể cả điểm phi chuẩn hoá
- [ ] Tầng Domain sạch phụ thuộc hạ tầng

## 🚨 Dấu hiệu Anemic Domain Model

| Dấu hiệu | Nghĩa là |
|---|---|
| Class domain chỉ có property với `get; set;` | Không có hành vi |
| Mọi logic nằm trong Handler / Service | Domain rỗng nghĩa |
| Có `SetStatus()`, `UpdateXxx()` thay vì `Cancel()`, `CheckIn()` | Đặt tên theo kỹ thuật, không theo nghiệp vụ |
| Không thể unit test rule nếu không có DbContext | Domain dính hạ tầng |

**Câu hỏi phỏng vấn:** *"Anemic Domain Model là gì? Code của anh có bị không? Làm sao anh biết?"*

## Liên kết
`00-glossary.md` · `06-business-rules.md` · `09-architecture.md` · `10-database-design.md`
