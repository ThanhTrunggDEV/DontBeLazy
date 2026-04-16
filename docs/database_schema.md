# Cấu trúc Cơ sở dữ liệu (Database Schema)

Dự án **Don't Be Lazy** sử dụng `SQLite` làm Local Database (Entity Framework Core là ORM) do tính chất Offline-first và Single-user.
Dữ liệu được tổ chức để đảm bảo tính bất biến của lịch sử (Analytics) và khả năng quản lý trạng thái phức tạp của Recurring Tasks.

> **Quy ước chung:**
> - Mọi cột `DATETIME` đều lưu theo **Local Time** của máy người dùng (xem UC05 — Múi giờ & ranh giới ngày).
> - Ranh giới ngày: **00:00 Local Time**. Phiên tính theo ngày bắt đầu.
> - EF Core convention: C# property `bool?` (nullable) sẽ được ghi chú rõ trong schema.

---

## 1. Bảng `Settings` (Cài đặt hệ thống toàn cục)
Chỉ có duy nhất 1 bản ghi (Id = 1). Lưu trữ các tham số hệ thống.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, Luôn bằng 1. |
| `GlobalStrictMode` | BOOLEAN | NOT NULL. Mặc định `false`. Tác động lên toàn bộ phiên Focus khi Per-Task không override. (UC04) |
| `EnableQuotes` | BOOLEAN | NOT NULL. Mặc định `true`. Khi `false`, tắt toàn bộ hệ thống Quote ở mọi sự kiện. (UC06) |
| `QuoteLanguage` | VARCHAR(10) | NOT NULL. `vi` hoặc `en`. Ngôn ngữ mặc định của câu quote. (UC06) |
| `DarkTheme` | BOOLEAN | NOT NULL. Mặc định `true`. UI preferences. |

---

