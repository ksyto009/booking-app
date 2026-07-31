# 03 — Yêu cầu chức năng (Functional Requirements)

> **Cách đọc:** mỗi `FR-xx` mô tả **hệ thống phải làm được gì**. Ràng buộc chi tiết nằm ở `BR-xx` ([06-business-rules.md](06-business-rules.md)); kịch bản tương tác nằm ở `UC-xx` ([05-use-cases.md](05-use-cases.md)).
> **Ma trận truy vết** ở cuối file — đây là thứ chứng minh không có yêu cầu nào bị bỏ quên.

**Ưu tiên:** 🔴 Must · 🟡 Should · 🔵 Could · ⚪ Won't (v1)

---

## FR-A — Tài khoản & Xác thực

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-01** | Khách đăng ký tài khoản bằng **số điện thoại**, xác thực bằng OTP | 🔴 | — | UC-01 |
| **FR-02** | Đăng nhập bằng SĐT + mật khẩu, trả về **Access Token** (ngắn hạn) và **Refresh Token** | 🔴 | — | UC-02 |
| **FR-03** | Làm mới phiên bằng Refresh Token, có **xoay vòng token** (rotation) | 🔴 | — | UC-02 |
| **FR-04** | Đăng xuất — thu hồi Refresh Token hiện tại | 🔴 | — | UC-03 |
| **FR-05** | Người dùng xem và cập nhật hồ sơ cá nhân (tên, email tuỳ chọn) | 🟡 | — | UC-04 |
| **FR-06** | Giới hạn tần suất gửi OTP và đăng nhập sai để chống dò mật khẩu | 🔴 | — | — |

## FR-B — Quản lý danh mục

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-07** | Owner tạo/sửa/vô hiệu hoá **chi nhánh** (tên, địa chỉ, giờ mở–đóng) | 🔴 | BR-31 | UC-20 |
| **FR-08** | Owner/Manager tạo/sửa/vô hiệu hoá **sân** trong chi nhánh (mã, loại, trạng thái) | 🔴 | BR-31 | UC-21 |
| **FR-09** | Owner cấu hình **bảng giá** theo chi nhánh/sân + thứ trong tuần + khung giờ, có độ ưu tiên | 🔴 | BR-14 | UC-22 |
| **FR-10** | Owner/Manager **đóng sân tạm thời** trong một khoảng thời gian, có ghi lý do | 🟡 | BR-08 | UC-23 |
| **FR-11** | Khi đóng sân, hệ thống liệt kê các đơn bị ảnh hưởng và hỗ trợ hủy + hoàn 100% | 🟡 | BR-18 | UC-23 |

## FR-C — Tra cứu lịch trống

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-12** | Bất kỳ ai (kể cả chưa đăng nhập) xem được **lịch trống của một chi nhánh theo ngày**, dạng lưới sân × khung giờ | 🔴 | BR-03, BR-08 | UC-05 |
| **FR-13** | Lịch trống hiển thị **giá của từng khung giờ** | 🔴 | BR-14 | UC-05 |
| **FR-14** | Không hiển thị slot trong quá khứ, slot ngoài giờ mở cửa, hoặc slot thuộc khoảng đóng sân | 🔴 | BR-03, BR-04, BR-08 | UC-05 |
| **FR-15** | Lọc lịch trống theo loại sân (trong nhà / ngoài trời) | 🔵 | — | UC-05 |

## FR-D — Đặt sân

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-16** | Khách đã đăng nhập tạo đơn cho **1..N slot liên tiếp trên cùng một sân** | 🔴 | BR-01, BR-02 | UC-06 |
| **FR-17** | Hệ thống **từ chối** đơn nếu bất kỳ slot nào đã bị chiếm, trả lỗi rõ ràng chỉ đúng slot xung đột | 🔴 | **BR-06** | UC-06 |
| **FR-18** | Đơn tạo online chuyển sang trạng thái **chờ thanh toán**, giữ slot trong **10 phút** | 🔴 | BR-07, BR-11 | UC-06 |
| **FR-19** | Hết 10 phút chưa thanh toán → hệ thống **tự động** hủy đơn và giải phóng slot | 🔴 | BR-11 | UC-07 |
| **FR-20** | Khách `IsTrusted` đặt online được xác nhận ngay, chọn trả tiền tại quầy | 🔴 | BR-12 | UC-06 |
| **FR-21** | Staff tạo đơn **hộ khách** (nhập SĐT khách), đơn được xác nhận ngay | 🔴 | BR-13 | UC-08 |
| **FR-22** | Khách xem danh sách đơn của mình (sắp tới / lịch sử) và chi tiết một đơn | 🔴 | — | UC-09 |
| **FR-23** | Mỗi đơn có **mã đơn dễ đọc** để khách đọc qua điện thoại | 🔴 | — | UC-09 |
| **FR-24** | Hệ thống cho khách chọn "sân nào cũng được", tự gán sân trống | 🔵 | BR-09 | — |

