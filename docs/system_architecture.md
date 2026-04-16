# Kiến trúc Hệ thống (System Architecture)

Dự án **Don't Be Lazy** được thiết kế theo tư tưởng **Kiến trúc Lục giác (Hexagonal Architecture / Ports and Adapters)** kết hợp với **Clean Architecture**. Mục tiêu của thiết kế này là tách biệt hoàn toàn Business Logic (UC) khỏi UI Framework (WPF) và các công nghệ hạ tầng (SQLite, Windows API), giúp code dễ test, dễ bảo trì và mở rộng.

Hệ thống được chia thành nhiều sub-projects (Class Libraries) theo các tầng sau:

---

## 1. Tầng Domain (`DontBeLazy.Domain`)
Lõi trung tâm của hệ thống, không phụ thuộc vào bất kỳ project hay thư viện bên ngoài nào (Zero dependencies ngoài .NET Core).
- **Trách nhiệm:** Định nghĩa các Entities cốt lõi (e.g., `Task`, `Profile`, `SessionHistory`) và các Value Objects.
- **Quy tắc:** Chỉ chứa data structure và các business rules thuần túy nội bộ của Entity (VD: Tính số giây chênh lệch, format trạng thái). Không chứa logic truy xuất DB hay gọi hạ tầng.

---

## 2. Tầng Ports (`DontBeLazy.Ports`)
Định nghĩa ranh giới giao tiếp giữa tầng Application (UseCase) và thế giới bên ngoài. Đây là nơi chứa toàn bộ các **Interface** hợp đồng (Contracts).
- **Inbound Ports:** Các interface định nghĩa những gì thế giới bên ngoài (UI) có thể yêu cầu Application làm (e.g., `ITaskUseCase`, `ISessionManagerUseCase`).
- **Outbound Ports:** Các interface định nghĩa những gì Application cần từ thế giới bên ngoài (DB, Hệ điều hành) (e.g., `ITaskRepository`, `IStrictEnginePort`, `IClockPort`).
- **Quy tắc:** Tầng này không chứa Implementations (code thực thi), chỉ chứa phần nguyên mẫu khai báo. Nó reference tới tầng `Domain`.

---

## 3. Tầng Application / UseCase (`DontBeLazy.UseCases`)
Nơi chứa toàn bộ cốt truyện nghiệp vụ (phản ánh 100% logic trong file `use_cases.md`).
- **Trách nhiệm:** Implement (Triển khai) các **Inbound Ports**.
- **Cách hoạt động:** Nhận request từ UI thông qua interface, thực hiện logic nghiệp vụ bằng cách điều phối các Entities từ tầng `Domain` và gọi các công cụ hạ tầng thông qua **Outbound Ports**.
- **Ví dụ:** File `StartFocusSessionHandler` sẽ gọi `ITaskRepository.Get()`, gọi tiếp `IStrictEnginePort.EnableShield()`, sau đó trả kết quả về cho GUI.
- **Quy tắc:** Phụ thuộc vào `Domain` và `Ports`. Tuyệt đối không biết SQLite hay WPF là gì.

---

## 4. Tầng Infrastructure Service (`DontBeLazy.Infrastructure`)
Nơi giao tiếp thực sự với hệ điều hành Windows API.
- **Trách nhiệm:** Implement các **Outbound Ports** liên quan đến công cụ bên ngoài (không phải DB).
- **Thành phần tham gia:**
  - `ProxyShieldService` (Cấu hình proxy chặn web).
  - `ProcessKillerService` (Diệt app lạ nằm ngoài Whitelist bằng WMI).
  - `SystemClockService` (Đọc Monotonic Clock chống gian lận).
  - `PlatformTaskMgrService` (Sửa registry để khóa Task Manager).
- **Quy tắc:** Phụ thuộc vào `Ports` và `Domain`. Reference các SDK đặc thù của Windows.

---

## 5. Tầng Truy cập Dữ liệu (`DontBeLazy.SqliteDataAccess`)
Nơi xử lý giao tiếp vật lý với Database Local.
- **Trách nhiệm:** Định nghĩa `AppDbContext` (Entity Framework Core) và implement các Repository interfaces từ tầng **Outbound Ports**.
- **Thành phần tham gia:**
  - `AppDbContext` (Khai báo `DbSet<Task>`, cấu hình mapping FluentAPI để khớp với `database_schema.md`).
  - Các Repositories (e.g., `TaskRepository`, `ProfileRepository`) thực hiện CRUD qua EF Core.
- **Quy tắc:** Phụ thuộc vào `Ports` và `Domain`. Chứa các logic migration Database.

---

