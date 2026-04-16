# Tài liệu Use Case (UC) - Dự án "Don't Be Lazy"

## Danh sách Tác nhân (Actors)
- **Người dùng (User):** Người trực tiếp thao tác trên ứng dụng cài đặt tại máy cá nhân. (Hệ thống không có Multi-role hay Admin do đây là Private Single-user App).

---

## Danh sách Use Case Tổng Quan
| Mã UC | Tên Use Case | Mô tả ngắn gọn |
|---|---|---|
| UC01 | Quản lý công việc (Task) | Thêm, sửa, xóa và đánh dấu hoàn thành task. |
| UC02 | Quản lý danh sách Whitelist | Thêm/Xóa các URL trang web và ứng dụng được phép chạy. |
| UC03 | Bắt đầu phiên tập trung | Khởi động đồng hồ đếm ngược và hệ thống chặn app/web. |
| UC04 | Cấu hình Strict Mode | Bật/tắt chế độ kỷ luật thép chặn mọi sự can thiệp giữa chừng. |
| UC05 | Xem báo cáo & thống kê | Xem thời gian tập trung và số lần bị hệ thống chặn. |
| UC06 | Hiển thị Motivation Quote | Hệ thống tự động xuất hiện các câu quote truyền động lực tại các thời điểm chiến lược. |

---

## Chi tiết các Use Case

### UC01: Quản lý công việc (Task Management)
- **Tác nhân:** Người dùng
- **Mô tả:** Người dùng tạo danh sách những việc cần làm trong ngày (To-do list).
- **Tiền điều kiện:** Ứng dụng đang mở.
- **Luồng sự kiện chính — Thêm Task mới:**
  1. Người dùng chọn chức năng "Quản lý công việc" (hoặc nhập trực tiếp từ màn hình chính).
  2. Hệ thống hiển thị danh sách các task hiện có, **sắp xếp theo thứ tự tạo mới nhất ở trên** (mặc định). Người dùng có thể kéo thả (drag-and-drop) để sắp xếp lại — thứ tự này được lưu vào Local DB (trường `sort_order`) và giữ nguyên sau khi khởi động lại app.
  3. Người dùng nhập tên công việc, thiết lập thời gian (VD: 60 phút), chọn **Loại task** (One-time hoặc Recurring) và **gán một bộ Whitelist Profile** cấu hình riêng cho Task này.
  4. Người dùng bấm "Thêm".
  5. Hệ thống validate dữ liệu đầu vào (xem Validation Rules bên dưới), sau đó lưu kết nối (Task <-> Whitelist) vào Local DB và cập nhật hiển thị.
- **Luồng thay thế — Sửa Task:**
  - Người dùng nhấp "Sửa" trên một task ở trạng thái `Pending`, `Done` hoặc `Abandoned`.
  - Hệ thống mở form Sửa với các trường có thể chỉnh sửa: **Tên task**, **Thời gian ước tính**, **Whitelist Profile được gán**, **Loại task (One-time / Recurring)** và cấu hình chu kỳ nếu là Recurring.
  - **Đổi type:** Khi đổi từ Recurring → One-time: chu kỳ lặp bị xóa, trạng thái task giữ nguyên. Khi đổi từ One-time → Recurring: hệ thống yêu cầu cấu hình chu kỳ (Daily / Weekly / Custom) như khi tạo mới.
  - Sau khi lưu, hệ thống cập nhật bản ghi trong Local DB mà không thay đổi trạng thái hiện tại của task.
- **Luồng thay thế — Hoàn thành Task:**
  - Người dùng click checkbox "Hoàn thành". Hệ thống gạch ngang task và chuyển trạng thái sang `Done`.
- **Luồng thay thế — Hoàn tác Done (Undo Done):**
  - Người dùng click lại vào checkbox của task đang ở trạng thái `Done` → hệ thống hiện xác nhận: *"Bỏ đánh dấu hoàn thành và đưa task về Pending?"* → Bấm "Xác nhận".
  - Task trở về trạng thái `Pending`, gạch ngang biến mất. Streak **không bị ảnh hưởng** (phiên Focus đã hoàn thành vẫn được tính).
