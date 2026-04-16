# Cấu trúc Cơ sở dữ liệu (Database Schema)

Dự án **Don't Be Lazy** sử dụng `SQLite` làm Local Database (Entity Framework Core là ORM) do tính chất Offline-first và Single-user.
Dữ liệu được tổ chức để đảm bảo tính bất biến của lịch sử (Analytics) và khả năng quản lý trạng thái phức tạp của Recurring Tasks.

---

## 1. Bảng `Settings` (Cài đặt hệ thống toàn cục)
Chỉ có duy nhất 1 bản ghi (Id = 1). Lưu trữ các tham số hệ thống.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, Luôn bằng 1. |
| `GlobalStrictMode` | BOOLEAN | Mặc định `false`. Tác động lên toàn bộ hệ thống. |
| `EnableQuotes` | BOOLEAN | Mặc định `true`. |
| `QuoteLanguage` | VARCHAR(10) | `vi` hoặc `en`. Ngôn ngữ mặc định của câu quote. |
| `DarkTheme` | BOOLEAN | UI preferences. |

---

## 2. Bảng `Profiles` (Kho Whitelist)
Định nghĩa các bộ cấu hình chặn, lưu trữ riêng biệt với Task để tái sử dụng.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Name` | VARCHAR(100) | Tên Profile (User tự đặt). Không được trùng. |
| `IsDefault` | BOOLEAN | Xác định Default Profile. Không thể xóa tên hay Profile này. |
| `CreatedAt` | DATETIME | Thời gian tạo (Local Time). |

---

## 3. Bảng `ProfileEntries` (Chi tiết Whitelist)
Lưu chi tiết các URLs và Process Name thuộc về một Profile. Tối đa 50 entries/Profile (logic check ở Application level).

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `ProfileId` | INTEGER | FK -> `Profiles(Id)`. *ON DELETE CASCADE*. |
| `Type` | VARCHAR(20) | Enum: `Website`, `App`. |
| `Value` | VARCHAR(500) | Domain trần (VD: `github.com`) hoặc process name (`code.exe`). |

---

## 4. Bảng `Tasks` (Danh sách công việc)
Bảng cốt lõi quản lý hệ thống trạng thái và vòng đời của công việc.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Name` | VARCHAR(200) | Tên công việc. NOT NULL. |
| `ExpectedMinutes` | INTEGER | Phút đếm ngược dự kiến (1 - 240). |
| `ProfileId` | INTEGER | FK -> `Profiles(Id)`. *ON DELETE SET NULL* (Nếu null, fallback về Default Profile). |
| `PerTaskStrictMode` | BOOLEAN | `1` (Bật), `0` (Tắt), `NULL` (Dùng cất đặt Global). Cấu hình đè. |
| `Status` | VARCHAR(50) | Enum: `Pending`, `Active`, `Done`, `Abandoned`. |
| `SortOrder` | INTEGER | Thứ tự kéo thả để hiển thị trên UI. |
| `CreatedAt` | DATETIME | Start date của Task. Dùng làm mốc cho Custom Recurring nếu chưa làm. |

**Bổ sung trường Recurring (Theo UC01):**

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `TaskType` | VARCHAR(50) | Enum: `One-time`, `Recurring`. |
| `RecurringType` | VARCHAR(50) | Enum: `Daily`, `Weekly`, `Custom`, `NULL`. |
| `RecurringConfig` | VARCHAR(100) | Cấu hình lặp. VD Weekly: `1,3,5` (Thứ 2,4,6). Custom: `3` (Sau 3 ngày). |
| `IsPaused` | BOOLEAN | Trạng thái Pause cho Recurring task. Tạm ẩn khỏi UI. |
| `LastDoneDate` | DATE | (Local Time) Lưu lại ngày cuối cùng hoàn thành task làm mốc tính toán reset tiếp theo. Có thể NULL nếu chưa hoàn thành lần nào. |

---

## 5. Bảng `SessionHistory` (Lịch sử phiên Focus)
**Thiết kế Immutable (Không đổi):** Khi Task bị xóa, lịch sử vẫn tồn tại nguyên vẹn. Do đó bảng này không join trực tiếp FK bắt buộc vào `Tasks`.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT. |
| `TaskId` | INTEGER | FK -> `Tasks(Id)`. *ON DELETE SET NULL*. Dùng để query history theo từng task nếu Task vẫn còn. |
| `SnapshotTaskName`| VARCHAR(200) | (Lưu cứng) Tên task lúc bắt đầu phiên. Đảm bảo UI Analytics không bị gãy khi Task bị đổi tên/xóa. |
| `FocusStartDate` | DATETIME | Chặn tham chiếu múi giờ: Luôn dùng Local Time lúc Start. Cột này là "Ngày bắt đầu gốc" quyết định Streak. |
| `ExpectedMinutes` | INTEGER | Số phút hứa ban đầu. |
| `ActualMinutes` | INTEGER | Thời lượng đếm ngược (đã vượt qua) thực tế. Không tính thời gian Sleep. |
| `CompletionStatus`| VARCHAR(50) | Enum: `Completed` (Hết giờ), `CompletedEarly` (Hoàn thành sớm), `Abandoned` (Bỏ cuộc). |
| `BlockedCount` | INTEGER | Số lần người dùng cố truy cập web/app rác trong phiên này. |
| `WasStrictMode` | BOOLEAN | Chụp lại trạng thái: Phiên này có phải Strict Mode không. |

---

## 6. Bảng `Quotes` (Kho tàng câu nói động lực)
Dữ liệu seed sẵn hoặc do người dùng tạo thêm. Không bao gồm quote Hardcoded Fallback.

| Cột | Kiểu dữ liệu | Ràng buộc / Ý nghĩa |
|---|---|---|
| `Id` | INTEGER | PK, AUTOINCREMENT |
| `Content` | VARCHAR(500) | Nội dung câu nói. |
| `Author` | VARCHAR(100) | Tên tác giả (VD: *Benjamin Franklin*). |
| `EventType` | VARCHAR(50) | Enum: `PreFocus`, `MidFocus`, `PostFocus`, `GiveUp`, `Random`. |
| `Language` | VARCHAR(10) | `vi` hoặc `en`. |
| `IsBundled` | BOOLEAN | Đánh dấu Quote của hệ thống. Nếu `true`, người dùng không thể Sửa/Xóa. |

---
## Ghi chú về luồng Dữ liệu (Theo thiết kế Use Cases V6.0)

1. **Recovery (Orphan Session):** 
   - `SessionHistory` sẽ tạo ngay 1 record lúc Start Focus, `CompletionStatus` = `NULL`.
   - Cứ 30s update lại `ActualMinutes`. 
   - Khi mất điện và khởi động lại, code sẽ query `SELECT * FROM SessionHistory WHERE CompletionStatus IS NULL`. Đó chính là checkpoint cho Orphan Session (UC03 / UC04).
2. **Streak Computation:**
   - Dùng query: `SELECT DISTINCT DATE(FocusStartDate) FROM SessionHistory WHERE CompletionStatus IN ('Completed', 'CompletedEarly')`.
   - Hàm nội suy chạy trên Local DB.
3. **Hiệu suất & File Location:**
   - Do Write traffic thấp (update 30s/lần) và Single-user, SQLite hoàn toàn đáp ứng được I/O.
   - FIle `.db` được lưu tại thư mục User AppData của HDH (VD: `C:\Users\{User}\AppData\Local\DontBeLazy\app.db`) để tránh giới hạn quyền Write File ở thư mục `Program Files`.
