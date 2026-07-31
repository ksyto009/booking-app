# 14 — Bảo mật (Security)

> 🚧 **CHƯA VIẾT.** Viết trong Sprint 1 (phần xác thực) và bổ sung ở Sprint 4 (phần thanh toán). Dùng skill `/doc-security`.
> ⚠️ Đây là tài liệu **bắt buộc**, không phải "nên có" — hệ thống xử lý **tiền** và **dữ liệu cá nhân**.

---

## Dàn ý bắt buộc

### 1. Tài sản cần bảo vệ
| Tài sản | Vì sao giá trị |
|---|---|
| Số điện thoại + tên khách | Dữ liệu cá nhân |
| Dữ liệu doanh thu từng tenant | Bí mật kinh doanh — **rò rỉ giữa tenant là sự cố giết công ty SaaS** |
| Giao dịch thanh toán | Tiền thật |
| Token phiên | Chiếm quyền tài khoản |

### 2. Mô hình đe doạ (STRIDE)
| Loại | Ví dụ trong dự án | Đối phó |
|---|---|---|
| **S**poofing | Giả mạo webhook VNPay | Xác thực chữ ký (NFR-26) |
| **T**ampering | Sửa `branchId` trên URL | Kiểm tra `BranchScope` phía server |
| **R**epudiation | Chối đã hủy đơn | Audit log (BR-32) |
| **I**nfo disclosure | Partner xem doanh thu chi nhánh khác | Data-scoped authorization |
| **D**oS | Spam OTP, dò mật khẩu | Rate limiting (NFR-25) |
| **E**levation | Staff tự nâng lên Owner | Kiểm tra quyền phía server, không tin claim do client gửi |

### 3. Xác thực (Authentication)
Đăng ký/đăng nhập bằng SĐT + OTP · băm mật khẩu (BCrypt/Argon2) · Access token **15 phút** · Refresh token: lưu **hash**, xoay vòng, **phát hiện tái sử dụng** *(dấu hiệu token bị đánh cắp → thu hồi cả chuỗi)*.

### 4. Phân quyền (Authorization)
Hai tầng: **RBAC** (vai trò làm được gì) + **Data-scoped** (trên bản ghi nào).
Nêu rõ cơ chế cài đặt và cách kiểm tra tự động cho **mọi** endpoint có dữ liệu thuộc chi nhánh.

### 5. Cách ly đa chủ sở hữu
Global Query Filter · gán `TenantId` tự động khi ghi · **4 cạm bẫy**: `IgnoreQueryFilters`, insert không tự gán, background job không có tenant context, raw SQL.

### 6. Bảo vệ dữ liệu
Mask SĐT trong log (`09xxxx1234`) · không đưa dữ liệu cá nhân vào query string · secrets qua biến môi trường · TLS bắt buộc.

### 7. Checklist OWASP Top 10
Đối chiếu từng mục với biện pháp cụ thể trong dự án. **A01 Broken Access Control là rủi ro số 1 của hệ thống này.**

### 8. Bảo mật thanh toán
Xác thực chữ ký · idempotency · không tin redirect trình duyệt · đối soát doanh thu hằng ngày · không bao giờ lưu thông tin thẻ.

---

## Tiêu chí hoàn thành

- [ ] Mỗi loại STRIDE có ít nhất một biện pháp cụ thể, có thể kiểm chứng
- [ ] Mỗi biện pháp có test hoặc kiểm tra tự động tương ứng
- [ ] Có test chứng minh **không rò rỉ dữ liệu giữa tenant**
- [ ] Không có secret nào trong mã nguồn hoặc Docker image
