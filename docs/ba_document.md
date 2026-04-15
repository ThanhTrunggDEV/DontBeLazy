# Tài liệu phân tích nghiệp vụ (BA Document) - Dự án "Don't Be Lazy"

## 1. Giới thiệu tổng quan (Introduction)
- **Tên dự án (dự kiến):** Don't Be Lazy (FocusLock / WorkEnforcer)
- **Mục tiêu:** Xây dựng một ứng dụng giúp người dùng nâng cao độ tập trung bằng cách ép buộc họ hoàn thành danh sách công việc (To-do list). Trong quá trình làm việc, hệ thống hoạt động theo cơ chế **Whitelist-first** (Chỉ cho phép những thứ cần thiết), chặn toàn bộ các trang mạng, phần mềm không liên quan để loại bỏ mọi sự xao nhãng.
- **Nền tảng mục tiêu:** Desktop (Windows/macOS) kết hợp trình duyệt, hoặc là ứng dụng nền hệ thống.

## 2. Tuyên bố vấn đề (Problem Statement)
- Người dùng thường xuyên bị mất tập trung bởi các mạng xã hội (Facebook, YouTube, TikTok) hoặc các ứng dụng giải trí (Game, Discord) trong lúc cần làm việc.
- Các công cụ chặn (Blocker) hiện tại dễ dàng bị tắt đi khi người dùng thiếu ý chí.
- **Giải pháp:** Một ứng dụng quản lý tác vụ liên kết trực tiếp với hệ thống chặn cứng. Ứng dụng này cung cấp "Chế độ nghiêm ngặt" (Strict Mode) khiến người dùng không thể dễ dàng can thiệp, tắt app hay phá vòng chặn cho đến khi hết thời gian quy định hoặc đã tick hoàn thành công việc.

## 3. Đối tượng người dùng mục tiêu & Phạm vi sử dụng (Target Audience & Scope)
- **Phạm vi hệ thống:** Ứng dụng cá nhân (Single-user application), hoạt động độc lập (Offline-first). Không yêu cầu tạo tài khoản (No Auth) hay hệ thống multi-user phức tạp. Bạn cài đặt và quản lý app của riêng mình trên máy cá nhân.
- **Đối tượng mục tiêu:**
  - Học sinh, sinh viên cần ôn thi.
  - Nhân viên văn phòng, freelancer làm việc từ xa (Work from home).
  - Những người mắc hội chứng ADHD hoặc gặp khó khăn trong việc duy trì sự tập trung dài hạn.

## 4. Danh sách tính năng (Feature List)

### 4.1. Tính năng cốt lõi (Core Features)
1. **Quản lý công việc (Task Management):**
   - Thêm, sửa, xóa các công việc cần làm trong ngày.
   - Ước lượng thời gian hoàn thành (Timer / Pomodoro).
   - **Gắn Whitelist theo Task:** Mỗi công việc sẽ có một danh sách Whitelist riêng biệt (Ví dụ: Task "Học Tiếng Anh" được mở từ điển, nhưng Task "Code" được mở VS Code). Người dùng thiết lập hoặc chọn Whitelist ngay khi tạo Task.
   - Đánh dấu hoàn thành.
2. **Quản lý danh sách ưu tiên (Whitelist Profiles Management):**
   - Thay vì 1 Whitelist dùng chung cho toàn app, người dùng có thể tạo ra các hồ sơ **Profile Whitelist** khác nhau (VD: Profile "Code", Profile "Design"). Khi tạo một task mới, chỉ cần gán một Profile thích hợp.
   - Mỗi bộ Profile bao gồm **Website Whitelist** (các URL cấp phép) và **App Whitelist** (các app được mở). Mặc định hệ thống chặn mọi app/web không thuộc Profile đang chạy.
   - **Blacklist (Tùy chọn phụ):** Chế độ cổ điển (Blocklist) chặn từng app cho user muốn xài cơ chế cũ.
3. **Chế độ tập trung (Focus Mode):**
   - **Bắt đầu phiên làm việc:** Kích hoạt hệ thống Whitelist ngay lập tức.
   - **Strict Mode (Chế độ kỷ luật thép):** Khi đã bật, vô hiệu hóa nút Stop/Pause, ngăn chặn việc thêm app/web mới vào Whitelist giữa chừng. Ứng dụng có cơ chế tự bảo vệ khó bị kill (tắt) bằng Task Manager. Chỉ kết thúc khi hết thời gian đếm ngược hoặc hoàn thành xong danh sách task đã chọn.
4. **Báo cáo & Thống kê (Analytics):**
   - Theo dõi thời gian tập trung thực tế mỗi ngày/tuần.
   - Thống kê lịch sử số lần cố gắng truy cập vào các trang web hoặc ứng dụng ngoài Whitelist.