## FR-E — Thanh toán

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-25** | Khách thanh toán đơn qua cổng thanh toán (VNPay sandbox) | 🔴 | BR-10 | UC-10 |
| **FR-26** | Hệ thống nhận **webhook** kết quả thanh toán, **xác thực chữ ký** trước khi xử lý | 🔴 | BR-15 | UC-10 |
| **FR-27** | Webhook trùng lặp **không** được xử lý hai lần | 🔴 | BR-15 | UC-10 |
| **FR-28** | Thanh toán thành công → đơn chuyển **Confirmed**, phát sự kiện `BookingConfirmed` | 🔴 | — | UC-10 |
| **FR-29** | Thanh toán thất bại → đơn giữ nguyên trạng thái chờ, khách được thử lại trong thời hạn giữ chỗ | 🔴 | BR-11 | UC-10 |
| **FR-30** | Staff ghi nhận **thu tiền mặt** tại quầy cho đơn `PayAtCounter` | 🔴 | BR-13 | UC-11 |

## FR-F — Hủy & Hoàn tiền

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-31** | Khách hủy đơn của mình; hệ thống tính mức hoàn theo thời điểm hủy | 🔴 | BR-16, BR-17 | UC-12 |
| **FR-32** | Hệ thống **hiển thị số tiền được hoàn trước khi khách xác nhận hủy** | 🔴 | BR-16 | UC-12 |
| **FR-33** | Hủy đơn → giải phóng slot ngay lập tức | 🔴 | BR-06 | UC-12 |
| **FR-34** | Staff/Manager hủy đơn thay khách, bắt buộc nhập lý do | 🔴 | BR-18, BR-32 | UC-13 |
| **FR-35** | Hủy do phía sân (sự cố, mưa) → hoàn **100%** bất kể thời điểm | 🟡 | BR-18 | UC-23 |
| **FR-36** | Yêu cầu hoàn tiền được xử lý **bất đồng bộ**, có trạng thái theo dõi được | 🔴 | BR-19 | UC-12 |

## FR-G — Check-in & No-show

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-37** | Staff tra cứu đơn theo **mã đơn** hoặc **SĐT** để check-in | 🔴 | — | UC-14 |
| **FR-38** | Staff check-in đơn → trạng thái `CheckedIn` | 🔴 | — | UC-14 |
| **FR-39** | Staff đánh dấu `NoShow` khi quá giờ bắt đầu 15 phút | 🔴 | BR-20, BR-21 | UC-15 |
| **FR-40** | `NoShow` làm tăng bộ đếm của khách; đủ 2 lần trong 90 ngày → **tự động** thu hồi `IsTrusted` | 🔴 | BR-22 | UC-15 |

## FR-H — Đặt định kỳ

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-41** | Staff/Owner tạo **chuỗi đặt định kỳ** (sân, thứ, giờ, số giờ, từ ngày, đến ngày, % giảm) | 🟡 | BR-23 | UC-16 |
| **FR-42** | Hệ thống **sinh trước** các buổi trong cửa sổ 8 tuần; job hàng tuần sinh tiếp | 🟡 | BR-24 | UC-17 |
| **FR-43** | Buổi bị trùng lịch được **bỏ qua và ghi log**, không làm hỏng cả chuỗi | 🟡 | BR-25 | UC-17 |
| **FR-44** | Hủy **một buổi** không ảnh hưởng chuỗi; hủy **cả chuỗi** chỉ ảnh hưởng buổi tương lai | 🟡 | BR-26 | UC-18 |
| **FR-45** | Xem danh sách các buổi của một chuỗi kèm trạng thái từng buổi | 🟡 | — | UC-18 |