- **Trạng thái của Task (Task State Machine):**
  | Trạng thái | Ý nghĩa | Hiển thị trong danh sách |
  |---|---|---|
  | `Pending` | Task đã tạo, chưa bắt đầu phiên Focus nào | ✅ Hiện — bình thường |
  | `Active` | Đang được thực hiện trong phiên Focus hiện tại | ✅ Hiện — có badge "Đang Focus" |
  | `Done` | Đã được đánh dấu hoàn thành bởi người dùng | ✅ Hiện — gạch ngang, mờ đi |
  | `Abandoned` | Phiên bị hủy sớm; task **ở lại danh sách với badge "Đã bỏ"** cho đến khi người dùng tự restart | ✅ Hiện — badge màu cam "Đã bỏ cuộc" |
  - `Abandoned` là **persistent state** — task không tự động về `Pending`. Người dùng phải chủ động bấm "Thử lại" để chuyển task về `Pending`.
  - **Ngoại lệ với Recurring Task:** Nếu recurring task đang ở trạng thái `Abandoned` khi đến ngày reset, hệ thống **ưu tiên reset về `Pending`** (recurring rule thắng persistent Abandoned). Badge "Đã bỏ cuộc" biến mất, task hiển thị lại bình thường.
  - Task ở trạng thái `Active` sẽ **disable** nút Xóa và nút Sửa trên UI.
  - Task chỉ được Xóa vĩnh viễn khi ở trạng thái `Pending`, `Done` hoặc `Abandoned`.
- **Validation Rules — Tên Task:**
  - Không được để trống hoặc chỉ có khoảng trắng → hiển thị lỗi *"Tên task không được để trống."*
  - Độ dài tối đa: **200 ký tự** → nếu vượt quá, hệ thống highlight đỏ và hiển thị bộ đếm ký tự còn lại.
- **Luồng ngoại lệ — Xóa task đang ở trạng thái `Active`:**
  - Hệ thống từ chối và hiển thị: *"Không thể xóa task đang trong phiên Focus. Hãy kết thúc hoặc hủy phiên hiện tại trước."*
- **Hậu điều kiện:** Dữ liệu task, trạng thái và thứ tự sắp xếp được lưu trữ cục bộ trên máy.

- **Loại Task (Task Type):**
  - Khi tạo hoặc sửa task, người dùng chọn một trong hai loại:
    - **One-time** (mặc định): Task thông thường, hoàn thành xong là xong.
    - **Recurring**: Task tự động tái xuất hiện theo chu kỳ đã cấu hình.

#### Cấu hình Recurring Task:
  | Loại lặp | Mô tả |
  |---|---|
  | `Daily` | Reset về `Pending` mỗi ngày |
  | `Weekly` | Chọn cụ thể các ngày trong tuần (VD: T2, T4, T6) — chỉ visible và `Pending` đúng ngày đã chọn |
  | `Custom` | Tự đặt chu kỳ: sau mỗi X ngày kể từ `last_done_date`. Nếu chưa làm lần nào, tính từ `created_date`. |

#### Logic reset Recurring Task:
  - Hệ thống dùng **lazy reset** — không cần background process chạy đêm. Kiểm tra khi app mở hoặc tab Tasks được focus.
  - Điều kiện reset: `last_done_date < hôm nay` (Daily) hoặc hôm nay nằm trong danh sách ngày đã chọn (Weekly) hoặc `today >= last_done_date + X` (Custom).
  - Khi reset: task về trạng thái `Pending`. **Chỉ hiện 1 instance** — không backfill nếu bỏ lỡ nhiều ngày.
  - **Không reset** nếu task đang ở trạng thái `Active`.
  - **Recurring + Abandoned:** Nếu đến ngày reset mà task đang `Abandoned`, hệ thống reset về `Pending` bình thường (xem State Machine phía trên).
  - **Mark Done Early trên Recurring Task:** Hệ thống ghi `last_done_date = hôm nay`, Streak cộng bình thường. Task **KHÔNG reset ngay lập tức** — chờ đến lần kiểm tra tiếp theo (mở app hôm sau hoặc khi đủ điều kiện chu kỳ) mới reset về `Pending`.
  - Xóa vĩnh viễn recurring task **không xóa lịch sử phiên** — Analytics vẫn giữ nguyên dữ liệu cũ.