## 2. Bảng `Profiles` (Kho Whitelist)
Định nghĩa các bộ cấu hình chặn, lưu trữ riêng biệt với Task để tái sử dụng.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Name` | VARCHAR(100) | NOT NULL, **UNIQUE**. Tên Profile do User tự đặt. (UC02 — Trùng tên bị từ chối) |
| `IsDefault` | BOOLEAN | NOT NULL. Mặc định `false`. Chỉ duy nhất 1 Profile có `IsDefault = true`. Không thể xóa Profile này. (UC02 — Default Profile) |
| `CreatedAt` | DATETIME | NOT NULL. Thời gian tạo. |
| `UpdatedAt` | DATETIME | NULLABLE. Cập nhật mỗi khi sửa nội dung Profile (entries). |

---

## 3. Bảng `ProfileEntries` (Chi tiết Whitelist)
Lưu chi tiết các URLs và App entries thuộc về một Profile. Tối đa 50 entries/Profile (check ở Application level). (UC02 — Giới hạn entries)

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `ProfileId` | INTEGER | FK -> `Profiles(Id)`. *ON DELETE CASCADE*. |
| `Type` | VARCHAR(20) | NOT NULL. Enum: `Website`, `App`. |
| `Value` | VARCHAR(500) | NOT NULL. Website: domain trần (VD: `github.com`). App: process name (VD: `code`). |
| `ExePath` | VARCHAR(1000) | NULLABLE. Chỉ dùng cho `Type = App`. Đường dẫn file `.exe` đầy đủ (VD: `C:\Program Files\Code\code.exe`). Được điền tự động khi user dùng nút "Browse...". (UC02 — App Rules) |

**Constraints:**
- `UNIQUE(ProfileId, Type, Value)` — chống trùng entry trong cùng 1 Profile.

**Index:**
- `IX_ProfileEntries_ProfileId` trên `ProfileId` — tối ưu CASCADE delete và lookup.

---

## 4. Bảng `Tasks` (Danh sách công việc)
Bảng cốt lõi quản lý hệ thống trạng thái và vòng đời của công việc — bao gồm cả cấu hình Recurring.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Name` | VARCHAR(200) | NOT NULL. Tên công việc. (UC01 — Validation: không trống, tối đa 200 ký tự) |
| `ExpectedMinutes` | INTEGER | NOT NULL. Phút đếm ngược dự kiến (1 - 240). (UC03 — Validation thời gian) |
| `ProfileId` | INTEGER | NULLABLE. FK -> `Profiles(Id)`. *ON DELETE SET NULL*. Nếu NULL, app fallback về Default Profile. (UC02 — Xóa Profile đang gán) |
| `PerTaskStrictMode` | **BOOLEAN NULLABLE** | `1` (Bật), `0` (Tắt), `NULL` (Dùng Global Setting). C# mapping: `bool?`. (UC04 — Per-task > Global) |
| `Status` | VARCHAR(20) | NOT NULL. Enum: `Pending`, `Active`, `Done`, `Abandoned`. Mặc định `Pending`. (UC01 — Task State Machine) |
| `SortOrder` | INTEGER | NOT NULL. Thứ tự kéo thả hiển thị trên UI. Lưu vào DB, giữ nguyên sau restart. (UC01 — Drag-and-drop) |
| `TaskType` | VARCHAR(20) | NOT NULL. Enum: `OneTime`, `Recurring`. Mặc định `OneTime`. (UC01 — Loại Task) |
| `RecurringType` | VARCHAR(20) | NULLABLE. Enum: `Daily`, `Weekly`, `Custom`. Chỉ NOT NULL khi `TaskType = Recurring`. (UC01 — Cấu hình Recurring) |
| `RecurringConfig` | VARCHAR(100) | NULLABLE. Cấu hình lặp. Weekly: `1,3,5` (T2,T4,T6). Custom: `3` (sau mỗi 3 ngày). (UC01 — Recurring Config) |
| `IsPaused` | BOOLEAN | NOT NULL. Mặc định `false`. Khi `true`, task ẩn khỏi danh sách và vòng lặp reset bị tạm dừng. (UC01 — Pause Recurring) |
| `LastDoneDate` | DATE | NULLABLE. Ngày cuối cùng hoàn thành task (Local Time, theo ngày bắt đầu phiên). Dùng làm mốc tính reset cho Recurring. NULL nếu chưa hoàn thành lần nào → fallback về `CreatedAt`. (UC01 — Cross-midnight rule) |
| `CreatedAt` | DATETIME | NOT NULL. Thời gian tạo. Cũng là mốc ban đầu cho Custom Recurring khi `LastDoneDate` còn NULL. |
| `UpdatedAt` | DATETIME | NULLABLE. Cập nhật mỗi khi sửa task (tên, profile, recurring config...). |

**Index:**
- `IX_Tasks_Status` trên `Status` — lọc nhanh task Pending/Active.
- `IX_Tasks_ProfileId` trên `ProfileId` — FK lookup.

---

## 5. Bảng `SessionHistory` (Lịch sử phiên Focus)
**Thiết kế Immutable:** Khi Task bị xóa, lịch sử vẫn tồn tại nguyên vẹn nhờ các cột Snapshot.
**Thiết kế Checkpoint:** Record được INSERT ngay lúc Start (với `CompletionStatus = NULL`), update mỗi 30 giây. Record có `CompletionStatus IS NULL` = phiên đang chạy hoặc orphan session cần recovery.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT. |
| `TaskId` | INTEGER | NULLABLE. FK -> `Tasks(Id)`. *ON DELETE SET NULL*. Dùng để query history theo task nếu Task còn tồn tại. |
| `SnapshotTaskName` | VARCHAR(200) | NOT NULL. Tên task lúc bắt đầu phiên. Đảm bảo UI Analytics không bị gãy khi Task bị đổi tên/xóa. |
| `SnapshotProfileName` | VARCHAR(100) | NULLABLE. Tên Profile lúc bắt đầu phiên. Dùng khi Profile bị đổi tên/xóa sau đó. |
| `FocusStartDate` | DATETIME | NOT NULL. Thời điểm bắt đầu phiên (Local Time). Cột này quyết định phiên thuộc ngày nào cho Analytics và Streak. (UC05 — Ranh giới ngày) |
| `FocusEndDate` | DATETIME | NULLABLE. Thời điểm kết thúc phiên. NULL nếu phiên đang chạy hoặc orphan. |
| `ExpectedSeconds` | INTEGER | NOT NULL. Thời lượng đếm ngược ban đầu **tính bằng giây**. (VD: 60 phút = 3600 giây) |
| `ActualSeconds` | INTEGER | NOT NULL. Mặc định `0`. Thời lượng đã trôi qua **tính bằng giây**, cập nhật mỗi 30 giây. Không tính thời gian sleep. (UC03 — Checkpoint mỗi 30s, UC04 — Crash Recovery ±30 giây) |
| `CompletionStatus` | VARCHAR(20) | NULLABLE. Enum: `Completed` (Hết giờ + chọn "Hoàn thành Task"), `CompletedEarly` (Hoàn thành sớm), `StillWorking` (Hết giờ + chọn "Vẫn cần làm thêm"), `Abandoned` (Bỏ cuộc). **NULL = phiên đang chạy hoặc orphan chưa kết thúc.** (UC03 — Multi-session flow) |
| `BlockedCount` | INTEGER | NOT NULL. Mặc định `0`. Số lần cố truy cập web/app ngoài Whitelist trong phiên này. (UC05 — Số lần bị chặn) |
| `WasStrictMode` | BOOLEAN | NOT NULL. Chụp lại: phiên này chạy Strict Mode hay không. (UC04 — Crash Recovery cần biết strict state) |