## 6. Tầng UI (`DontBeLazy.WPF`)
Tầng hiển thị tương tác trực tiếp với Người dùng (User).
- **Kiến trúc:** Sử dụng **MVVM (Model - View - ViewModel)** thông qua `CommunityToolkit.Mvvm`.
- **Trách nhiệm:**
  - **View:** File `.xaml`, chịu trách nhiệm render nút bấm, biểu đồ, layout với `MaterialDesignThemes`.
  - **ViewModel:** Định nghĩa bindings, Commands, và xử lý state UI. Gọi xuống hệ thống qua các **Inbound Ports** (UseCases) để gửi lệnh.
  - **Dependency Injection (DI) Root:** Project này là điểm khởi chạy (Entrypoint). Nó cấu hình Microsoft DI container (`ServiceCollection`), mapping các Interfaces ở tầng `Ports` với các Concrete Classes ở tầng `Infrastructure` và `SqliteDataAccess`.
- **Quy tắc:** Phụ thuộc vào TẤT CẢ các tầng còn lại (chỉ để cấu hình DI), nhưng logic code trong ViewModel chỉ được giao tiếp duy nhất thông qua tầng `Ports` và `Domain`.

---

## Sơ đồ Phụ thuộc (Dependency Flow)

Mũi tên `─>` thể hiện "Reference tới":

```text
         [DontBeLazy.WPF (UI / DI Root)]
                 │
                 ├──> DontBeLazy.SqliteDataAccess (EF Core) ─┐
                 │                                           │
                 ├──> DontBeLazy.Infrastructure (Win API) ───┤ Implement
                 │                                           │
                 └──> DontBeLazy.UseCases (App layer) ───────┘
                                 │                           │
                   Implement     │       Call                │
                   [Inbound]     │       [Outbound]          │
                                 v                           v
                           [DontBeLazy.Ports (Interfaces)]
                                         │
                                         v
                            [DontBeLazy.Domain (Entities)]
```

### Lợi ích của kiến trúc này:
1. **Dễ Test (Testability):** Có thể viết Unit Test cho tầng `UseCases` cực kì dễ dàng bằng cách Mock các Outbound Ports (giả lập thao tác chặn app mà không cần chạy code chặn thật).
2. **Ngăn chặn Spagetti Code:** Giao diện WPF thuần túy chỉ có XAML và logic hiển thị, không chứa code cào Database hay gọi system registry. Mọi bug về Business sẽ chỉ nằm duy nhất trong cụm `UseCases`.
3. **Plug-and-Play Hạ tầng:** Nếu tương lai không xài SQLite mà đổi sang file JSON, chỉ cần viết một file thư viện `JsonDataAccess` implement lại các Outbound Ports là xong, không phải sửa dù chỉ 1 dòng ở tầng WPF UI hay UseCases.

---

## 7. Cấu trúc Thư mục Vật lý (Folder Structure)

Khi implement, dự án sẽ có cấu trúc vật lý tĩnh (các project .csproj) mapping 1-1 với thiết kế kiến trúc ở trên như sau:

``text
DontBeLazy/
├── DontBeLazy.slnx                     # Solution File
├── src/
│   ├── DontBeLazy.Domain/              # Class Library (.NET Core)
│   │   ├── Entities/                   # Task, Profile, SessionHistory...
│   │   ├── ValueObjects/
│   │   └── DontBeLazy.Domain.csproj
│   │
│   ├── DontBeLazy.Ports/               # Class Library (.NET Core)
│   │   ├── Inbound/                    # ITaskUseCase, ...
│   │   ├── Outbound/                   # ITaskRepository, IStrictEnginePort, ...
│   │   └── DontBeLazy.Ports.csproj
│   │
│   ├── DontBeLazy.UseCases/            # Class Library (.NET Core)
│   │   ├── Tasks/                      # StartFocusSessionUseCase...
│   │   ├── Profiles/
│   │   └── DontBeLazy.UseCases.csproj
│   │
│   ├── DontBeLazy.Infrastructure/      # Class Library (Windows Specific)
│   │   ├── Windows/                    # ProcessKillerService, ClockService...
│   │   ├── Proxy/                      # ProxyShieldService...
│   │   └── DontBeLazy.Infrastructure.csproj
│   │
│   ├── DontBeLazy.SqliteDataAccess/    # Class Library (EF Core SQLite)
│   │   ├── Persistence/                # AppDbContext, Entity Configurations
│   │   ├── Repositories/               # DbTaskRepository, DbProfileRepository...
│   │   ├── Migrations/                 # EF Core Migrations
│   │   └── DontBeLazy.SqliteDataAccess.csproj
│   │
│   └── DontBeLazy.WPF/                 # WPF Application (UI & Startup)
│       ├── Core/                       # Cấu hình DI (ServiceCollection)
│       ├── Views/                      # DashboardView.xaml, FocusSessionView.xaml...
│       ├── ViewModels/                 # MainViewModel.cs, TaskListViewModel.cs...
│       ├── Helpers/                    # UI Converters, XAML Behaviors
│       ├── App.xaml                    # Bootstrap
│       └── DontBeLazy.WPF.csproj
│
└── docs/                               # Tài liệu dự án
    ├── system_architecture.md
    ├── database_schema.md
    ├── ba_document.md
    └── use_cases.md
``