## FR-I — Quản trị & Phân quyền

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-46** | Owner mời/gán vai trò cho nhân sự (Staff, BranchManager, Partner) | 🔴 | BR-29 | UC-24 |
| **FR-47** | Owner giới hạn **phạm vi chi nhánh** cho từng người | 🔴 | BR-29 | UC-24 |
| **FR-48** | Mọi truy vấn dữ liệu **tự động** lọc theo tenant, không phụ thuộc lập trình viên | 🔴 | BR-28 | — |
| **FR-49** | Người dùng chỉ thao tác được trên dữ liệu trong phạm vi được cấp; vi phạm trả **403** | 🔴 | BR-29, BR-30 | — |
| **FR-50** | Staff/Manager đánh dấu / gỡ `IsTrusted` cho khách | 🔴 | BR-12 | UC-19 |
| **FR-51** | Mọi hành động nhạy cảm được ghi **audit log** kèm giá trị trước/sau | 🔴 | BR-32 | — |

## FR-J — Báo cáo

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-52** | Báo cáo **doanh thu theo chi nhánh** theo khoảng thời gian | 🔴 | BR-29 | UC-25 |
| **FR-53** | Báo cáo **tỉ lệ lấp đầy theo sân** — biết sân nào ế | 🔴 | — | UC-25 |
| **FR-54** | Báo cáo **tỉ lệ lấp đầy theo khung giờ** — biết giờ nào nên giảm giá | 🔴 | — | UC-25 |
| **FR-55** | Danh sách **khách ruột** (theo số lần đặt / doanh thu đóng góp) | 🔵 | — | UC-25 |
| **FR-56** | Báo cáo tôn trọng phạm vi chi nhánh của người xem | 🔴 | BR-29, BR-30 | UC-25 |
| **FR-57** | Xuất báo cáo ra Excel | 🔵 | — | — |

## FR-K — Thông báo

| Mã | Yêu cầu | Ưu tiên | Rule | UC |
|---|---|---|---|---|
| **FR-58** | Gửi xác nhận khi đơn được xác nhận | 🟡 | — | — |
| **FR-59** | Nhắc lịch trước giờ chơi 2 tiếng | 🟡 | — | — |
| **FR-60** | Thông báo khi đơn bị hủy do sân đóng | 🟡 | BR-18 | — |
| **FR-61** | Consumer thông báo phải **idempotent** — không gửi trùng khi message lặp | 🔴 | — | — |

---

## Ma trận truy vết (Traceability Matrix)

Mỗi yêu cầu nghiệp vụ phải được phủ bởi ít nhất một yêu cầu chức năng. **Ô trống = lỗ hổng.**

| Yêu cầu nghiệp vụ | Được phủ bởi |
|---|---|
| BRQ-01 Không trùng lịch | FR-17, FR-19, FR-33 |
| BRQ-02 Thanh toán trước | FR-18, FR-25, FR-28 |
| BRQ-03 Ngoại lệ cho khách ruột | FR-20, FR-30, FR-50 |
| BRQ-04 Khách tự đặt | FR-12, FR-16, FR-22 |
| BRQ-05 Nhân viên đặt hộ | FR-21, FR-37 |
| BRQ-06 Báo cáo cho chủ | FR-52, FR-53, FR-54 |
| BRQ-07 Đặt định kỳ | FR-41 → FR-45 |
| BRQ-08 Phạm vi người góp vốn | FR-47, FR-49, FR-56 |
| BRQ-09 Cách ly đa chủ sở hữu | FR-48, FR-49 |
| BRQ-10 Chính sách hủy | FR-31, FR-32, FR-36 |
| BRQ-11 Đóng sân do sự cố | FR-10, FR-11, FR-35 |
| BRQ-12 Nhắc lịch | FR-59 |
| BRQ-13 Nhận diện khách ruột | FR-50, FR-55 |

✅ **Không có yêu cầu nghiệp vụ nào chưa được phủ.**
