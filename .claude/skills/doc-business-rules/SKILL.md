---
name: doc-business-rules
description: Chuẩn viết và rà soát docs/06-business-rules.md — quy tắc nghiệp vụ (mã BR) và ma trận phân quyền dự án Court Booking. Dùng khi thêm ràng buộc nghiệp vụ, khi viết test theo rule, hoặc khi thiết kế authorization.
---

# Skill: Quy tắc nghiệp vụ (Business Rules)

## Mục tiêu
Biến ràng buộc nghiệp vụ thành các phát biểu **kiểm chứng được**, có mã, và **truy vết được tới test**.

## Cấu trúc bắt buộc

1. Ghi chú quy ước đặt tên test: `Booking_ShouldRejectOverlappingSlot_BR06()`
2. Nhóm rule theo miền, mỗi nhóm một bảng: `Mã` · `Quy tắc` · **`Kiểm chứng ở đâu`**
3. **Ma trận phân quyền** — hàng = hành động, cột = vai trò, ô phân biệt: ✅ toàn tenant · 🔶 giới hạn phạm vi · ❌ không có quyền
4. Bảng kiểm tra độ phủ test theo nhóm rule

## Quy tắc chất lượng

1. **Mỗi rule phải kiểm chứng được.** Nếu không nghĩ ra được test → chưa phải rule, mới là mong muốn.
2. **Cột "Kiểm chứng ở đâu" là bắt buộc** và phải cụ thể: `Domain: Booking.Create()` · `CSDL: partial unique index` · `Application: validator`.
   → Nó ép bạn quyết định **tầng nào chịu trách nhiệm**, và lộ ra rule nào đang không ai bảo vệ.
3. **Rule quan trọng nhất phải được đánh dấu và đặt ở tầng CSDL nếu có thể.** Bất biến sống còn không được phó thác cho tầng ứng dụng.
4. **Số cụ thể nằm trong rule**, không viết "một khoảng thời gian hợp lý". `10 phút`, `15 phút`, `24h/4h`, `2 lần trong 90 ngày`.
5. **Rule mâu thuẫn phải được hoà giải rõ ràng** và ghi lại rule nào làm việc đó.
6. **Không đánh số lại.** Rule bị bỏ → đánh dấu `Deprecated`, giữ nguyên mã.
7. 🔶 trong ma trận phân quyền là **data-scoped authorization**, không cài được bằng `[Authorize(Roles=...)]` — phải ghi chú rõ điều này.

## Checklist trước khi đóng

- [ ] Mọi rule có cột "kiểm chứng ở đâu" điền cụ thể
- [ ] Rule bất biến sống còn được bảo vệ ở **tầng CSDL**, không chỉ tầng ứng dụng
- [ ] Mọi số ngưỡng đều cụ thể, không mơ hồ
- [ ] Ma trận phân quyền phân biệt được toàn tenant vs giới hạn phạm vi
- [ ] Mọi rule được nhắc tới ở ≥1 use case hoặc FR *(rule mồ côi = thừa hoặc chưa dùng)*
- [ ] Có bảng độ phủ test theo nhóm
- [ ] Nhóm rule phủ đủ: giao dịch lõi, tiền, hủy/hoàn, ngoại lệ vận hành, batch, phân quyền, dữ liệu

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Rule mơ hồ ("hủy sớm thì được hoàn nhiều hơn") | Không code được, không test được |
| Đặt bất biến sống còn ở tầng ứng dụng | Race condition, dữ liệu sai |
| Quên nhóm rule về **dữ liệu** (soft delete, audit) | Phát hiện muộn, phải sửa schema |
| Ma trận phân quyền chỉ có ✅/❌ | Bỏ sót data-scoped authorization → lỗ hổng IDOR |
| Rule trùng lặp với FR | Rule là **ràng buộc**, FR là **chức năng** — không phải một |

## Liên kết
`02-business-requirements.md` · `05-use-cases.md` · `07-domain-model.md` · `14-security.md` · `15-testing-strategy.md`