**Constraints:**
- `CompletionStatus` chỉ nhận giá trị `NULL`, `Completed`, `CompletedEarly`, `StillWorking`, hoặc `Abandoned` (check ở Application level).

**Index:**
- `IX_SessionHistory_FocusStartDate` trên `FocusStartDate` — Analytics query theo ngày.
- `IX_SessionHistory_CompletionStatus` trên `CompletionStatus` — Streak computation & Orphan detection.
- `IX_SessionHistory_TaskId` trên `TaskId` — Lọc phiên theo task (bộ lọc "Theo task" trong Analytics).

---

## 6. Bảng `SessionProfileSnapshot` (Snapshot Whitelist cho Recovery)
Lưu bản sao cứng của Profile Rules tại thời điểm Start Focus. Dùng khi Crash Recovery cần khôi phục lại khiên chặn **đúng rule đã chạy**, không bị ảnh hưởng bởi việc Profile bị sửa/xóa trong lúc app bị crash.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `SessionId` | INTEGER | FK -> `SessionHistory(Id)`. *ON DELETE CASCADE*. Mỗi Session có 1 bộ snapshot. |
| `Type` | VARCHAR(20) | NOT NULL. Enum: `Website`, `App`. |
| `Value` | VARCHAR(500) | NOT NULL. Domain trần hoặc process name. |
| `ExePath` | VARCHAR(1000) | NULLABLE. Đường dẫn `.exe` (chỉ cho App). |

**Lifecycle:**
- **INSERT:** Khi Start Focus, sao chép toàn bộ entries từ Profile gắn với Task vào bảng này. (UC03 — Profile Snapshot)
- **READ:** Khi Crash Recovery, đọc từ bảng này thay vì từ `ProfileEntries` hiện tại. (UC04 — Crash Recovery)
- **DELETE:** Tự động xóa khi phiên kết thúc bình thường (CASCADE từ SessionHistory nếu cần, hoặc app tự dọn). Giữ lại nếu `CompletionStatus IS NULL` (orphan).

---

## 7. Bảng `Quotes` (Kho tàng câu nói động lực)
Dữ liệu seed sẵn hoặc do người dùng tạo thêm. Quote hard-coded fallback (VD: *"Keep going."*) **không nằm trong DB** mà được code cứng trong Application layer. (UC06 — Kho quote rỗng fallback)

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Content` | VARCHAR(500) | NOT NULL. Nội dung câu nói. |
| `Author` | VARCHAR(100) | NULLABLE. Tên tác giả (VD: *Benjamin Franklin*). User quote có thể không có tác giả. |
| `EventType` | VARCHAR(20) | NOT NULL. Enum: `PreFocus`, `MidFocus`, `PostFocus`, `GiveUp`, `Random`. (UC06 — 4 sự kiện + Random) |
| `Language` | VARCHAR(10) | NOT NULL. `vi` hoặc `en`. (UC06 — Chọn ngôn ngữ) |
| `IsBundled` | BOOLEAN | NOT NULL. Đánh dấu Quote bundle sẵn của hệ thống. Nếu `true`, người dùng không thể Sửa/Xóa. (UC06 — Quản lý kho Quote) |

**Index:**
- `IX_Quotes_EventType_Language` trên `(EventType, Language)` — lựa chọn quote theo sự kiện và ngôn ngữ.

---

## Sơ đồ quan hệ (ERD tóm tắt)

```
Settings (1 row)

