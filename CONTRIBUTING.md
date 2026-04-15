# 🤝 Contributing to Don't Be Lazy

Trước tiên, cảm ơn bạn đã quan tâm đến dự án! Mọi đóng góp đều được chào đón, dù là báo lỗi, đề xuất tính năng hay gửi Pull Request.

---

## 📌 Quy trình đóng góp

### 1. Fork & Clone

```bash
# Fork repo về tài khoản của bạn, sau đó clone về máy
git clone https://github.com/<your-username>/DontBeLazy.git
cd DontBeLazy
```

### 2. Tạo branch mới

Luôn tạo branch riêng, KHÔNG làm việc trực tiếp trên `master`:

```bash
git checkout -b feat/ten-tinh-nang
# hoặc
git checkout -b fix/mo-ta-loi
```

### 3. Thực hiện thay đổi & Commit

Tuân thủ quy tắc **Conventional Commits**:

| Prefix | Ý nghĩa |
|---|---|
| `feat:` | Thêm tính năng mới |
| `fix:` | Sửa lỗi |
| `docs:` | Thay đổi tài liệu |
| `refactor:` | Tái cấu trúc code |
| `chore:` | Cập nhật config, dependency |
| `test:` | Thêm/cập nhật test |

```bash
git commit -m "feat: thêm tính năng quản lý Whitelist Profile"
```

### 4. Push & Mở Pull Request

```bash
git push origin feat/ten-tinh-nang
```

Sau đó mở Pull Request về nhánh `master` của repo gốc. Mô tả rõ ràng những gì thay đổi và lý do.

---

## 🐛 Báo cáo lỗi (Bug Report)

Khi mở Issue báo lỗi, vui lòng cung cấp:
- [ ] Môi trường (OS, phiên bản app)
- [ ] Các bước tái hiện lỗi
- [ ] Kết quả thực tế vs kết quả kỳ vọng
- [ ] Screenshots/logs nếu có

---

## 💡 Đề xuất tính năng (Feature Request)

Mở Issue với title bắt đầu bằng `[Feature Request]:` và mô tả:
- Vấn đề bạn đang gặp phải
- Giải pháp bạn đề xuất
- Các giải pháp thay thế đã cân nhắc

---

## 📋 Checklist trước khi gửi PR

- [ ] Code đã được test thủ công trên máy cá nhân.
- [ ] Không có file thừa (`.env`, file build, `node_modules`...).
- [ ] Commit message tuân thủ Conventional Commits.
- [ ] PR có mô tả rõ ràng mục đích thay đổi.

---

Cảm ơn vì đã đóng góp! 🚀