- **Luồng thay thế — Pause Recurring Task:**
  - Người dùng nhấp icon ⏸ trên một recurring task → hệ thống hiện xác nhận: *"Tạm dừng task này? Task sẽ không hiện trong danh sách cho đến khi bạn bật lại."*
  - Khi Pause: task ẩn khỏi danh sách, vòng lặp reset bị tạm dừng, **không ảnh hưởng Streak**.
  - Người dùng có thể vào Settings → Recurring Tasks → xem danh sách đang Pause và bật lại bất kỳ lúc nào.

- **Luồng ngoại lệ — Profile bị xóa trên Recurring Task:**
  - Nếu Profile gắn với một recurring task bị xóa, task hiển thị badge cảnh báo màu vàng: *"Profile bị xóa — đang dùng Default"* mỗi khi task reset về Pending.
  - Người dùng cần vào Sửa task để gán lại Profile phù hợp.

- **Luồng ngoại lệ — Weekly task visibility:**
  - Task weekly chỉ **visible và có thể Focus** vào đúng các ngày đã chọn. Ngày khác: task ẩn hoàn toàn khỏi danh sách (không hiển thị dù đang Pending).
  - Trong Analytics, vẫn hiển thị lịch sử các phiên đã làm đúng ngày.

### UC02: Quản lý bộ quy tắc ưu tiên (Whitelist Profiles)
- **Tác nhân:** Người dùng
- **Mô tả:** Người dùng tạo trước các tệp hồ sơ (Profiles) tập hợp các ứng dụng và trang web được phép chạy (VD: Profile "Lập trình", Profile "Học Ngoại Ngữ"). Các Profile này nhằm mục đích gán nhanh cho mỗi Task cụ thể sau này.
- **Tiền điều kiện:** KHÔNG ở trong trạng thái Strict Mode của một Task đang dùng Profile đó.
- **Luồng sự kiện chính:**
  1. Chọn mục cài đặt "Whitelist Profiles" → Bấm tạo mới Profile.
  2. Người dùng đặt tên Profile và chọn tab "Websites" hoặc "Apps".
  3. Nhập URL theo quy tắc định dạng (xem URL Rules bên dưới) hoặc thêm ứng dụng theo đường dẫn hoặc tên process (xem App Rules bên dưới).
  4. Bấm "Thêm". Hệ thống lưu Profile vào danh sách cục bộ để sẵn sàng gán cho Task.
- **URL Rules — Định dạng cho phép:**
  - Chỉ hỗ trợ **exact domain** (VD: `github.com`, `docs.google.com`). Ký tự wildcard (`*`) **không được hỗ trợ** ở giai đoạn MVP để đơn giản hóa engine chặn.
  - Subdomain được tính riêng: `mail.google.com` và `docs.google.com` là 2 mục riêng biệt.
  - URLs có thể có hoặc không có `https://` — hệ thống tự chuẩn hóa về dạng domain trần.
- **App Rules — Nhận diện ứng dụng:**
  - Hỗ trợ 2 cách thêm app:
    - **Theo đường dẫn file thực thi** (VD: `C:\Program Files\Code\code.exe`) — chính xác nhưng phụ thuộc vào nơi cài đặt.
    - **Theo process name** (VD: `code`) — linh hoạt hơn, không cần biết path. Hệ thống so sánh với tên process đang chạy trong Task Manager.
  - Người dùng có thể dùng nút **"Browse..."** để chọn file `.exe` — hệ thống tự điền cả path và process name.
- **Giới hạn entries:**
  - Mỗi Profile tối đa **50 entries** (website + app cộng lại). Nếu vượt quá, nút "Thêm" bị disable và hiển thị: *"Profile đã đạt giới hạn 50 mục."*
- **Validation Rules — Tên Profile:**
  - Không được để trống. Độ dài tối đa: **100 ký tự**.
- **Luồng ngoại lệ — URL không hợp lệ:**
  - Nếu nhập sai format hoặc thiếu TLD, hệ thống highlight đỏ ô input và hiển thị gợi ý cú pháp đúng.
- **Luồng ngoại lệ — Trùng tên Profile:**
  - Nếu người dùng đặt tên Profile trùng với một Profile đã có, hệ thống từ chối và hiển thị: *"Tên Profile đã tồn tại. Vui lòng chọn tên khác."*
