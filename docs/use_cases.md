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
- **Luồng sự kiện chính:**
  1. Người dùng chọn chức năng "Quản lý công việc" (hoặc nhập trực tiếp từ màn hình chính).
  2. Hệ thống hiển thị danh sách các task hiện có.
  3. Người dùng nhập tên bài tập/công việc, thiết lập thời gian (VD: 60 phút) và **gán một bộ Whitelist Profile** (hoặc tạo list ngoại lệ mới) cấu hình riêng cho Task này.
  4. Người dùng bấm "Thêm".
  5. Hệ thống lưu kết nối (Task <-> Whitelist) vào Local DB và cập nhật hiển thị.
- **Luồng thay thế:** 
  - Tại bước 2, người dùng có thể nhấp vào biểu tượng "Sửa" hoặc "Xóa" một task.
  - Khi hoàn thành công việc, người dùng click chọn checkbox "Hoàn thành". Hệ thống gạch ngang task và đưa vào trạng thái Done.
- **Trạng thái của Task (Task State Machine):**
  | Trạng thái | Ý nghĩa |
  |---|---|
  | `Pending` | Task đã tạo, chưa bắt đầu phiên Focus nào |
  | `Active` | Đang được chọn để thực hiện trong phiên Focus hiện tại |
  | `Done` | Đã được đánh dấu hoàn thành bởi người dùng |
  | `Abandoned` | Phiên sớm kết thúc (người dùng bỏ cuộc), task được trả về `Pending` |
  - Task chỉ được Xóa vĩnh viễn khi ở trạng thái `Pending`, `Done` hoặc `Abandoned`.
  - Task ở trạng thái `Active` sẽ disable nút Xóa và nút Sửa trên UI.
- **Luồng ngoại lệ — Xóa task đang chạy Focus Mode:**
  - Nếu người dùng cố xóa một task đang ở trạng thái `Active`, hệ thống từ chối và hiển thị: *"Không thể xóa task đang trong phiên Focus. Hãy kết thúc hoặc hủy phiên hiện tại trước."*
  - Nút "Xóa" của task đang `Active` sẽ bị disable (grayed out) để tránh thao tác nhầm.
- **Hậu điều kiện:** Dữ liệu task và trạng thái được lưu trữ cục bộ trên máy.

### UC02: Quản lý bộ quy tắc ưu tiên (Whitelist Profiles)
- **Tác nhân:** Người dùng
- **Mô tả:** Người dùng tạo trước các tệp hồ sơ (Profiles) tập hợp các ứng dụng và trang web được phép chạy (VD: Profile "Lập trình", Profile "Học Ngoại Ngữ"). Các Profile này nhằm mục đích gán nhanh cho mỗi Task cụ thể sau này.
- **Tiền điều kiện:** KHÔNG ở trong trạng thái Strict Mode của một Task đang dùng Profile đó.
- **Luồng sự kiện chính:**
  1. Chọn mục cài đặt "Whitelist Profiles" -> Bấm tạo mới Profile.
  2. Người dùng đặt tên Profile và chọn tab "Websites" hoặc "Apps".
  3. Nhập URL (VD: `github.com`) hoặc trỏ đường dẫn tới file chạy của ứng dụng (VD: `code.exe` - VS Code).
  4. Bấm "Thêm". Hệ thống lưu Profile ngoại lệ này vào danh sách cục bộ để sẵn sàng gán cho Task.
- **Luồng ngoại lệ — URL không hợp lệ:**
  - Nếu người dùng nhập URL không hợp lệ (sai format, thiếu TLD...), hệ thống highlight đỏ ô input và hiển thị gợi ý cú pháp đúng.
- **Luồng ngoại lệ — Sửa Profile đang được dùng trong phiên Focus:**
  - Nếu người dùng cố sửa hoặc xóa một Profile đang được một task áp dụng trong phiên Focus hiện tại, hệ thống từ chối và hiển thị: *"Profile này đang được dùng trong phiên Focus. Không thể chỉnh sửa cho đến khi phiên kết thúc."*
  - Mọi thay đổi với Profile chỉ có hiệu lực ở phiên **tiếp theo**, không ảnh hưởng ngược lại phiên đang chạy.
