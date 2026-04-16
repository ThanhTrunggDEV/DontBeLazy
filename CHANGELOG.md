# 📝 Changelog

Tất cả các thay đổi đáng chú ý của dự án sẽ được ghi lại tại đây.

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
và dự án tuân theo [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added / Feature
- **Light Mode UI Refactoring**: Thay thế toàn bộ mã màu HEX cứng trong giao diện (Settings, Dashboard, Focus, Profiles, MainWindow, App) bằng hệ thống `DynamicResource` của MaterialDesign. Đảm bảo ứng dụng hiển thị xuất sắc và đồng nhất trên cả 2 nền tối và sáng, tự động theo theme người dùng chọn.
- **Motivation Quote UI**: Triển khai hiển thị câu chữ truyền động lực (Motivation Quote) bên dưới đồng hồ đếm ngược. UI tự động gọi AI lấy câu quote (theo database hoặc fallback) dựa trên các cột mốc chiến lược: `PreFocus` (bắt đầu ngẫu nhiên), `MidFocus` (qua 50% thời gian), và `PostFocus` (gần hoàn thành, vượt 90%). Khi khôi phục phiên sẽ hiển thị quote giữ lửa cố định.

---

## [1.0.3] - 2026-04-17

### Added / Feature
- **AI Priority Auto-Sort**: Cập nhật tính năng `AiSuggestPriority` để hệ thống tự động lưu `SortOrder` vào Database dựa vào JSON phân tích của AI thay vì chỉ in chuỗi tĩnh ra cho người dùng đọc. Thay đổi luồng hiển thị ưu tiên.
- **Session UI Binding**: Tự động bind giá trị `ExpectedMinutes` và `Profile` của công việc vào giao diện (ngoài Dialog và vào màn đếm ngược `25:00` gốc) ngay khi người dùng pick item, tiết kiệm thao tác thiết lập lại cho người dùng.

---

## [1.0.2] - 2026-04-16

### Added / Feature
- **File Logging System (Decorator Pattern)**: Triển khai hệ thống ghi log ra file theo ngày tại `%AppData%\DontBeLazy\logs\app-YYYY-MM-DD.log`. Sử dụng Decorator Design Pattern để bọc `IFocusSessionUseCase` và `IStrictEnginePort` — ghi lại mọi sự kiện quan trọng (bắt đầu phiên, hoàn thành, chặn web, lỗi hệ thống) mà không chạm vào logic lõi. Log tự động xoay vòng, giữ tối đa 7 ngày gần nhất.
- **Settings → Mở file log**: Thêm nút "Mở file log" trong mục Quản lý Dữ liệu ở màn hình Cài đặt.
- **AI Automation Pipeline**: Tự động trích xuất JSON từ kết quả của Gemini (cho AI Profile Generation và AI Task Breakdown), tự động khởi tạo dữ liệu trong DB thay vì chỉ trả về văn bản tĩnh.
- **Data Management (Export CSV)**: Chức năng xuất dữ liệu lịch sử tập trung (Session History) ra file CSV ở thẻ Analytics.
- **Data Management (Clear History)**: Bổ sung bộ lọc ngày để dọn dẹp và xoá lịch sử cũ ở màn hình Settings, giúp giải phóng lưu lượng lưu trữ.

### Fixed & Changed
- **State Loss / Session Bug (UI)**: Chuyển đổi toàn bộ vòng đời của tập ViewModels (FocusSession, Dashboard, Settings, etc.) sang `AddSingleton` thay vì `AddTransient` để tránh lỗi mất hiển thị thời gian tập trung (Session Data) khi chuyển đổi qua lại giữa các tab UI (Navigation).
- **Silent Fail / Strict Engine**: Cập nhật cơ chế chặn Web (`hosts` modifier). Ở bản Release build, nếu Antivirus (ví dụ Windows Defender Tamper Protection) chặn app ghi file vì app chưa được ký (unsigned), hệ thống sẽ chủ động hiển thị MessageBox cảnh báo ngay lập tức thay vì lỗi ngầm (Silent Fail).  
- **Crash Recovery**: Vá các lỗi crash tĩnh (silent crash) và quản lý ngoại lệ chưa xử lý qua `Dispatcher.UnhandledException` bằng Logger.
- **UI / SettingsView**: Sửa lỗi thẻ tag XAML bị sai lệch làm rối `DialogHost`, khắc phục lỗi không thể compile WPF.
- **ViewModel Syntax & Null Checks**: Xử lý bug namespace `IFocusSessionUseCase`, thuộc tính `PerTaskStrictMode` không tồn tại, và chặn `NullReferenceException` cho Quote Author tại `DtoMapper`. Khép lại toàn bộ vòng lặp review kiến trúc mã nguồn.

---

## [1.0.1] - 2026-04-16

### Fixed
- **Startup Crash**: Sửa lỗi crash vòng lặp khởi động (startup crash loop) trên các phiên bản cài đặt MSI và bản portable sau khi người dùng upgrade lên version `1.0.0`.

---

## [1.0.0] - 2026-04-16

### Added / Feature
- **AI Settings**: Bổ sung giao diện thiết lập trực tiếp Gemini API Key và chọn Model trong trang Cài đặt. Hỗ trợ cấu hình tự động cho các phiên bản Model mới nhất (Gemini 2.5 Flash, 2.5 Pro, 3.1 Pro Preview...).
- **Settings UI**: Thêm thông tin tác giả (ThanhTrunggDEV) vào giao diện Cài đặt.
- **Strict Mode Friction (UC04)**: Tích hợp hộp thoại cảnh báo bắt buộc gõ "Tôi chấp nhận giảm mức độ kỷ luật" trước khi cho phép tắt Global Strict Mode.
- **Ultimate Strict Mode**: Bổ sung `taskmgr` vào danh sách ứng dụng bị tắt bắt buộc (blacklist) trong quá trình đếm ngược, cùng với cơ chế chặn đóng bằng Alt+F4 / Window Close.
- **Profile Edit (UC02)**: Cho phép edit / đổi tên Profile, xử lý làm sạch URL tự động.
- **Fallback Polling Loop**: Kích hoạt bộ theo dõi dự phòng giám sát ứng dụng vòng lặp trong `WindowsStrictEngine`.

### Fixed & Changed
- **Database Architecture**: Chuyển đổi cơ chế khởi tạo Database từ `EnsureCreated()` sang `Database.Migrate()` lúc khởi động (App Startup) để hỗ trợ Auto Migration. Tự động apply pending migrations vào SQLite, đảm bảo nâng cấp schema mượt mà không loss data (Ví dụ: Thêm cột cho Settings).
- **WindowsStrictEngine**: Bổ sung khối lệnh `try-catch/finally` vào quá trình xóa cờ chặn (ClearRestrictions) để chắc chắn việc đồng bộ khóa / DB không ghim file `hosts`. Giải quyết crash hệ thống qua container `ActiveSessionState`.
- **Profiles UI**: Khắc phục lỗi `DialogHost` bị ẩn do Binding Converter, sửa lỗi chồng lấn các nút (overlapping icons) và lệch hộp thoại Danh sách Profile. Tự động chuyển mục chọn Profile ngay khi tạo Profile mới.
- **Analytics UI**: Sửa lỗi tràn chữ ở thẻ *Streak hiện tại* trên các chuỗi thông báo dài.
- **Session Validation**: Bắt buộc người dùng phải chọn cả `Task` và `Profile` trước khi hiển thị khung nhập cam kết ý định khởi tạo phiên tập trung.
- **Update Dialog**: Tích hợp Markdown (thông qua thư viện `MdXaml`) để render giao diện danh sách các chức năng mới (Release Notes) trở nên có format, chuyên nghiệp và có màu sắc hơn thay vì chuỗi thô.



---

## [0.3.0-beta] - 2026-04-16

### Fixed
- **Auto Update**: Sửa lỗi tính năng tự động cập nhật không nhận các bản phát hành có hậu tố (ví dụ: `v0.x-beta`). Đã chuyển sang dùng endpoint `/releases` (hỗ trợ hiển thị mọi tag kể cả pre-releases) và chuẩn hoá version chuỗi trước khi so sánh.
- **Icon Crash**: Sửa lỗi ứng dụng không khởi động được (throw `XamlParseException` / `Cannot locate resource 'app.ico'`). Ứng dụng hiện sử dụng đường dẫn vật lý cục bộ của `app.ico` để nạp programmatically vào runtime để chống lỗi nạp từ Pack URI.
- **dotnet run (Debug)**: Cho phép build và debug cục bộ `dotnet run` từ bất kì terminal nào mà không cần Admin privileges. Lệnh khởi chạy của build Release dưới file `DontBeLazy.exe` (hay `.msi`) sẽ vẫn yêu cầu quyền Admin thông qua UAC prompt (bảo vệ chức năng chỉnh sửa hệ thống).

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
