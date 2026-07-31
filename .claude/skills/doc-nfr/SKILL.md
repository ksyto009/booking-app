---
name: doc-nfr
description: Chuẩn viết và rà soát docs/04-non-functional-requirements.md — yêu cầu phi chức năng dự án Court Booking (hiệu năng, tải, bảo mật, khả năng quan sát) kèm số liệu và ánh xạ sang quyết định kiến trúc. Dùng khi cần biện minh cho một lựa chọn hạ tầng hoặc khi nghi ngờ over-engineering.
---

# Skill: Yêu cầu phi chức năng (NFR)

## Mục tiêu
Biến "hệ thống phải nhanh và ổn định" thành **con số kiểm chứng được**, rồi dùng chính những con số đó để **biện minh hoặc bác bỏ** mọi quyết định kiến trúc.

> NFR là **đầu vào của kiến trúc**. Không có NFR thì kiến trúc chỉ là mê tín — và mọi câu trả lời phỏng vấn sẽ thành "em thấy trên mạng người ta làm vậy".

## Cấu trúc bắt buộc

| Phần | Nội dung |
|---|---|
| **1. Cơ sở ước lượng tải** | Phép tính Fermi hiển thị đầy đủ + **kết luận kiến trúc rút ra** |
| **2–10. Nhóm NFR** | Hiệu năng · Chịu tải · Đúng đắn · Sẵn sàng · Bảo mật · Bảo trì & kiểm thử · Quan sát · Vận hành · Trải nghiệm |
| **11. Bảng đối chiếu NFR → Quyết định kiến trúc** | 🔴 **Bắt buộc** — đây là phần chứng minh NFR không phải giấy tờ trang trí |

Mỗi NFR: `Mã` · `Yêu cầu` · `Chỉ tiêu (có SỐ)` · `Cách đo`.

## Quy tắc chất lượng

1. **Không có số thì không phải NFR.** Cấm tuyệt đối: "nhanh", "ổn định", "nhiều người dùng", "dễ mở rộng".
2. **Khách hàng không có số → tự ước lượng bằng Fermi**, ghi rõ giả định và đưa vào `17-risk-analysis.md`.
3. **Phải ghi cả cách đo.** Chỉ tiêu không đo được là chỉ tiêu vô nghĩa.
4. Dùng **percentile** (p95/p99), không dùng trung bình — trung bình che giấu đuôi chậm.
5. 🔥 **Phân biệt yêu cầu ĐÚNG ĐẮN với yêu cầu HIỆU NĂNG.** Ví dụ: "không bao giờ đặt trùng" là đúng đắn, không phải hiệu năng — tải thấp **không** làm nó dễ hơn.
6. **Mọi lựa chọn hạ tầng phải trỏ được về một NFR có số.** Không trỏ được → over-engineering, loại bỏ.
7. Ghi rõ **chế độ suy giảm**: mỗi thành phần chết thì hệ thống còn làm được gì.

## Checklist trước khi đóng

- [ ] Mọi NFR có số và có cách đo
- [ ] Có phép tính ước lượng tải hiển thị đầy đủ, không chỉ kết quả
- [ ] Bảng §11 tồn tại, mỗi quyết định hạ tầng lớn có một dòng
- [ ] Có NFR về **đúng đắn** (không chỉ hiệu năng)
- [ ] Có NFR về **khả năng quan sát** và **kiểm thử** — hai nhóm bị quên nhiều nhất
- [ ] Mỗi thành phần ngoài (Redis, RabbitMQ, cổng thanh toán) có mô tả chế độ suy giảm
- [ ] Không có công nghệ nào được chọn mà không trỏ về được NFR

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| NFR không số | Không kiểm chứng, không dùng để quyết định được |
| Suy ra tải từ cảm giác thay vì phép tính | Over/under-engineering |
| Bỏ qua NFR đúng đắn, chỉ có hiệu năng | Bỏ sót bài toán concurrency |
| Chọn công nghệ trước rồi bịa NFR để hợp thức hoá | Ngược quy trình — người phỏng vấn nhận ra ngay |
| Đặt uptime 99.99% cho hệ thống không cần | Chi phí gấp nhiều lần mà không ai đòi |

## Liên kết
`09-architecture.md` · `15-testing-strategy.md` · `14-security.md` · `17-risk-analysis.md` · `16-decision-records/`
