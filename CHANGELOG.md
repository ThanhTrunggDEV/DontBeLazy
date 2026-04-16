# 📝 Changelog

Tất cả các thay đổi đáng chú ý của dự án sẽ được ghi lại tại đây.

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
và dự án tuân theo [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [0.2.0-beta] - 2026-04-16

### Added
- **Auto-elevate Administrator** (`app.manifest`): app tự động hiện UAC prompt khi khởi chạy — không cần người dùng phải right-click "Run as administrator"
  - `requestedExecutionLevel level="requireAdministrator"` — bắt buộc vì app dùng DNS flush và kill process trong Strict Mode
  - `PerMonitorV2` DPI awareness — giao diện sắc nét trên màn hình HiDPI
- **App Icon**: logo được nhúng vào exe dưới dạng `.ico` (256×256), hiển thị trong taskbar, Alt+Tab, title bar và File Explorer
- **Branch Protection** cho nhánh `master`: không cho phép push trực tiếp, mọi thay đổi phải qua Pull Request và phải pass CI
- **CI Workflow** (`.github/workflows/ci.yml`): tự động build + test cho mọi PR vào `master`
- **App Logo**: thiết kế logo cho app (power/motivation concept — fist breaking chains), thêm vào README

### Fixed
- File `.db`, `.db-shm`, `.db-wal` bị commit lên remote — đã xóa khỏi tracking và thêm rule vào `.gitignore`

### Changed
- README viết lại hoàn toàn: thêm badge CI/CD, hướng dẫn cài đặt (MSI vs portable), Tech Stack table, hướng dẫn GEMINI_API_KEY
- CHANGELOG sắp xếp lại theo version thực tế

---

## [0.1.0-beta] - 2026-04-16

Bản phát hành beta đầu tiên. Toàn bộ tính năng cốt lõi đã hoạt động trên Windows.

### Added

#### Core Architecture
- **Clean Architecture** gồm 5 layer: `Domain`, `Ports`, `UseCases`, `Infrastructure`, `WPF`
- **Domain Layer**: entities `FocusTask`, `Profile`, `ProfileEntry`, `SessionHistory`, `SystemSettings`, `DailyAnalytics`; enums `TaskStatus`, `CompletionStatus`, `ProfileEntryType`
- **Ports Layer**: interfaces phân tách theo ISP/SRP — `IFocusTaskUseCase`, `IFocusSessionUseCase`, `IProfileUseCase`, `ISystemSettingsUseCase`, `IQuoteUseCase`, `IAnalyticsUseCase`; outbound repositories & service ports
- **UseCases Layer**: business logic đầy đủ — strict mode, time-travel protection, session state singleton, Unit of Work pattern
- **Infrastructure Layer**: Gemini AI client, Win32 DNS flusher, whitelist process sweeper
- **SQLite Data Access**: EF Core 9 + migrations; repositories cho tất cả entities
- **DTOs Layer**: tách biệt hoàn toàn UI khỏi Domain entities

#### UI / WPF
- Material Design Dark theme (màu tím `#7C4DFF`), 5 views: Dashboard, FocusSession, Profiles, Analytics, Settings
- MVVM đầy đủ với CommunityToolkit.Mvvm; converters cho status/color/label
- Thay toàn bộ emoji bằng Material Design `PackIcon`

#### Task Management (UC01)
- Tạo task một lần hoặc lặp lại (Daily / Weekly / Custom interval)
- Dialog Thêm và Chỉnh sửa với đầy đủ trường bao gồm Recurring
- Nút toggle **Undo Done** (Done ↔ Pending), icon tự đổi theo trạng thái
- Nút **Retry Abandoned** — reset task về Pending bằng một click
- Guard: nút Edit/Delete bị disable khi task đang `Active`

#### Profile Management (UC02)
- Tạo, xoá, đổi tên Whitelist Profile
- Quản lý ProfileEntry (Website / App) cho từng profile

#### Focus Session (UC03)
- Start → Pause/Resume → Complete flow
- **Strict Mode**: không thể tắt trong phiên làm việc
- **Friction gate bỏ cuộc**: phải gõ đúng *"Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc"* trước khi abandon

#### AI Features (UC04/UC05/UC06 — Gemini Flash)
- `AiSuggestPriority`: gợi ý thứ tự ưu tiên danh sách task
- `AiBreakdownTask`: phân tách task thành các bước nhỏ có thể thực hiện
- `AiGenerateProfileSuggestion`: gợi ý cấu hình whitelist profile từ mô tả ý định
- **Guilt-trip Quotes**: Gemini tạo câu quote cá nhân hoá (Tiếng Việt / English) hiện trước khi cho phép bỏ cuộc

#### CI/CD & Distribution
- **GitHub Actions** (`.github/workflows/release.yml`): auto build + release khi push tag `v*.*.*`
  - `dotnet publish` win-x64 self-contained single file
  - Build **MSI installer** (WiX v4) — install vào Program Files, Desktop & Start Menu shortcuts, MajorUpgrade
  - Build **portable zip** — chạy thẳng không cần cài
  - Release notes tự động trích từ CHANGELOG; tag có `-` → đánh dấu pre-release
- **Auto-updater** (`UpdaterService`): check GitHub API lúc khởi động, download MSI/ZIP, cài & relaunch
- **Settings — Mục CẬP NHẬT**: version hiện tại, nút kiểm tra, progress bar, release notes, nút cài đặt

### Fixed
- Nút "Huỷ" và "Tạo/Thêm" trong dialog không phản hồi → đồng bộ qua `DialogClosing` event handlers
- DB path relative gây crash Settings (`InvalidOperationException`) → dùng `AppContext.BaseDirectory`
- Icon `UpdateOutline`, `DownloadOutline` không tồn tại trong MaterialDesign 5.3.1 → thay bằng `CloudSync`, `Download`
- File `.db` bị commit lên remote → xoá khỏi tracking, thêm rule vào `.gitignore`

### Known Issues (Beta)
- Strict Mode chưa chặn được các process cố tình thoát qua Task Manager trên Windows 11 ARM
- macOS chưa được hỗ trợ

---

<!-- Template:

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
