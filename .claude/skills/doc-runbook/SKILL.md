---
name: doc-runbook
description: Chuẩn viết và rà soát docs/19-runbook.md — sổ tay vận hành dự án Court Booking (kịch bản sự cố, chẩn đoán, khắc phục, truy vấn hay dùng). Dùng khi có sự cố production, sau mỗi postmortem, hoặc khi chuẩn bị go-live.
---

# Skill: Sổ tay vận hành (Runbook)

## Mục tiêu
Viết tài liệu để **người đang mệt, đang lo, lúc 11 giờ đêm** vẫn xử lý được sự cố.

> Đây là tài liệu duy nhất được đọc **khi đang hoảng**. Mọi quy tắc viết lách khác đều nhường chỗ cho tiêu chí này.

## Cấu trúc bắt buộc

| § | Nội dung |
|---|---|
| 1 | **Thông tin nhanh** — dashboard, lệnh xem log, lệnh vào CSDL, nơi backup, ai gọi khi bí |
| 2 | **Kiểm tra sức khoẻ** — endpoint health + bảng: thành phần chết → triệu chứng → ảnh hưởng |
| 3 | **Kịch bản sự cố** — mỗi cái theo mẫu **Triệu chứng → Chẩn đoán → Xử lý → Xác minh**, có mức ưu tiên |
| 4 | **Truy vấn hay dùng** — SQL copy-paste được |
| 5 | **Quy trình khẩn cấp** — rollback, restore, tạm dừng job, chế độ chỉ đọc |
| 6 | **Sau sự cố** — quy trình postmortem |

## Quy tắc chất lượng

1. 🔥 **Mọi bước phải copy-paste chạy được ngay.** Không viết "kiểm tra log xem có lỗi gì không" — viết đúng lệnh.
2. **Câu ngắn, mệnh lệnh, không giải thích dài dòng.** Người đọc đang không có thời gian học.
3. **Bắt đầu bằng TRIỆU CHỨNG**, không bắt đầu bằng nguyên nhân. Người gặp sự cố chỉ biết triệu chứng.
4. **Mỗi kịch bản có bước XÁC MINH** — làm sao biết đã sửa xong thật.
5. **Có mức ưu tiên** (P0/P1/P2) để biết cái nào phải xử lý ngay.
6. **Bao gồm cả sự cố về tính đúng đắn dữ liệu**, không chỉ sự cố "hệ thống chết": dữ liệu lệch trạng thái, nghi ngờ rò rỉ giữa tenant — đây thường là sự cố nghiêm trọng nhất và ít được chuẩn bị nhất.
7. **Cập nhật sau MỖI sự cố thật, trong vòng 48 giờ.** Runbook lớn dần theo kinh nghiệm là runbook sống.
8. **Ghi cả cách quay lui**, không chỉ cách sửa tới.

## Checklist trước khi đóng

- [ ] Mọi bước có lệnh cụ thể copy-paste được
- [ ] Mọi kịch bản có bước xác minh
- [ ] Có mức ưu tiên cho từng kịch bản
- [ ] Có kịch bản cho sự cố **đúng đắn dữ liệu**, không chỉ sự cố sập
- [ ] Có kịch bản nghi ngờ rò rỉ dữ liệu giữa tenant (mức P0)
- [ ] Có quy trình rollback và restore
- [ ] Người chưa đọc code vẫn theo được
- [ ] Mỗi sự cố thật đã được bổ sung

## Lỗi thường gặp

| Lỗi | Hậu quả |
|---|---|
| Viết chung chung ("kiểm tra log") | Lúc hoảng không ai nghĩ ra được lệnh |
| Giải thích kiến trúc trong runbook | Sai chỗ — để ở `09-architecture.md` |
| Không có bước xác minh | Tưởng sửa xong nhưng chưa |
| Chỉ có sự cố hạ tầng | Bỏ qua sự cố dữ liệu sai — loại nguy hiểm hơn |
| Viết một lần lúc go-live rồi bỏ | Lỗi thật không bao giờ được ghi lại |
| Không có cách rollback | Kẹt giữa chừng |

## Liên kết
`13-deployment.md` · `09-architecture.md` · `17-risk-analysis.md` · `20-tech-debt.md`
