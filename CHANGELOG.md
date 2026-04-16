# 📝 Changelog

Tất cả các thay đổi đáng chú ý của dự án sẽ được ghi lại tại đây.

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
và dự án tuân theo [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [0.4.1] - 2026-04-16

### Fixed
- **Crash khi mở Settings** (`XamlParseException`): icon `UpdateOutline` và `DownloadOutline` không tồn tại trong MaterialDesignThemes 5.3.1 — thay bằng `CloudSync` và `Download`
- **Crash khi mở Settings** (`InvalidOperationException: SystemSettings have not been initialized`): DB path `dontbelazy.db` là relative path, resolve sai thư mục khi chạy `dotnet run` — đổi sang `Path.Combine(AppContext.BaseDirectory, "dontbelazy.db")` để luôn dùng đúng file
- `SettingsView.xaml.cs`: bọc `UserControl_Loaded` bằng `try-catch` để lỗi không crash toàn app

---

## [0.4.0] - 2026-04-16

### Added
- **CI/CD Release Pipeline** (`.github/workflows/release.yml`): tự động build và publish khi push tag `v*.*.*`
  - `dotnet publish` win-x64 self-contained single file
  - Build MSI installer bằng WiX v4 (`wix` dotnet tool)
  - Build portable zip làm fallback
  - Tự tạo GitHub Release và đính kèm cả hai artifact
  - Release notes tự động trích xuất từ `CHANGELOG.md`
  - Tag có `-` (VD: `v1.0.0-beta`) tự động đánh dấu là pre-release
- **MSI Installer** (`installer/Product.wxs` — WiX v4):
  - Cài vào `Program Files\DontBeLazy\` (per-machine, 64-bit)
  - Desktop shortcut + Start Menu shortcut
  - `MajorUpgrade`: cài version mới tự động gỡ version cũ
  - UI wizard chuẩn Windows (WixUI_InstallDir)
- **Auto-updater trong app** (`UpdaterService`, `UpdateViewModel`):
  - Kiểm tra GitHub Releases API khi khởi động (ngầm, không chặn UI)
  - So sánh semantic version với assembly hiện tại
  - Ưu tiên tải `.msi`, fallback `.zip` portable
  - MSI: khởi chạy `msiexec /i /qb` (hiện UAC, Windows xử lý cài đặt)
  - ZIP: download → giải nén → cmd script tự copy + relaunch
  - Progress bar khi tải
- **Settings — Mục CẬP NHẬT**: Hiển thị version hiện tại, nút kiểm tra, release notes, nút Tải & Cài đặt

### Changed
- `DontBeLazy.WPF.csproj`: bổ sung `Version`, `AssemblyVersion`, `FileVersion`, `Product`, `Copyright` — được inject từ tag CI (`-p:Version=...`)

---

## [0.3.0] - 2026-04-16

### Added
- **UC01 — Task Type (One-time / Recurring)**:
  - CheckBox "Lặp lại" trong dialog Thêm và Chỉnh sửa task
  - Chu kỳ Daily / Weekly (chọn ngày) / Custom (mỗi N ngày)
  - Các trường hiển thị có điều kiện theo loại chu kỳ
- **UC01 — Undo Done**: nút toggle Done↔Pending trên mỗi task card; icon tự đổi theo trạng thái
- **UC01 — Retry Abandoned**: nút Refresh (màu cam) chỉ hiện khi task `Abandoned`; đặt lại về `Pending` bằng một click
- **UC01 — Edit Task dialog**: mirror đầy đủ các trường của Add dialog, bao gồm Recurring
- **UC01 — Guard Active tasks**: nút Edit và Delete bị disable khi task đang ở trạng thái `Active`
- **UC01 — AI Breakdown Task**: nút AI trên mỗi task card gọi Gemini phân tích chi tiết
- **UC02 — Đổi tên Profile**: nút bút chì trên mỗi profile item mở dialog Rename; gọi `UpdateProfileNameAsync`
- **UC03 — Friction gate khi bỏ cuộc**: overlay guilt-trip yêu cầu người dùng gõ đúng câu `"Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc"` trước khi nút Bỏ cuộc được bật
- **Converters mới**: `TaskStatusToCheckIconConverter`, `TaskStatusEqualConverter`, `StringEqualConverter`

### Fixed
- Nút "Huỷ" và "Tạo/Thêm" trong các dialog không phản hồi — đồng bộ trạng thái ViewModel qua `DialogClosing` event handlers
- Bánh badge "Tạm dừng" hiển thị trên task card khi session đang pause

---

## [0.2.0] - 2026-04-16

### Added
- **Tích hợp Gemini AI** cho 3 use case:
  - `AiSuggestPriority`: gợi ý thứ tự ưu tiên các task
  - `AiBreakdownTask`: phân tách task thành các bước nhỏ
  - `AiGenerateProfileSuggestion`: gợi ý cấu hình whitelist profile từ mô tả ý định
- **Guilt-trip AI Quotes**: Gemini tạo câu quote cá nhân hoá cản người dùng bỏ cuộc, dựa trên tên task và ngôn ngữ cài đặt
- **Dialog AI Result**: hiển thị kết quả AI với loading spinner, scroll view, và tên dialog động

### Fixed
- Xử lý safety feedback của Gemini API (blocked content) — trả thông báo thân thiện thay vì crash
- DNS flusher và Win32 process sweeper hoạt động đúng khi strict mode bật

---

## [0.1.0] - 2026-04-16

### Added
- **Kiến trúc Clean Architecture** gồm 5 layer: `Domain`, `Ports`, `UseCases`, `Infrastructure`, `WPF`
- **Domain Layer**: entities `FocusTask`, `Profile`, `ProfileEntry`, `SessionHistory`, `SystemSettings`, `DailyAnalytics`; enums `TaskStatus`, `CompletionStatus`, `ProfileEntryType`
- **Ports Layer**: interfaces phân tách theo ISP/SRP — `IFocusTaskUseCase`, `IFocusSessionUseCase`, `IProfileUseCase`, `IProfileEntryUseCase`, `ISystemSettingsUseCase`, `IQuoteUseCase`, `IAnalyticsUseCase`; outbound repositories và service ports
- **UseCases Layer**: triển khai đầy đủ business logic bao gồm strict mode, time-travel protection, session state singleton, Unit of Work pattern
- **Infrastructure Layer**: Gemini AI client, Win32 DNS flusher, whitelist process sweeper
- **SQLite Data Access**: EF Core 9 với migrations; repositories cho tất cả entities
- **WPF UI**: Material Design Dark theme, 5 views (Dashboard, FocusSession, Profiles, Analytics, Settings) + MVVM đầy đủ; converters cho status/color/label
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection` wiring toàn bộ stack
- **DTOs Layer**: tách biệt hoàn toàn UI khỏi Domain entities
- **Redesign UI**: thay toàn bộ emoji bằng Material Design `PackIcon`; dark purple theme nhất quán

---

<!-- Mẫu cho các phiên bản tiếp theo:

## [x.y.z] - YYYY-MM-DD

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Removed
- ...

-->
