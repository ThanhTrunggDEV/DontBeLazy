# 🔒 Security Policy

## Phiên bản được hỗ trợ (Supported Versions)

Hiện tại dự án đang ở giai đoạn **Planning / Pre-release**. Khi ứng dụng chính thức phát hành, bảng sau sẽ được cập nhật:

| Version | Supported |
|---|---|
| Pre-release | 🔄 Đang phát triển |

---

## ⚠️ Lưu ý bảo mật quan trọng

**Don't Be Lazy** can thiệp vào hệ thống ở mức độ sâu để thực hiện chức năng chặn:

- Thay đổi file `hosts` của hệ điều hành (yêu cầu quyền Admin/Root).
- Theo dõi và terminate các process app đang chạy.
- Tự bảo vệ chính mình khỏi bị kill bằng Task Manager (Strict Mode).

Do đó, chúng tôi cam kết:
- **KHÔNG thu thập bất kỳ dữ liệu người dùng nào** lên server hay cloud.
- **KHÔNG thực hiện bất kỳ kết nối mạng nào** ngoài phạm vi được người dùng cấp phép qua Whitelist.
- Toàn bộ dữ liệu (task, whitelist, lịch sử) được lưu **100% trên máy cá nhân** (Local DB).

---

## 🐛 Báo cáo lỗ hổng bảo mật (Reporting a Vulnerability)

Nếu phát hiện lỗ hổng bảo mật, **KHÔNG** mở public Issue trực tiếp. Thay vào đó:

1. **Email:** Gửi báo cáo chi tiết qua GitHub Private Security Advisory.
2. **Nội dung cần có:**
   - Mô tả lỗ hổng và tác động tiềm tàng.
   - Các bước tái hiện lỗi (Steps to reproduce).
   - Phiên bản ứng dụng bị ảnh hưởng.
   - (Nếu có) Đề xuất cách khắc phục.
3. **Thời gian phản hồi:** Chúng tôi sẽ cố gắng phản hồi trong vòng **72 giờ**.

---

## 🙏 Cảm ơn

Chúng tôi đánh giá cao mọi đóng góp giúp dự án trở nên an toàn hơn.
