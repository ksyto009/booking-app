---
name: doc-security
description: Chuẩn viết và rà soát docs/14-security.md — bảo mật dự án Court Booking (STRIDE, JWT/refresh token, cách ly tenant, chống IDOR, bảo mật thanh toán, OWASP). Dùng khi thiết kế xác thực/phân quyền, tích hợp thanh toán, hoặc rà soát lỗ hổng.
---

# Skill: Bảo mật

## Mục tiêu
Xác định **tài sản cần bảo vệ**, **mối đe doạ**, và **biện pháp kiểm chứng được** — không phải danh sách khẩu hiệu.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Tài sản cần bảo vệ** + vì sao có giá trị |
| 2 | **Mô hình đe doạ STRIDE** — mỗi loại có ví dụ **cụ thể trong dự án** và biện pháp |
| 3 | **Xác thực** — OTP, băm mật khẩu, vòng đời token |
| 4 | **Phân quyền** — RBAC **và** data-scoped |
| 5 | **Cách ly đa chủ sở hữu** + các cạm bẫy |
| 6 | **Bảo vệ dữ liệu** — mask log, secrets, TLS |
| 7 | **Checklist OWASP Top 10** đối chiếu từng mục |
| 8 | **Bảo mật thanh toán** |

## Quy tắc chất lượng

1. **Mỗi mối đe doạ phải có biện pháp KIỂM CHỨNG ĐƯỢC**, và tốt nhất là có test tự động.
2. 🔥 **RBAC không đủ.** Vai trò cho phép *gọi* API; nó **không** ngăn người dùng đổi id trên URL để đọc dữ liệu ngoài phạm vi. Đây là **IDOR** — thuộc A01 Broken Access Control, hạng **#1** OWASP.
3. **Access token phải ngắn** (~15 phút). Token dài nghĩa là thu hồi quyền không có hiệu lực cho đến khi hết hạn.
4. **Refresh token: lưu hash, xoay vòng, phát hiện tái sử dụng.** Token cũ được dùng lại = dấu hiệu bị đánh cắp → thu hồi cả chuỗi.
5. 🔥 **Không bao giờ tin dữ liệu chưa xác thực chữ ký**, đặc biệt là webhook. "Kẻ tấn công tự gửi webhook đã-thanh-toán" là lỗ hổng phổ biến và tốn tiền thật.
6. **Cách ly tenant phải ép ở tầng hạ tầng**, không phụ thuộc lập trình viên nhớ `WHERE tenant_id`. Ghi rõ **4 cạm bẫy** của Global Query Filter: `IgnoreQueryFilters`, insert không tự gán, background job thiếu tenant context, raw SQL.
7. **Log không được chứa** mật khẩu, token, hay số điện thoại dạng thô.
8. **Không tự cài mã hoá.** Dùng thư viện chuẩn (BCrypt/Argon2 cho mật khẩu).

## Checklist trước khi đóng

- [ ] Mỗi loại STRIDE có ví dụ cụ thể trong dự án + biện pháp
- [ ] Mỗi biện pháp có test hoặc kiểm tra tự động
- [ ] Có test chứng minh **không rò rỉ dữ liệu giữa tenant**
- [ ] Có test chống **IDOR** cho mọi endpoint có dữ liệu thuộc chi nhánh
- [ ] Webhook có xác thực chữ ký
- [ ] Có chính sách mask dữ liệu cá nhân trong log
- [ ] Không có secret trong repo/image
- [ ] Rate limiting cho endpoint đăng nhập và OTP
- [ ] Đã đối chiếu đủ 10 mục OWASP Top 10

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Chỉ dùng `[Authorize(Roles=...)]` | IDOR — người dùng đọc dữ liệu ngoài phạm vi |
| Tin webhook không xác thực chữ ký | Kẻ tấn công tự xác nhận thanh toán |
| Access token sống 24h | Thu hồi quyền vô nghĩa |
| Lưu refresh token dạng thô trong DB | Lộ DB = lộ toàn bộ phiên |
| Log nguyên object request | Lộ mật khẩu, token, dữ liệu cá nhân |
| Trả 403 khi tài nguyên không thuộc phạm vi | Tiết lộ tài nguyên đó **có tồn tại** — nhiều trường hợp nên trả 404 |
| Kiểm tra quyền ở frontend | Ẩn nút không phải là phân quyền |

## Liên kết
`06-business-rules.md` §8 · `04-non-functional-requirements.md` §6 · `08-system-context.md` · `15-testing-strategy.md`
