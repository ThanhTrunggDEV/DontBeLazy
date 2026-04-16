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
  2. Hệ thống hiển thị danh sách các task hiện có, **sắp xếp theo thứ tự tạo mới nhất ở trên** (mặc định). Người dùng có thể kéo thả (drag-and-drop) để sắp xếp lại theo ý muốn.
  3. Người dùng nhập tên công việc, thiết lập thời gian (VD: 60 phút) và **gán một bộ Whitelist Profile** cấu hình riêng cho Task này.
  4. Người dùng bấm "Thêm".
  5. Hệ thống validate dữ liệu đầu vào (xem Validation Rules bên dưới), sau đó lưu kết nối (Task <-> Whitelist) vào Local DB và cập nhật hiển thị.
- **Luồng thay thế — Sửa Task:**
  - Người dùng nhấp "Sửa" trên một task ở trạng thái `Pending`, `Done` hoặc `Abandoned`.
  - Hệ thống mở form Sửa với các trường: **Tên task**, **Thời gian ước tính**, **Whitelist Profile được gán**. Tất cả đều có thể chỉnh sửa.
  - Sau khi lưu, hệ thống cập nhật bản ghi trong Local DB mà không thay đổi trạng thái hiện tại của task.
- **Luồng thay thế — Hoàn thành Task:**
  - Người dùng click checkbox "Hoàn thành". Hệ thống gạch ngang task và chuyển trạng thái sang `Done`.
- **Trạng thái của Task (Task State Machine):**
  | Trạng thái | Ý nghĩa |
  |---|---|
  | `Pending` | Task đã tạo, chưa bắt đầu phiên Focus nào |
  | `Active` | Đang được chọn để thực hiện trong phiên Focus hiện tại |
  | `Done` | Đã được đánh dấu hoàn thành bởi người dùng |
  | `Abandoned` | Phiên kết thúc sớm (người dùng bỏ cuộc), task tự trả về `Pending` |
  - Task ở trạng thái `Active` sẽ **disable** nút Xóa và nút Sửa trên UI.
  - Task chỉ được Xóa vĩnh viễn khi ở trạng thái `Pending`, `Done` hoặc `Abandoned`.
- **Validation Rules — Tên Task:**
  - Không được để trống hoặc chỉ có khoảng trắng → hiển thị lỗi *"Tên task không được để trống."*
  - Độ dài tối đa: **200 ký tự** → nếu vượt quá, hệ thống highlight đỏ và hiển thị bộ đếm ký tự còn lại.
- **Luồng ngoại lệ — Xóa task đang ở trạng thái `Active`:**
  - Hệ thống từ chối và hiển thị: *"Không thể xóa task đang trong phiên Focus. Hãy kết thúc hoặc hủy phiên hiện tại trước."*
- **Hậu điều kiện:** Dữ liệu task và trạng thái được lưu trữ cục bộ trên máy.

### UC02: Quản lý bộ quy tắc ưu tiên (Whitelist Profiles)
- **Tác nhân:** Người dùng
- **Mô tả:** Người dùng tạo trước các tệp hồ sơ (Profiles) tập hợp các ứng dụng và trang web được phép chạy (VD: Profile "Lập trình", Profile "Học Ngoại Ngữ"). Các Profile này nhằm mục đích gán nhanh cho mỗi Task cụ thể sau này.
- **Tiền điều kiện:** KHÔNG ở trong trạng thái Strict Mode của một Task đang dùng Profile đó.
- **Luồng sự kiện chính:**
  1. Chọn mục cài đặt "Whitelist Profiles" → Bấm tạo mới Profile.
  2. Người dùng đặt tên Profile và chọn tab "Websites" hoặc "Apps".
  3. Nhập URL theo quy tắc định dạng (xem URL Rules bên dưới) hoặc trỏ đường dẫn tới file thực thi (VD: `code.exe`).
  4. Bấm "Thêm". Hệ thống lưu Profile vào danh sách cục bộ để sẵn sàng gán cho Task.
- **URL Rules — Định dạng cho phép:**
  - Chỉ hỗ trợ **exact domain** (VD: `github.com`, `docs.google.com`). Ký tự wildcard (`*`) **không được hỗ trợ** ở giai đoạn MVP để đơn giản hóa engine chặn.
  - Subdomain được tính riêng: `mail.google.com` và `docs.google.com` là 2 mục riêng biệt.
  - URLs có thể có hoặc không có `https://` — hệ thống tự chuẩn hóa về dạng domain trần.