- **Luồng ngoại lệ — Sửa/Xóa Profile đang dùng trong phiên Focus:**
  - Hệ thống từ chối và hiển thị: *"Profile này đang được dùng trong phiên Focus. Không thể chỉnh sửa cho đến khi phiên kết thúc."*
  - Mọi thay đổi với Profile chỉ có hiệu lực ở phiên **tiếp theo**.
- **Luồng ngoại lệ — Xóa Profile đang được gán cho Task `Pending` hoặc `Abandoned`:**
  - Hệ thống hiển thị cảnh báo: *"Profile này đang được gán cho [N] task chưa thực hiện. Nếu xóa, các task đó sẽ tự động fallback về Default Profile. Tiếp tục?"*
  - Nếu xác nhận, hệ thống xóa Profile và tự động cập nhật các task bị ảnh hưởng sang Default Profile.
- **Khái niệm Profile Mặc định (Default Profile):**
  - Hệ thống luôn duy trì một **"Default Profile"** không thể xóa, chỉ có thể sửa nội dung.
  - Task không gán Profile → tự động dùng Default Profile (chỉ cho phép mở chính app Don't Be Lazy, không có internet).
  - Default Profile hiển thị badge **"Mặc định"** trên UI để nhắc nhở người dùng chưa cấu hình Whitelist.
- **Luồng thay thế — Import/Export Profile:**
  - Người dùng vào Settings → **"Whitelist Profiles"** → Bấm **"Export"** để xuất toàn bộ danh sách Profiles ra file `.json`.
  - Người dùng bấm **"Import"** để nạp lại file `.json` đã export → hệ thống merge với danh sách hiện có.
  - **Conflict resolution khi import:** Nếu Profile trong file trùng tên với Profile đang có, hệ thống hỏi từng trường hợp: **Ghi đè** (xóa toàn bộ entries cũ của Profile đó, thay bằng entries từ file) / **Bỏ qua** (giữ nguyên Profile cũ, bỏ Profile trong file).
  - Dùng cho mục đích backup hoặc khôi phục sau khi cài lại app.

### UC03: Bắt đầu phiên tập trung (Focus Mode)
- **Tác nhân:** Người dùng
- **Mô tả:** Kích hoạt quá trình tính thời gian và hệ thống sẽ tự động bật khiên chắn Block mọi app/web nằm ngoài Whitelist.
- **Tiền điều kiện:** Phải có ít nhất 1 task ở trạng thái `Pending` hoặc `Abandoned` (đã bấm "Thử lại") trong danh sách. Nếu danh sách trống (VD: tất cả Weekly task hôm nay không phải ngày của chúng), màn hình hiển thị Empty State với gợi ý: *"Không có task khả dụng hôm nay. Thêm task mới hoặc kiểm tra lại lịch Recurring."*
- **Luồng sự kiện chính:**
  1. Tại màn hình chính, người dùng chọn task cần thực hiện.
  2. Hệ thống áp dụng trick **"Lời hứa chủ động"**: Yêu cầu đặt thời gian đếm ngược và tự tay gõ câu cam kết mục tiêu rồi mới được bấm "Start Focus".
     - **Validation thời gian:** Tối thiểu **1 phút**, tối đa **240 phút** (4 giờ). Nếu nhập ngoài khoảng này, ô input highlight đỏ và hiển thị giới hạn cho phép.
     - **Hiển thị cấu hình Strict Mode áp dụng:** Ngay trên màn hình này, hệ thống hiển thị rõ: *"Phiên này sẽ chạy với Strict Mode: [BẬT/TẮT]"* (theo thứ tự ưu tiên per-task > global) để người dùng biết trước.
  3. Hệ thống bắt đầu bấm giờ đếm ngược. Task chuyển trạng thái sang `Active`.
  4. Hệ thống gọi kịch bản xử lý ngầm để kích hoạt khiên chặn toàn bộ internet và ứng dụng lạ, sau đó đọc **Whitelist Profile gắn với Task hiện tại** để cấp quyền chạy ngoại lệ.
  5. Khi hết thời gian đếm ngược, hệ thống phát âm thông báo, thả ngắt chặn web/app, hiển thị chúc mừng và cập nhật **cộng dồn điểm Streak**.
- **Luồng thay thế — Hoàn thành sớm hơn timer (Mark Done Early):**
  - Trong lúc đếm giờ, nếu người dùng đã xong việc trước khi hết giờ, có thể bấm nút **"Hoàn thành sớm ✓"** (khác với nút Stop/Give up).
  - Hệ thống hiện xác nhận: *"Bạn đã hoàn thành task trước thời hạn? Điều này sẽ kết thúc phiên và đánh dấu task là Done."* → Bấm "Xác nhận".
  - Hệ thống kết thúc phiên, ghi nhận thời gian thực tế đã tập trung, mở khóa app/web, Task → `Done`, Streak được cộng bình thường.
  - **Với Recurring Task:** Hệ thống ghi `last_done_date = hôm nay`. Task không reset ngay — chờ đến lần kiểm tra tiếp theo mới về `Pending`.
- **Luồng thay thế — Nhiều phiên Focus cho 1 Task (Multi-Session):**
  - Một task `Pending` có thể được focus nhiều lần (VD: 3 phiên Pomodoro). Mỗi phiên tạo ra 1 bản ghi session riêng trong DB.
  - Analytics hiển thị tổng thời gian tập trung của **tất cả các phiên** gộp lại theo task/theo ngày.
  - Streak chỉ cần có ít nhất 1 phiên **hoàn thành** trong ngày, không cần tất cả phiên đều thành công.
- **Luồng thay thế — Dừng sớm (Strict Mode Tắt — Kích hoạt Cản Bước Tâm Lý):**
  - Trong quá trình đếm giờ, người dùng nhấp vào nút "Stop/Give up".
  - Chức năng **Guilt-tripping** và **Loss Aversion** kích hoạt: Hệ thống hiện thông báo đỏ *"Bạn thực sự muốn vứt bỏ công sức và mất đi chuỗi Streak [X] ngày sao?"*.
  - Chức năng **Tạo ma sát** kích hoạt: Yêu cầu người dùng tự gõ chính xác chuỗi **(1)** *"Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc"* mới có thể bấm nút Tắt. *(Lưu ý: Đây là chuỗi Friction riêng cho ngữ cảnh bỏ cuộc Focus — khác với chuỗi Friction tắt Strict Mode trong UC04).*
  - Sau khi gõ xong: hệ thống hủy phiên, mở khóa ứng dụng, Reset Streak về 0, Task chuyển sang trạng thái **`Abandoned`** (hiển thị badge cam trong danh sách).
- **Luồng ngoại lệ — Strict Mode Bật:**
  - Nút "Stop" bị Disable hoặc ẩn hoàn toàn. Mọi cơ chế đóng/kill app bị từ chối.
- **Luồng ngoại lệ — Máy tính sleep / tắt màn hình giữa phiên:**
  - Timer được **tạm dừng tự động** khi máy vào trạng thái sleep/hibernate/screen lock.
  - Khi máy thức dậy, hệ thống hiển thị hộp thoại:
    > *"Phiên Focus của bạn đã bị gián đoạn. Bạn muốn: **Tiếp tục** (tiếp tục đếm ngược từ chỗ dừng) hay **Đặt lại** (xóa bỏ phiên hiện tại)?"*
  - Thời gian sleep không được tính vào Analytics.
  - Trong Strict Mode: luôn tự động **Tiếp tục**, không hiển thị hộp thoại chọn.
- **Luồng ngoại lệ — Orphan Session (app bị force-close ngoài Strict Mode):**
  - Nếu app bị tắt đột ngột (không phải Strict Mode), checkpoint vẫn được ghi trong DB.
  - Khi app khởi động lại, nếu phát hiện checkpoint chưa kết thúc của phiên **không phải Strict**: hệ thống hiển thị: *"Có một phiên Focus chưa hoàn thành trước đó. Bạn muốn: **Khôi phục** hay **Bỏ qua** (xóa phiên đó)?"*
  - Nếu "Bỏ qua": phiên bị xóa, task về `Abandoned`. Nếu "Khôi phục": timer tiếp tục từ checkpoint cuối.
  - **Với Recurring Task bị đưa về `Abandoned` do Orphan Session:** Vòng reset vẫn hoạt động bình thường — nếu đến ngày reset, task tự về `Pending` theo rule Recurring.
- **Hậu điều kiện:** Lưu lại thời gian tập trung thực tế và trạng thái task cuối cùng vào Local DB.

### UC04: Quản lý cấu hình Strict Mode
- **Tác nhân:** Người dùng
- **Mô tả:** Cấu hình chế độ "kỷ luật thép" — có thể bật toàn cục hoặc per-task. Khi Strict Mode kích hoạt, mọi cơ chế dừng/can thiệp bị chặn hoàn toàn.
- **Tiền điều kiện:** Chỉ được tinh chỉnh khi hệ thống **CHƯA** trong trạng thái Focus đang chạy.
- **Phân cấp Strict Mode:**
  - **Global Strict Mode:** Áp dụng cho tất cả các phiên Focus. Cấu hình trong Settings → Strict Mode.
  - **Per-Task Strict Mode:** Cho phép bật/tắt Strict Mode riêng cho từng task khi tạo hoặc sửa task (ghi đè Global).
  - Thứ tự ưu tiên: **Per-task setting > Global setting**.
  - **Ví dụ cụ thể:** Global = ON, Task "Đọc email" có Per-Task = OFF → khi Start Focus trên task đó, phiên chạy **không có Strict Mode**. Ngược lại, Global = OFF, Task "Học thi" có Per-Task = ON → phiên chạy **có Strict Mode**. Người dùng được thông báo rõ trên màn hình xác nhận trước khi bắt đầu (UC03 bước 2).

#### Luồng bật Strict Mode (OFF → ON):
  1. Người dùng vào Settings → toggle "Strict Mode" sang ON (hoặc bật trong form tạo/sửa task).
  2. Hệ thống hiển thị hộp thoại xác nhận cảnh báo:
     > *"Khi bật Strict Mode trong phiên làm việc, bạn sẽ KHÔNG THỂ: dừng sớm, sửa Whitelist, hay kill app bằng Task Manager. Phiên chỉ kết thúc khi hết giờ hoặc hoàn thành task. Bạn có chắc chắn?"*
  3. Người dùng xác nhận "Đồng ý". Hệ thống lưu trạng thái. UI hiển thị badge ⚔️ cảnh báo.

#### Luồng tắt Strict Mode (ON → OFF):
  1. Người dùng toggle "Strict Mode" sang OFF.
  2. Hệ thống xác nhận 2 bước:
     - **Bước 1:** *"Bạn có chắc muốn tắt Strict Mode? Điều này sẽ cho phép bỏ cuộc dễ dàng hơn."* → Bấm "Tiếp tục".
     - **Bước 2:** Tự tay gõ chuỗi **(2)** *"Tôi chấp nhận giảm mức độ kỷ luật"* → Bấm "Xác nhận tắt". *(Lưu ý: Đây là chuỗi Friction riêng cho ngữ cảnh tắt Strict Mode — khác với chuỗi Friction bỏ cuộc Focus trong UC03).*
  3. Hệ thống lưu `strict_mode = false`. Badge cảnh báo biến mất.

#### Luồng ngoại lệ — Đang trong phiên Focus:
  - Toggle Strict Mode bị **disable hoàn toàn** — không thể thay đổi cho đến khi phiên kết thúc.

#### Luồng ngoại lệ — Máy tắt/restart đột ngột trong Strict Mode (Crash Recovery):
  - Hệ thống ghi một **session checkpoint** vào Local DB mỗi 30 giây trong lúc Focus (lưu: thời gian còn lại, task ID, strict state).
  - Khi ứng dụng khởi động lại sau crash/restart, hệ thống kiểm tra DB xem có checkpoint chưa kết thúc không.
  - Nếu tìm thấy checkpoint của phiên **Strict Mode** chưa hoàn thành:
    - Hệ thống **tự động kích hoạt lại chế độ chặn** ngay khi app load xong.
    - Hiển thị banner: *"Phiên Strict Mode của bạn đã bị gián đoạn do máy tắt. Hệ thống đã khôi phục lại — còn [X] phút (±30 giây do gián đoạn)."* *(Sai lệch tối đa 30 giây do khoảng cách giữa 2 checkpoint — người dùng được thông báo rõ.)*
    - Timer tiếp tục đếm ngược từ thời điểm checkpoint cuối cùng.
  - Nếu tìm thấy checkpoint của phiên **không Strict** chưa hoàn thành → xem UC03 luồng "Orphan Session".

### UC05: Xem báo cáo & thống kê
- **Tác nhân:** Người dùng
- **Mô tả:** Xem lại lịch sử kỷ luật của bản thân để đánh giá hành trình, nhìn thấy tiến bộ và duy trì động lực.
- **Tiền điều kiện:** Ứng dụng đang mở.
- **Múi giờ & ranh giới ngày:**
  - Tất cả thời gian lưu vào DB theo **Local Time của máy người dùng**.
  - Ranh giới ngày là **00:00 Local Time**. Phiên tính theo **ngày bắt đầu** — phiên bắt đầu lúc 23:30 và kết thúc lúc 00:10 thuộc ngày 23:30.
- **Luồng sự kiện chính:**
  1. Người dùng điều hướng tới tab "Thống kê" (Analytics/Dashboard).
  2. Hệ thống mặc định hiển thị dữ liệu của **7 ngày gần nhất**.
  3. Người dùng có thể chọn bộ lọc thời gian: **Hôm nay / 7 ngày / 30 ngày / Tất cả**.
  4. Hệ thống query Local DB theo bộ lọc đã chọn và render các chỉ số:
     - 📊 **Biểu đồ cột** — Tổng thời gian tập trung thực tế theo từng ngày (không tính thời gian sleep).
     - 🔥 **Streak Counter** — Chuỗi ngày liên tục có ít nhất 1 phiên hoàn thành. Hiển thị nổi bật ở đầu trang.
     - 🚫 **Số lần bị chặn** — Tổng số lần cố truy cập app/web nằm ngoài Whitelist.
     - ✅ **Tỷ lệ hoàn thành** — Số phiên hoàn thành / tổng số phiên đã bắt đầu (%).
- **Recurring Task trong Analytics:**
  - Mỗi phiên Focus của recurring task được lưu là **1 bản ghi riêng** theo ngày — không group tự động theo task name.
  - Người dùng có thể dùng bộ lọc **"Theo task"** (chọn tên task từ dropdown) để xem tổng số phiên và tổng thời gian của 1 recurring task qua nhiều ngày.
- **Logic tính Streak:**
  - Mỗi ngày dương lịch (theo Local Time) có ít nhất 1 phiên Focus **hoàn thành** (kể cả "Hoàn thành sớm") → cộng 1 ngày vào Streak.
  - Nếu không có phiên hoàn thành nào trong ngày hôm qua → Streak **reset về 0**.
  - Streak được cập nhật tự động sau mỗi phiên kết thúc.
- **Luồng thay thế — Xóa lịch sử:**
  - Người dùng vào Settings → **"Dữ liệu & Quyền riêng tư"** → Chọn "Xóa lịch sử".
  - Hệ thống cho phép chọn phạm vi xóa: **Hôm nay / 7 ngày / 30 ngày / Tất cả**.
  - Sau khi xác nhận, hệ thống xóa các bản ghi session tương ứng.
  - **Logic Streak sau khi xóa một phần:** Hệ thống **tính lại Streak từ dữ liệu còn lại** (không reset về 0 nếu còn ngày hoàn thành liên tiếp trước phạm vi bị xóa). VD: Xóa "7 ngày gần nhất" nhưng trước đó đã có 14 ngày streak → Streak được cập nhật thành 7 (từ ngày trước phạm vi xóa).
  - *Lưu ý thiết kế: Tính năng đặt sâu trong Settings để tránh người dùng bốc đồng xóa nhầm.*
- **Luồng thay thế — Export dữ liệu (Session Export):**
  - Người dùng vào Settings → **"Xuất dữ liệu"** → Chọn định dạng **CSV** hoặc **JSON**.
  - **Scope của export:** Chỉ xuất **session history** (task name, thời gian bắt đầu, thời gian thực tế, trạng thái hoàn thành). Không bao gồm Tasks và Whitelist Profiles (dùng tính năng Import/Export Profile trong UC02 cho mục đích đó).
  - Hệ thống mở hộp thoại Save As của hệ điều hành để người dùng chọn nơi lưu.
- **Luồng ngoại lệ — Empty State (Lần đầu sử dụng, chưa có dữ liệu):**
  - Nếu Local DB chưa có phiên nào, hệ thống hiển thị màn hình **Empty State** với:
    - Icon minh họa (đồng hồ cát hoặc rocket chưa phóng).
    - Dòng chữ: *"Bạn chưa có phiên làm việc nào. Hãy bắt đầu phiên đầu tiên để xây dựng Streak!"*
    - Nút CTA *"Bắt đầu ngay"* dẫn thẳng về màn hình chọn Task.

### UC06: Hiển thị Motivation Quote
- **Tác nhân:** Hệ thống (kích hoạt tự động theo sự kiện), Người dùng (có thể quản lý kho quote).
- **Mô tả:** Hệ thống tự động hiển thị các câu quote truyền cảm hứng tại các **thời điểm chiến lược** trong hành trình làm việc nhằm duy trì động lực và củng cố thói quen tốt.
- **Tiền điều kiện:** Ứng dụng đang mở **và** tính năng Quote chưa bị tắt trong Settings.
- **Luồng sự kiện chính (Tự động theo sự kiện):**
  - **Sự kiện 1 — Trước khi bắt đầu:** Ngay sau khi người dùng gõ xong câu cam kết mục tiêu, hệ thống hiển thị 1 quote ngắn tiếp thêm fuel: VD: *"The secret of getting ahead is getting started." — Mark Twain*.
  - **Sự kiện 2 — Trong lúc tập trung (giữa phiên):** Khi đã trôi qua khoảng 50% thời gian **và** thời gian còn lại ≥ 2 phút, hệ thống hiện một toast nhỏ góc màn hình. Toast **tự động biến mất sau 5 giây** mà không cần người dùng bấm tắt. Người dùng có thể click vào toast để dismiss ngay lập tức. Nếu user "Mark Done Early" trước mốc 50%, sự kiện 2 bị bỏ qua hoàn toàn — không fallback.
  - **Sự kiện 3 — Khi hoàn thành phiên:** Sau khi đồng hồ về 0 (hoặc Mark Done Early), cùng màn hình chúc mừng xuất hiện một câu quote ăn mừng chiến thắng: VD: *"Well done is better than well said." — Benjamin Franklin*.
  - **Sự kiện 4 — Khi người dùng định bỏ cuộc (Stop sớm):** Trong luồng "Tạo ma sát", trước khi hiện ô gõ tự nhận thua, hệ thống flash một quote đánh thẳng vào sự tự ái: VD: *"Push yourself, because no one else is going to do it for you."*.
- **Luồng thay thế — Người dùng tự quản lý kho Quote:**
  1. Người dùng vào Settings → **"Motivation Quotes"**.
  2. Hệ thống hiển thị danh sách quote mặc định (bundle sẵn, có thể xem theo sự kiện).
  3. Người dùng có thể **thêm quote cá nhân**, **sửa**, hoặc **xóa** bất kỳ quote nào.
  4. Người dùng có thể assign quote vào từng **sự kiện cụ thể** (trước phiên / giữa phiên / khi hoàn thành / khi bỏ cuộc) hoặc để chế độ **ngẫu nhiên (random)**.
  5. Người dùng có thể chọn **ngôn ngữ hiển thị quote mặc định**: Tiếng Việt / Tiếng Anh. Bundle sẵn có quote cho cả 2 ngôn ngữ.
  6. Người dùng có thể **tắt toàn bộ hệ thống Quote** bằng toggle "Hiển thị Motivation Quotes" (OFF). Khi tắt, không có quote nào được hiển thị ở bất kỳ sự kiện nào.
  7. Hệ thống lưu danh sách, cài đặt ngôn ngữ và trạng thái bật/tắt vào Local DB.
- **Luồng ngoại lệ — Kho quote của một sự kiện bị rỗng:**
  - Nếu người dùng xóa hết toàn bộ quote của một sự kiện cụ thể, hệ thống **không hiển thị lỗi và không crash**.
  - Thay vào đó hệ thống tự động **fallback về 1 quote hard-coded mặc định** cho sự kiện đó (không thể xóa). VD: *"Keep going."*
  - UI hiển thị gợi ý: *"Kho quote của mục này đang trống. Thêm quote hoặc bật lại quote mặc định."*
- **Hậu điều kiện:** Kho quote, cài đặt ngôn ngữ và trạng thái bật/tắt được lưu cục bộ, sẵn sàng render ở lần tiếp theo.