Profiles ──1:N──> ProfileEntries
    │
    └──1:N──> Tasks ──1:N──> SessionHistory ──1:N──> SessionProfileSnapshot
                                    │
                                    └──> Quotes (independent, no FK)
```

---

## Ghi chú về luồng Dữ liệu (Cross-reference Use Cases v6.0)

### 1. Recovery (Orphan Session & Crash Recovery) — UC03, UC04
- Lúc Start Focus: INSERT vào `SessionHistory` (`CompletionStatus = NULL`) + INSERT snapshot vào `SessionProfileSnapshot`.
- Cứ 30 giây: UPDATE `ActualSeconds` trong `SessionHistory`.
- Khi app khởi động lại:
  ```sql
  SELECT * FROM SessionHistory WHERE CompletionStatus IS NULL
  ```
  - Nếu tìm thấy + `WasStrictMode = true` → tự động khôi phục, đọc rules từ `SessionProfileSnapshot`.
  - Nếu tìm thấy + `WasStrictMode = false` → hỏi Khôi phục / Bỏ qua.
  - Thời gian còn lại = `ExpectedSeconds - ActualSeconds` (sai lệch tối đa ±30 giây).

### 2. Streak Computation — UC05
```sql
SELECT DISTINCT DATE(FocusStartDate) 
FROM SessionHistory 
WHERE CompletionStatus IN ('Completed', 'CompletedEarly', 'StillWorking')
ORDER BY DATE(FocusStartDate) DESC
```
- Lưu ý: `StillWorking` cũng tính vào Streak vì phiên đã hoàn thành (chỉ task chưa Done).
- Hàm nội suy đếm ngược chuỗi ngày liên tiếp từ hôm nay.

### 3. Analytics Filter "Theo task" — UC05
```sql
SELECT SnapshotTaskName, SUM(ActualSeconds), COUNT(*) 
FROM SessionHistory 
WHERE TaskId = @taskId OR SnapshotTaskName = @taskName
GROUP BY DATE(FocusStartDate)
```

### 4. Lazy Reset Recurring — UC01
```sql
-- Daily: reset nếu LastDoneDate < hôm nay
SELECT * FROM Tasks 
WHERE TaskType = 'Recurring' AND RecurringType = 'Daily' 
  AND Status IN ('Done', 'Abandoned') 
  AND (LastDoneDate < DATE('now','localtime') OR LastDoneDate IS NULL)
  AND IsPaused = 0

-- Weekly: chỉ visible đúng ngày
-- App logic check: CAST(strftime('%w', 'now', 'localtime') AS INT) IN RecurringConfig
```

### 5. Hiệu suất & File Location
- Write traffic thấp (update 30s/lần) và Single-user → SQLite hoàn toàn đáp ứng.
- File `.db` lưu tại: `C:\Users\{User}\AppData\Local\DontBeLazy\app.db` — tránh vấn đề quyền ghi ở `Program Files`.

### 6. Mapping CompletionStatus → UC03 Flow
| Hành động người dùng | CompletionStatus | Task Status |
|---|---|---|
| Timer hết giờ → "Hoàn thành Task" | `Completed` | `Done` |
| Timer hết giờ → "Vẫn cần làm thêm" | `StillWorking` | `Pending` |
| Bấm "Hoàn thành sớm ✓" | `CompletedEarly` | `Done` |
| Bấm "Stop/Give up" → gõ friction | `Abandoned` | `Abandoned` |
| App đang chạy / Orphan chưa recover | `NULL` | `Active` |