- **Khái niệm Profile Mặc định (Default Profile):**
  - Hệ thống luôn duy trì một **"Default Profile"** không thể xóa, chỉ có thể sửa nội dung.
  - Nếu người dùng tạo Task mà **không gán bất kỳ Profile nào**, hệ thống tự động gán Default Profile (chỉ cho phép mở chính app Don't Be Lazy, không có internet và không có app nào khác).
  - Default Profile được hiển thị rõ trong UI bằng badge "Mặc định" để người dùng biết mình chưa cấu hình Whitelist.

### UC03: Bắt đầu phiên tập trung (Focus Mode)
- **Tác nhân:** Người dùng
- **Mô tả:** Kích hoạt quá trình tính thời gian và hệ thống sẽ tự động bật khiên chắn Block mọi app/web nằm ngoài Whitelist.
- **Tiền điều kiện:** Phải có ít nhất 1 task trong danh sách công việc chưa hoàn thành.
- **Luồng sự kiện chính:**
  1. Tại màn hình chính, người dùng chọn task cần thực hiện.
  2. Hệ thống áp dụng trick **"Lời hứa chủ động"**: Yêu cầu đặt thời gian đếm ngược (VD: 25 phút Pomodoro) và tự tay gõ một câu cam kết mục tiêu (VD: *"Tôi sẽ hoàn thành việc code"*) rồi mới được bấm "Start Focus".
  3. Hệ thống bắt đầu bấm giờ đếm ngược.
  4. Hệ thống gọi kịch bản xử lý ngầm để kích hoạt khiên chặn toàn bộ internet và ứng dụng lạ, sau đó hệ thống đọc thông số **Whitelist Profile gắn với Task hiện tại** để cấp quyền chạy ngoại lệ.
  5. Khi hết thời gian đếm ngược, hệ thống phát âm thông báo, thả ngắt chặn web/app, hiển thị chúc mừng và cập nhật **cộng dồn điểm Streak** (chuỗi ngày liên tục).
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
- **Mô tả:** Bật/tắt chế độ "kỷ luật thép" (chặn tối đa, chống mọi sự can thiệp trong phiên làm việc).
- **Tiền điều kiện:** Chỉ được tinh chỉnh khi hệ thống **CHƯA** trong trạng thái Start Focus.

#### Luồng bật Strict Mode (OFF → ON):
  1. Người dùng vào Settings → toggle "Strict Mode" sang ON.
  2. Hệ thống hiển thị hộp thoại xác nhận cảnh báo:
     > *"Khi bật Strict Mode trong phiên làm việc, bạn sẽ KHÔNG THỂ: dừng sớm, sửa Whitelist, hay kill app bằng Task Manager. Phiên chỉ kết thúc khi hết giờ hoặc hoàn thành task. Bạn có chắc chắn?"*
  3. Người dùng xác nhận "Đồng ý". Hệ thống lưu trạng thái `strict_mode = true` vào Settings.
  4. UI hiển thị badge/icon cảnh báo nhỏ để nhắc nhở Strict Mode đang bật.

#### Luồng tắt Strict Mode (ON → OFF):
  1. Người dùng toggle "Strict Mode" sang OFF.
  2. Hệ thống hiển thị hộp thoại xác nhận 2 bước để tránh tắt nhầm:
     - **Bước 1:** *"Bạn có chắc muốn tắt Strict Mode? Điều này sẽ cho phép bỏ cuộc dễ dàng hơn trong phiên tiếp theo."* → Bấm "Tiếp tục".
     - **Bước 2:** Yêu cầu nhập lại câu xác nhận: *"Tôi chấp nhận giảm mức độ kỷ luật"* → Bấm "Xác nhận tắt".
  3. Hệ thống lưu `strict_mode = false`. Badge cảnh báo biến mất.

#### Luồng ngoại lệ:
  - Nếu người dùng đang trong phiên Focus, toggle Strict Mode bị **disable hoàn toàn** trên UI — không thể thay đổi cho đến khi phiên kết thúc.

### UC05: Xem báo cáo & thống kê
- **Tác nhân:** Người dùng
- **Mô tả:** Xem lại lịch sử kỷ luật của bản thân để đánh giá hành trình, nhìn thấy tiến bộ và duy trì động lực.
- **Tiền điều kiện:** Ứng dụng đang mở, đã có ít nhất 1 phiên làm việc được lưu trong Local DB.
- **Luồng sự kiện chính:**
  1. Người dùng điều hướng tới tab "Thống kê" (Analytics/Dashboard).
  2. Hệ thống mặc định hiển thị dữ liệu của **7 ngày gần nhất**.
  3. Người dùng có thể chọn bộ lọc thời gian: **Hôm nay / 7 ngày / 30 ngày / Tất cả**.
  4. Hệ thống query Local DB theo bộ lọc đã chọn và render các chỉ số:
     - 📊 **Biểu đồ cột** — Tổng thời gian tập trung thực tế theo từng ngày.
     - 🔥 **Streak Counter** — Chuỗi ngày liên tục có ít nhất 1 phiên hoàn thành (không bỏ cuộc). Hiển thị nổi bật ở đầu trang.
     - 🚫 **Số lần bị chặn** — Tổng số lần cố truy cập app/web nằm ngoài Whitelist trong khoảng thời gian đã chọn.
     - ✅ **Tỷ lệ hoàn thành** — Số phiên hoàn thành / tổng số phiên đã bắt đầu (%).
- **Logic tính Streak:**
  - Mỗi ngày dương lịch có ít nhất 1 phiên Focus **hoàn thành** (không bị hủy giữa chừng) → cộng 1 ngày vào Streak.
  - Nếu không có phiên hoàn thành nào trong ngày hôm qua → Streak **reset về 0**.
  - Streak được cập nhật tự động sau mỗi phiên kết thúc (tại UC03 bước 5).
- **Luồng ngoại lệ — Empty State (Lần đầu sử dụng, chưa có dữ liệu):**
  - Nếu Local DB chưa có phiên nào được ghi lại, trang Analytics không hiển thị biểu đồ trống rỗng hay lỗi.
  - Hệ thống hiển thị màn hình **Empty State** với:
    - Icon minh họa (VD: đồng hồ cát hoặc rocket chưa phóng).
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
  1. Người dùng vào Settings -> **"Motivation Quotes"**.
  2. Hệ thống hiển thị danh sách quote mặc định đang có (bundle sẵn trong app).
  3. Người dùng có thể **thêm quote cá nhân**, **sửa**, hoặc **xóa** bất kỳ quote nào.
  4. Người dùng có thể assign quote vào từng **sự kiện cụ thể** (trước phiên / giữa phiên / khi hoàn thành / khi bỏ cuộc) hoặc để chế độ **ngẫu nhiên (random)**.
  5. Hệ thống lưu danh sách vào Local DB.
- **Hậu điều kiện:** Kho quote được duy trì cục bộ, sẵn sàng được render ở lần tiếp theo.