- **Luồng ngoại lệ — URL không hợp lệ:**
  - Nếu nhập sai format hoặc thiếu TLD, hệ thống highlight đỏ ô input và hiển thị gợi ý cú pháp đúng.
- **Luồng ngoại lệ — Trùng tên Profile:**
  - Nếu người dùng đặt tên Profile trùng với một Profile đã có, hệ thống từ chối và hiển thị: *"Tên Profile đã tồn tại. Vui lòng chọn tên khác."*
- **Luồng ngoại lệ — Sửa/Xóa Profile đang dùng trong phiên Focus:**
  - Hệ thống từ chối và hiển thị: *"Profile này đang được dùng trong phiên Focus. Không thể chỉnh sửa cho đến khi phiên kết thúc."*
  - Mọi thay đổi với Profile chỉ có hiệu lực ở phiên **tiếp theo**.
- **Luồng ngoại lệ — Xóa Profile đang được gán cho Task `Pending`:**
  - Hệ thống hiển thị cảnh báo: *"Profile này đang được gán cho [N] task chưa thực hiện. Nếu xóa, các task đó sẽ tự động fallback về Default Profile. Tiếp tục?"*
  - Nếu xác nhận, hệ thống xóa Profile và tự động cập nhật các task bị ảnh hưởng sang Default Profile.
