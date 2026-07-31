---
name: doc-testing-strategy
description: Chuẩn viết và rà soát docs/15-testing-strategy.md — chiến lược kiểm thử dự án Court Booking (kim tự tháp test, Testcontainers, đặt tên test theo mã BR, architecture test). Dùng khi lập kế hoạch test, viết test cho một business rule, hoặc khi nghi ngờ test cho kết quả xanh giả.
---

# Skill: Chiến lược kiểm thử

## Mục tiêu
Đảm bảo test **thật sự chứng minh được** hệ thống đúng — không phải test xanh cho vui.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Kim tự tháp test** — tầng · tỉ lệ · kiểm cái gì · công cụ |
| 2 | 🔴 **Quy tắc tuyệt đối** về công cụ (cấm cái gì, vì sao) |
| 3 | **Quy ước đặt tên** gắn với mã `BR-xx` |
| 4 | **Test bắt buộc theo use case** — bảng: test · vì sao bắt buộc |
| 5 | **Architecture test** |
| 6 | **Dữ liệu test** — builder, cô lập, reset |
| 7 | **Test phụ thuộc thời gian** |
| 8 | **Ngưỡng chất lượng trong CI** |

## Quy tắc chất lượng

1. 🔥 **Cấm EF Core InMemory provider.** Nó **không hỗ trợ unique index và không có transaction thật** → test chống trùng lịch sẽ **xanh giả** trong khi production vẫn đặt trùng. Dùng **Testcontainers** với CSDL thật.
2. **Đặt tên test trích mã business rule:**
   `<Đối tượng>_Should<Kết quả>_When<Điều kiện>_<BRxx>()`
   → cho phép truy vết mỗi rule được test nào phủ.
3. **Mỗi `BR-xx` phải có ≥1 test.** Rule không có test là rule sẽ bị vi phạm.
4. 🔥 **Bất biến về concurrency phải có test song song thật** — bắn N request đồng thời, khẳng định đúng 1 thành công. Test tuần tự **không** chứng minh được gì.
5. **Test phải cô lập.** Không phụ thuộc thứ tự chạy, không dùng chung dữ liệu.
6. **Inject `TimeProvider` thay vì `DateTime.UtcNow`.** Không có nó thì rule về mốc thời gian (24h, 90 ngày) **không thể test**.
7. **Architecture test là test thật**, không phải tuỳ chọn: kiểm tra quy tắc phụ thuộc giữa tầng và giữa module.
8. **Test bảo mật là bắt buộc**: rò rỉ tenant, IDOR — hai thứ này không tự lộ ra khi dùng tay.
9. **Coverage là chỉ báo, không phải mục tiêu.** 90% coverage mà không có test concurrency thì vô nghĩa.

## Checklist trước khi đóng

- [ ] Mọi `BR-xx` có ≥1 test trích mã rule trong tên
- [ ] Có test bắn N request song song cho bất biến concurrency
- [ ] Có test webhook trùng và webhook sai chữ ký
- [ ] Có test rò rỉ dữ liệu giữa tenant
- [ ] Có test IDOR (đổi id ngoài phạm vi → 403/404)
- [ ] Có test job chạy 2 lần không sinh dữ liệu trùng
- [ ] Có architecture test cho quy tắc phụ thuộc
- [ ] Không project test nào tham chiếu InMemory provider
- [ ] Logic phụ thuộc thời gian dùng `TimeProvider`

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Dùng InMemory cho test có ràng buộc CSDL | **Xanh giả** — bug lọt thẳng ra production |
| Test concurrency bằng cách gọi tuần tự | Không chứng minh được gì |
| Mock repository rồi test "logic" | Đang test cái mock, không test hệ thống |
| Test phụ thuộc thứ tự chạy | Đỏ ngẫu nhiên, đội mất niềm tin vào CI |
| Dùng `DateTime.Now` trong code nghiệp vụ | Không test được rule theo thời gian |
| Chạy theo con số coverage | Test rác vẫn tăng coverage |
| Không test đường lỗi | Đường lỗi mới là chỗ hay hỏng |

## Liên kết
`06-business-rules.md` · `05-use-cases.md` §6 · `14-security.md` · `04-non-functional-requirements.md` §7