### 4.2. Tính năng mở rộng (Nice-to-have Features)
- **Gamification (Thưởng/Phạt):** Tích luỹ kinh nghiệm (EXP) để lên cấp khi hoàn thành task. Hoặc áp dụng hình phạt nặng (phạt mất tiền cam kết - money penalty) nếu cố tình tìm mọi cách phá luật.
- **Tích hợp bên thứ ba:** Đồng bộ task từ Google Calendar, Notion, Todoist, Trello.

### 4.3. Ứng dụng Tâm lý học hành vi (Psychological Tricks)
1. **Tạo ma sát (Friction) để cản bước:** Khi Strict Mode đang tắt, nếu người dùng muốn bấm nút dừng phiên làm việc sớm (Stop), hệ thống không dừng ngay mà bắt người dùng tự tay gõ một câu dài đầy xấu hổ: *"Tôi là kẻ lười biếng, thiếu kỷ luật và tôi chấp nhận bỏ cuộc"*. Việc này tạo ra một rào cản lớn khiến họ lười thao tác nhượng bộ.
2. **Kích hoạt Cảm giác tội lỗi (Guilt-tripping):** Khi có ý định tắt app sớm, hiển thị màn hình cảnh báo nhấn mạnh vào sự bỏ cuộc: *"Bạn chỉ còn 10 phút nữa là xong! Bạn thực sự muốn vứt bỏ công sức từ nãy đến giờ sao?"* kèm theo các icon thất vọng hoặc câu quote đánh vào sự tự ái.
3. **Nỗi sợ mất mát (Loss Aversion):** Hiển thị chuỗi ngày kỷ luật (Streak). Nếu user bỏ cuộc, Streak (VD: đang 14 ngày làm việc liên tục) sẽ cháy thành tro, kích thích tâm lý tiếc nuối và buộc họ vượt qua cơn lười.
4. **Lời hứa chủ động (Implementation Intention):** Trước khi bấm Start chạy bộ đếm thời gian, buộc user phải tự tay gõ lại cam kết: *"Trong [X] phút tới, tôi sẽ hoàn thành [Tên Task]"*. Việc tự tay viết ra lời hứa sẽ khiến não bộ có xu hướng tuân thủ mục tiêu cao hơn 40%.

## 5. Yêu cầu phi chức năng (Non-Functional Requirements)
- **Lưu trữ & Bảo mật dữ liệu (Privacy-first):** Toàn bộ dữ liệu (danh sách công việc, cấu hình chặn, lịch sử tập trung) được lưu hoàn toàn ở máy cá nhân (Local DB như SQLite hoặc file JSON) cho 1 người dùng duy nhất. Không lưu trữ trên Cloud.
- **Hiệu năng:** Ứng dụng chạy ngầm cực nhẹ, tiêu tốn ít RAM/CPU để không ảnh hưởng đến các công cụ làm việc khác.
- **Bảo mật & Tính can thiệp system (Tamper-proofing):**
  - Chỉnh sửa file `hosts` hoặc dùng local VPN proxy để chặn web ở level hệ điều hành (chặn trên mọi trình duyệt).
  - Tự động detect và terminate process của các app bị block ngay khi chúng vừa khởi chạy.
  - Ngăn ngừa user gỡ cài đặt (Uninstall) trong lúc Focus Mode đang chạy.
- **UX/UI:** Trực quan, dễ sử dụng, thiết kế sang trọng, tối giản, hỗ trợ Dark Mode.

## 6. User Stories (Câu chuyện người dùng)
1. **Là một người dùng,** tôi muốn tạo một phiên làm việc Pomodoro 60 phút, **để** tôi có thể tập trung hoàn toàn vào việc viết code.
2. **Là một người dùng,** tôi muốn chỉ cấp quyền (Whitelist) cho `github.com` và `stackoverflow.com`, **để** mọi trang web giải trí hoặc ngoài lề tự động bị chặn, giúp tôi không bị xao nhãng.
3. **Là một người làm việc thiếu kỷ luật,** tôi muốn bật Strict Mode, **để** dù có nản chí và muốn từ bỏ giữa chừng, tôi cũng không thể tắt trình chặn hoặc tự thêm game vào Whitelist để chơi được.
4. **Là một người dùng,** tôi muốn xem biểu đồ thống kê thời gian tập trung trong tuần, **để** có động lực cải thiện bản thân.

## 7. Đề xuất Công nghệ (Technical Recommendation)
- **Frontend / Giao diện người dùng:** 
  - **Tauri** (Rust + React/Vue/Svelte) kết hợp với TailwindCSS. Lý do: Ứng dụng build ra rất nhẹ, native và tối ưu hơn Electron rất nhiều.
  - (Hoặc) **Electron** nếu cấu hình team quen thuộc với NodeJS.
- **Backend / Core Engine:** 
  - **Rust** hoặc **C++ / C#** để can thiệp sâu vào system API của hệ điều hành (quản lý Windows Registry, quản lý process, sửa file hosts cần quyền admin, hook vào service của OS để chống kill app).