- **Khái niệm Profile Mặc định (Default Profile):**
  - Hệ thống luôn duy trì một **"Default Profile"** không thể xóa, chỉ có thể sửa nội dung.
  - Task không gán Profile → tự động dùng Default Profile (chỉ cho phép mở chính app Don't Be Lazy, không có internet).
  - Default Profile hiển thị badge **"Mặc định"** trên UI để nhắc nhở người dùng chưa cấu hình Whitelist.

### UC03: Bắt đầu phiên tập trung (Focus Mode)
- **Tác nhân:** Người dùng
- **Mô tả:** Kích hoạt quá trình tính thời gian và hệ thống sẽ tự động bật khiên chắn Block mọi app/web nằm ngoài Whitelist.
- **Tiền điều kiện:** Phải có ít nhất 1 task trong danh sách công việc chưa hoàn thành.
- **Luồng sự kiện chính:**
  1. Tại màn hình chính, người dùng chọn task cần thực hiện.
  2. Hệ thống áp dụng trick **"Lời hứa chủ động"**: Yêu cầu đặt thời gian đếm ngược và tự tay gõ câu cam kết mục tiêu rồi mới được bấm "Start Focus".
     - **Validation thời gian:** Tối thiểu **1 phút**, tối đa **240 phút** (4 giờ). Nếu nhập ngoài khoảng này, ô input highlight đỏ và hiển thị giới hạn cho phép.
  3. Hệ thống bắt đầu bấm giờ đếm ngược. Task chuyển trạng thái sang `Active`.
  4. Hệ thống gọi kịch bản xử lý ngầm để kích hoạt khiên chặn toàn bộ internet và ứng dụng lạ, sau đó đọc **Whitelist Profile gắn với Task hiện tại** để cấp quyền chạy ngoại lệ.
  5. Khi hết thời gian đếm ngược, hệ thống phát âm thông báo, thả ngắt chặn web/app, hiển thị chúc mừng và cập nhật **cộng dồn điểm Streak**.
- **Luồng thay thế — Hoàn thành sớm hơn timer (Mark Done Early):**
  - Trong lúc đếm giờ, nếu người dùng đã xong việc trước khi hết giờ, có thể bấm nút **"Hoàn thành sớm ✓"** (khác với nút Stop/Give up).
  - Hệ thống hiện xác nhận: *"Bạn đã hoàn thành task trước thời hạn? Điều này sẽ kết thúc phiên và đánh dấu task là Done."* → Bấm "Xác nhận".
  - Hệ thống kết thúc phiên, ghi nhận thời gian thực tế đã tập trung, mở khóa app/web, Task → `Done`, Streak được cộng bình thường.
- **Luồng thay thế — Nhiều phiên Focus cho 1 Task (Multi-Session):**
  - Một task `Pending` có thể được focus nhiều lần (VD: 3 phiên Pomodoro). Mỗi phiên tạo ra 1 bản ghi session riêng trong DB.
  - Analytics hiển thị tổng thời gian tập trung của **tất cả các phiên** gộp lại theo task/theo ngày.
  - Streak chỉ cần có ít nhất 1 phiên **hoàn thành** trong ngày, không cần tất cả phiên đều thành công.
- **Luồng thay thế (Dừng sớm khi Strict Mode Đang Tắt - Kích hoạt Cản Bước Tâm Lý):**
  - Trong quá trình đếm giờ, người dùng nhấp vào nút "Stop/Give up".
  - Chức năng **Guilt-tripping** và **Loss Aversion** kích hoạt: Hệ thống hiện thông báo đỏ *"Bạn thực sự muốn vứt bỏ công sức và mất đi chuỗi Streak [X] ngày sao?"*.
  - Chức năng **Tạo ma sát** kích hoạt: Yêu cầu người dùng tự gõ chính xác dòng chữ *"Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc"* mới có thể bấm nút Tắt.
  - Sau khi gõ xong, hệ thống mới huỷ phiên làm việc, mở khóa ứng dụng và tự động Reset Streak về 0.
- **Luồng ngoại lệ (Khi Strict Mode đang Bật):** 
  - Nút Stop sẽ bị Disable, ẩn hoàn toàn hoặc thông báo bị từ chối thoát mọi hình thức.
- **Luồng ngoại lệ — Máy tính sleep / tắt màn hình giữa phiên:**
  - Nếu hệ điều hành đưa máy vào trạng thái ngủ (sleep/hibernate) hoặc tắt màn hình (screen lock), timer được **tạm dừng tự động**.
  - Khi máy thức dậy và người dùng đăng nhập lại, hệ thống hiển thị hộp thoại:
    > *"Phiên Focus của bạn đã bị gián đoạn. Bạn muốn: **Tiếp tục** (tiếp tục đếm ngược từ chỗ dừng) hay **Đặt lại** (xóa bỏ phiên hiện tại)?"*
  - Thời gian sleep không được tính vào thời gian tập trung thực tế trong Analytics.
  - Trong Strict Mode: luôn tự động **Tiếp tục** khi máy thức dậy, không hiển thị hộp thoại chọn.
- **Hậu điều kiện:** Lưu lại tổng thời gian tập trung vừa qua vào Local DB.

### UC04: Quản lý cấu hình Strict Mode
- **Tác nhân:** Người dùng
- **Mô tả:** Cấu hình chế độ "kỷ luật thép" — có thể bật toàn cục hoặc per-task. Khi Strict Mode kích hoạt, mọi cơ chế dừng/can thiệp bị chặn hoàn toàn.
- **Tiền điều kiện:** Chỉ được tinh chỉnh khi hệ thống **CHƯA** trong trạng thái Focus đang chạy.
- **Phân cấp Strict Mode:**
  - **Global Strict Mode:** Áp dụng cho tất cả các phiên Focus. Cấu hình trong Settings → Strict Mode.
  - **Per-Task Strict Mode:** Cho phép bật/tắt Strict Mode riêng cho từng task khi tạo hoặc sửa task (ghi đè Global). VD: Task "Học thi" bật Strict, Task "Đọc email" tắt Strict.
  - Thứ tự ưu tiên: Per-task setting > Global setting.

#### Luồng bật Strict Mode (OFF → ON):
  1. Người dùng vào Settings → toggle "Strict Mode" sang ON (hoặc bật trong form tạo/sửa task).
  2. Hệ thống hiển thị hộp thoại xác nhận cảnh báo:
     > *"Khi bật Strict Mode trong phiên làm việc, bạn sẽ KHÔNG THỂ: dừng sớm, sửa Whitelist, hay kill app bằng Task Manager. Phiên chỉ kết thúc khi hết giờ hoặc hoàn thành task. Bạn có chắc chắn?"*
  3. Người dùng xác nhận "Đồng ý". Hệ thống lưu trạng thái. UI hiển thị badge ⚔️ cảnh báo.

#### Luồng tắt Strict Mode (ON → OFF):
  1. Người dùng toggle "Strict Mode" sang OFF.
  2. Hệ thống xác nhận 2 bước:
     - **Bước 1:** *"Bạn có chắc muốn tắt Strict Mode? Điều này sẽ cho phép bỏ cuộc dễ dàng hơn."* → Bấm "Tiếp tục".
     - **Bước 2:** Tự tay gõ: *"Tôi chấp nhận giảm mức độ kỷ luật"* → Bấm "Xác nhận tắt".
  3. Hệ thống lưu `strict_mode = false`. Badge cảnh báo biến mất.

#### Luồng ngoại lệ — Đang trong phiên Focus:
  - Toggle Strict Mode bị **disable hoàn toàn** — không thể thay đổi cho đến khi phiên kết thúc.

#### Luồng ngoại lệ — Máy tắt/restart đột ngột trong Strict Mode (Crash Recovery):
  - Hệ thống ghi một **session checkpoint** vào Local DB mỗi 30 giây trong lúc Focus (lưu: thời gian còn lại, task ID, strict state).
  - Khi ứng dụng khởi động lại sau crash/restart, hệ thống kiểm tra DB xem có checkpoint chưa kết thúc không.
  - Nếu tìm thấy checkpoint của phiên Strict Mode chưa hoàn thành:
    - Hệ thống **tự động kích hoạt lại chế độ chặn** ngay khi app load xong.
    - Hiển thị banner: *"Phiên Strict Mode của bạn đã bị gián đoạn do máy tắt. Hệ thống đã khôi phục lại — còn [X] phút."*
    - Timer tiếp tục đếm ngược từ thời điểm checkpoint cuối cùng.

### UC05: Xem báo cáo & thống kê
- **Tác nhân:** Người dùng
- **Mô tả:** Xem lại lịch sử kỷ luật của bản thân để đánh giá hành trình, nhìn thấy tiến bộ và duy trì động lực.
- **Tiền điều kiện:** Ứng dụng đang mở.
- **Luồng sự kiện chính:**
  1. Người dùng điều hướng tới tab "Thống kê" (Analytics/Dashboard).
  2. Hệ thống mặc định hiển thị dữ liệu của **7 ngày gần nhất**.
  3. Người dùng có thể chọn bộ lọc thời gian: **Hôm nay / 7 ngày / 30 ngày / Tất cả**.
  4. Hệ thống query Local DB theo bộ lọc đã chọn và render các chỉ số:
     - 📊 **Biểu đồ cột** — Tổng thời gian tập trung thực tế theo từng ngày (chỉ tính thời gian thực sự focus, không tính thời gian sleep).
     - 🔥 **Streak Counter** — Chuỗi ngày liên tục có ít nhất 1 phiên hoàn thành. Hiển thị nổi bật ở đầu trang.
     - 🚫 **Số lần bị chặn** — Tổng số lần cố truy cập app/web nằm ngoài Whitelist.
     - ✅ **Tỷ lệ hoàn thành** — Số phiên hoàn thành / tổng số phiên đã bắt đầu (%).
- **Logic tính Streak:**
  - Mỗi ngày dương lịch có ít nhất 1 phiên Focus **hoàn thành** (kể cả "Hoàn thành sớm") → cộng 1 ngày vào Streak.
  - Nếu không có phiên hoàn thành nào trong ngày hôm qua → Streak **reset về 0**.
  - Streak được cập nhật tự động sau mỗi phiên kết thúc.
- **Luồng thay thế — Xóa lịch sử:**
  - Người dùng vào Settings → **"Dữ liệu & Quyền riêng tư"** → Chọn "Xóa lịch sử".
  - Hệ thống cho phép chọn phạm vi xóa: **Hôm nay / 7 ngày / 30 ngày / Tất cả**.
  - Sau khi xác nhận, hệ thống xóa các bản ghi session tương ứng và **reset Streak về 0**.
  - *Lưu ý thiết kế: Tính năng này được đặt sâu trong Settings (không phải trực tiếp trên màn hình Analytics) để tránh người dùng bốc đồng xóa nhầm.* 
- **Luồng thay thế — Export dữ liệu:**
  - Người dùng vào Settings → **"Xuất dữ liệu"** → Chọn định dạng **CSV** hoặc **JSON**.
  - Hệ thống xuất toàn bộ lịch sử phiên Focus (task name, thời gian bắt đầu, thời gian thực tế, trạng thái hoàn thành) ra file và mở hộp thoại Save As của hệ điều hành.
- **Luồng ngoại lệ — Empty State (Lần đầu sử dụng, chưa có dữ liệu):**
  - Nếu Local DB chưa có phiên nào, hệ thống hiển thị màn hình **Empty State** với:
    - Icon minh họa (đồng hồ cát hoặc rocket chưa phóng).
    - Dòng chữ: *"Bạn chưa có phiên làm việc nào. Hãy bắt đầu phiên đầu tiên để xây dựng Streak!"*
    - Nút CTA *"Bắt đầu ngay"* dẫn thẳng về màn hình chọn Task.

### UC06: Hiển thị Motivation Quote
- **Tác nhân:** Hệ thống (kích hoạt tự động theo sự kiện), Người dùng (có thể quản lý kho quote).
- **Mô tả:** Hệ thống tự động hiển thị các câu quote truyền cảm hứng tại các **thời điểm chiến lược** trong hành trình làm việc nhằm duy trì động lực và củng cố thói quen tốt.
- **Tiền điều kiện:** Ứng dụng đang mở.
- **Luồng sự kiện chính (Tự động theo sự kiện):**
  - **Sự kiện 1 — Trước khi bắt đầu:** Ngay sau khi người dùng gõ xong câu cam kết mục tiêu (Implementation Intention), hệ thống hiển thị 1 quote ngắn tiếp thêm fuel: VD: *"The secret of getting ahead is getting started." — Mark Twain*.
  - **Sự kiện 2 — Trong lúc tập trung (giữa phiên):** Khi đã trôi qua khoảng 50% thời gian, hệ thống hiện một toast nhỏ góc màn hình, không cản giao diện, để cổ vũ tiếp: VD: *"You're halfway there. Don't stop now."*.
  - **Sự kiện 3 — Khi hoàn thành phiên:** Sau khi đồng hồ về 0, cùng màn hình chúc mừng xuất hiện một câu quote ăn mừng chiến thắng: VD: *"Well done is better than well said." — Benjamin Franklin*.
  - **Sự kiện 4 — Khi người dùng định bỏ cuộc (Stop sớm):** Trong luồng "Tạo ma sát", trước khi hiện ô gõ tự nhận thua, hệ thống flash một quote đánh thẳng vào sự tự ái: VD: *"Push yourself, because no one else is going to do it for you."*.
- **Luồng thay thế — Người dùng tự quản lý kho Quote:**
  1. Người dùng vào Settings → **"Motivation Quotes"**.
  2. Hệ thống hiển thị danh sách quote mặc định (bundle sẵn, có thể xem theo sự kiện).
  3. Người dùng có thể **thêm quote cá nhân**, **sửa**, hoặc **xóa** bất kỳ quote nào.
  4. Người dùng có thể assign quote vào từng **sự kiện cụ thể** (trước phiên / giữa phiên / khi hoàn thành / khi bỏ cuộc) hoặc để chế độ **ngẫu nhiên (random)**.
  5. Người dùng có thể chọn **ngôn ngữ hiển thị quote mặc định**: Tiếng Việt / Tiếng Anh. Bundle sẵn có quote cho cả 2 ngôn ngữ.
  6. Hệ thống lưu danh sách và cài đặt ngôn ngữ vào Local DB.
- **Luồng ngoại lệ — Kho quote của một sự kiện bị rỗng:**
  - Nếu người dùng xóa hết toàn bộ quote của một sự kiện cụ thể (VD: xóa hết quote "khi bỏ cuộc"), hệ thống **không hiển thị lỗi và không crash**.
  - Thay vào đó hệ thống tự động **fallback về 1 quote hard-coded mặc định** cho sự kiện đó (không thể xóa). VD: *"Keep going."*
  - UI hiển thị gợi ý: *"Kho quote của mục này đang trống. Thêm quote hoặc bật lại quote mặc định."*
- **Hậu điều kiện:** Kho quote và cài đặt ngôn ngữ được lưu cục bộ, sẵn sàng render ở lần tiếp theo.
