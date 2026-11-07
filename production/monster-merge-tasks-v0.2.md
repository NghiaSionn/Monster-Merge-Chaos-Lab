# MONSTER MERGE: CHAOS LAB — CLAUDE CODE TASKS v0.2

**Sprint:** v0.2 “Fun & Balance”  
**Mục tiêu:** Dựa trên gameplay v0.1 đang chạy để cải thiện độ vui, độ rõ và quyết định chiến thuật. **Không viết lại project từ đầu.**  
**Nguồn:** `README_SETUP.md`, GDD v0.1, task v0.1, và `MONSTER_MERGE_V0.2_VIDEO_REVIEW.md`.

> Dành cho Claude Code làm việc **trực tiếp trong repo Unity**. Tài liệu này là yêu cầu triển khai, **không phải báo cáo đã thực hiện**. Phải audit code trước khi sửa. Những gì liên quan hình ảnh trong video chỉ là hiện tượng quan sát, không mặc định là lỗi code.

## A. Nguyên tắc bắt buộc

- [ ] Đọc GDD v0.1, README_SETUP.md, video review, cấu trúc scene và code hiện tại.
- [ ] Unity 6000.6.8f1, URP Renderer2D, New Input System, asmdef `MonsterMerge.Runtime`, `.Editor`, `.Tests` như README. **Không cài package mới** khi chưa có lý do.
- [ ] Ghi trạng thái test v0.1 làm baseline: báo cáo README là 29 pass, 0 fail; **xác minh bằng cách chạy lại**, không coi kết quả cũ là test v0.2.
- [ ] Lưu an toàn trước khi sửa: kiểm tra Git; nếu chưa có repo, tạo `.gitignore` chuẩn Unity, khởi tạo Git chỉ khi không xung đột với quản lý phiên bản của người dùng; không tự push remote. Tạo bản sao/commit trước những thay đổi có thể ghi đè.
- [ ] `Tools > Monster Merge > Build Prototype Scene` có thể **ghi đè GameScene và Monster.prefab**. Kiểm tra script builder và bảo đảm mọi thay đổi UI/Prefab được lưu vào nguồn sinh scene hoặc được tách khỏi builder. Không chạy menu này khi chưa đảm bảo dữ liệu không mất.
- [ ] Không thay đổi GDD lõi đã chốt: **không chiến đấu, không enemy, không shop, không ads, không thêm tier 6** trong sprint này.
- [ ] Mọi trị số balance phải cấu hình Inspector/ScriptableObject, tránh hard-code tùy tiện; giữ API/scene đang chạy trừ khi có lý do kỹ thuật rõ.
- [ ] Phân biệt `IMPLEMENTED`, `AUTO-TEST PASS`, `MANUAL TEST PENDING`, `ANDROID BLOCKED`. Không khai báo hoàn thành vượt bằng chứng.

## B. Sprint 1 — P0: Cấp cuối và khả năng cứu bể

### TASK-01: Cấu hình quy tắc khi 2 quái tier 5 va chạm

- [ ] Audit `MergeManager`, `Monster`, `MonsterData` và xử lý hiện tại của tier tối đa.
- [ ] Tạo tùy chọn Inspector/ScriptableObject `MaxTierMergeMode`:
  - `ClassicCap`: 5 + 5 không hợp nhất (hành vi v0.1).
  - `OverloadBurst`: 5 + 5 cùng tier hợp nhất bằng cách **xóa cả hai**, sinh FX ngắn + cộng điểm; **không sinh monster tier 6**.
- [ ] Dùng đúng cơ chế khóa cặp merge hiện tại; chỉ một sự kiện xử lý, một lần thưởng điểm/năng lượng. Merge đồng thời và triple-contact không tạo duplicate.
- [ ] `OverloadBurst` có thể thêm lực đẩy *rất nhỏ* tùy cấu hình, mặc định bằng 0 để bảo đảm prototype ổn định. FX được cleanup.
- [ ] Cho điều chỉnh điểm thưởng trong Inspector; combo và năng lượng hoạt động nhất quán với merge bình thường, ghi rõ quyết định.
- [ ] Thêm debug/log hoặc bộ đếm nội bộ theo chế độ để có thể so sánh A/B.

**Acceptance:** Inspector đổi chế độ không cần sửa code; 5+5 ở ClassicCap giữ nguyên; 5+5 ở OverloadBurst chỉ xóa đúng hai con và chỉ thưởng một lần; không NullReference hoặc object leak khi restart.

### TASK-02: Làm rõ điều kiện và phản hồi kỹ năng Magni

- [ ] Audit quy tắc năng lượng/cặp Magni/bán kính và cơ chế tap chọn mục tiêu đang có.
- [ ] Khi năng lượng 100% nhưng chưa đủ mục tiêu hợp lệ: cho người chơi biết lý do `Cần 2 Magni` hoặc `Không có cặp trong tầm`, không trừ năng lượng.
- [ ] Khi có mục tiêu hợp lệ: làm nổi nút Magni và các Magni đủ điều kiện (hiệu ứng nhẹ, không rung liên tục gây phiền).
- [ ] Tap UI không tạo lượt thả; tap monster sai không mất năng lượng; nút hủy vẫn hoạt động; hiệu ứng chỉ tính khi kỹ năng được kích hoạt hợp lệ.
- [ ] Có hướng dẫn 1 lần khi lần đầu kỹ năng thực sự dùng được. Dùng PlayerPrefs/local state hoặc hệ sẵn có để không hiện mãi, và bảo đảm test reset được.
- [ ] Bổ sung tùy chọn log trong Editor: thời điểm thanh đầy, lần đầu có cặp hợp lệ, số lần activate, số lần thành công, số lần attempt thất bại. Không bắt buộc analytics/cloud.

**Acceptance:** T01/T10/T11/T12/T13/T14/T15/T16 cũ không hồi quy; có ít nhất một kịch bản test dựng 2 Magni + đầy năng lượng dùng thành công; không có điều kiện UI bị đánh dấu dùng được mà không có mục tiêu.

## C. Sprint 2 — P1: Feel & readability

### TASK-03: Nhãn tier luôn dễ đọc

- [ ] Kiểm tra hierarchy SpriteRenderer/TextMeshPro/Text hiện tại.
- [ ] Giữ monster xoay theo Rigidbody2D, nhưng hiển thị chữ/số tier với hướng đứng yên hoặc tự counter-rotate mỗi frame. Không làm phát sinh lực vật lý.
- [ ] Không buộc dùng UI overlay toàn màn nếu child label giải quyết được. Bảo đảm nhãn không bị giật sau merge/spawn.

**Acceptance:** Quay monster 90°/180° vẫn đọc được chữ tier; test restart/game over không xuất hiện nhãn rác; font giữ được khi chạy Android (manual pending nếu chưa build).

### TASK-04: Juice hợp nhất tối thiểu

- [ ] Pop/tween scale ngắn (~0,12–0,20 giây, tùy chỉnh) khi tạo monster mới; dùng Coroutine/tween tự viết, không thêm package.
- [ ] Một hiệu ứng vòng tròn/spark đơn giản tại điểm hợp nhất, tự biến mất; không che monster hoặc cản input.
- [ ] Giữ popup điểm hiện có; tránh trùng popup và giới hạn số hiệu ứng tồn tại cùng lúc.
- [ ] SFX merge bằng AudioSource/clip hợp lệ nếu project có audio; nếu chưa có asset phù hợp, sử dụng clip tạm do đội phát triển sở hữu/có quyền, **không tải nhạc/clip bản quyền tùy tiện**. Có toggle volume/mute nếu dễ tích hợp.
- [ ] Combo >=3 được nhấn rõ, không kéo dài giao diện che bể. Có thể bật/tắt `JuiceEnabled` để A/B nhanh.

**Acceptance:** 20+ merge liên tiếp không spawn FX vô hạn, không đúp âm, không còn coroutine truy cập monster đã destroy; gameplay vật lý không bị scale animation làm sai collider (ưu tiên animate visual child, không animate physics root).

### TASK-05: Giao diện hướng dẫn ngắn

- [ ] Trên lần chơi đầu hiển thị 1–2 câu về kéo/thả, nhỏ và không khóa input lâu.
- [ ] Phân biệt preview hiện tại / `Tiếp theo`, tránh người chơi hiểu sai vị trí thả.
- [ ] Khi DangerZone thật sự sắp trigger game over, vạch cảnh báo có trạng thái rõ; không chớp đỏ chỉ vì 1 vật đang rơi nhanh qua vạch.
- [ ] Tôn trọng kích thước 9:16 và 9:20; touch target tối thiểu đủ dễ thao tác; không che vùng thả.

**Acceptance:** Input chuột và mô phỏng cảm ứng vẫn không thả nhầm khi bấm nút; UI 9:16 không chồng; 9:20 kiểm tra an toàn.

## D. Sprint 3 — P1/P2: Playtest và hồi quy

### TASK-06: Automated regression (bắt buộc)

- [ ] Chạy lại toàn bộ test v0.1, lưu kết quả thực tế vào file báo cáo mới `Assets/MonsterMerge/Docs/V02_TEST_REPORT.md` (hoặc Docs hiện có), đếm số pass/fail thực.
- [ ] Thêm PlayMode test cho `ClassicCap` và `OverloadBurst`; 3+ quái chạm đồng thời; reset sau burst; điểm & energy cộng đúng 1 lần.
- [ ] Test Magni thiếu cặp, không có cặp trong tầm, cặp hợp lệ, hủy chọn và game over trong khi chọn.
- [ ] Kiểm tra label counter-rotation và FX lifetime ít nhất ở mức component/integration phù hợp.
- [ ] Soak bot nhiều ván với **cả hai mode** để phát hiện lỗi; ghi thời lượng game-time, số drop, số merge, số tier 5 burst, điểm. Bot không đại diện người chơi thật.
- [ ] Không được khẳng định test Android pass nếu chưa có thiết bị và module.

### TASK-07: Manual playtest protocol (người dùng sẽ chạy)

- [ ] Chơi 3 ván `ClassicCap` và 3 ván `OverloadBurst`; ghi **mode, điểm, thời lượng, số lần Magni thành công, lý do Game Over, có cảm giác còn cơ hội cứu không**.
- [ ] Thử 1 ván cố tình dùng Magni ngay khi đủ điều kiện; xem feedback có rõ ràng không.
- [ ] Chơi thêm 1 ván với `JuiceEnabled=false` và 1 ván `true`; so cảm giác merge.
- [ ] Xác nhận Best Score sau thoát Play Mode/vào lại và nếu có Android thì đóng/mở app thật.
- [ ] Ghi nhận 2 vấn đề khó chịu nhất vào báo cáo, không tự động bổ sung tính năng mới để chữa triệu chứng.

### TASK-08: Android check (chỉ khi đủ module và thiết bị)

- [ ] Nếu Unity chưa có `Android Build Support`, `OpenJDK`, SDK/NDK, ghi `BLOCKED: missing modules`, kèm hướng dẫn cài.
- [ ] Nếu có, build Development APK, test touch 1 ngón, UI safe area, pause/resume, các popup, FPS. Ghi thiết bị/Android version.
- [ ] Chưa cần AdMob hay Play Console cho v0.2.

## E. Bàn giao bắt buộc của Claude Code

Tạo/cập nhật các file tại thư mục tài liệu của project:

1. `V02_IMPLEMENTATION_REPORT.md`: liệt kê file code đã sửa, lý do, thay đổi Inspector/Scene/Prefab, tính năng chưa hoàn thành, điều chỉnh từ GDD.
2. `V02_TEST_REPORT.md`: test chạy thật, kết quả, failed/skipped; test manual và Android nào chưa thực hiện.
3. `V02_PLAYTEST_FORM.md`: bảng trống cho 6 ván A/B + chú thích cách thay `MaxTierMergeMode` trong Inspector.
4. Cập nhật README_SETUP.md với đường dẫn scene, nơi sửa `MaxTierMergeMode`, nơi chỉnh juice/khả năng Magni, cảnh báo builder ghi đè nếu vẫn tồn tại.
5. Ghi `CHANGELOG` v0.2 ngắn; không xóa/chỉnh sửa lịch sử v0.1 một cách khó truy vết.

## F. Definition of Done

- [ ] Toàn bộ core v0.1 còn chạy, không hồi quy test cũ.
- [ ] Có lựa chọn A/B xử lý tier 5 và automated tests cho cả hai.
- [ ] Magni có hướng dẫn và trạng thái sử dụng rõ; không mất energy sai.
- [ ] Nhãn tier không xoay ngược/khó đọc khi thân quay.
- [ ] Merge có phản hồi nhìn/tiếng tối thiểu, không gây ảnh hưởng Rigidbody2D.
- [ ] Người dùng có bảng khảo sát A/B và biết cách test.
- [ ] Không tự gắn nhãn `Manual playtest passed`, `APK verified` hoặc `game vui` khi chưa có bằng chứng.

## G. Prompt có thể gửi nguyên văn cho Claude Code

> Đọc `MONSTER_MERGE_V0.2_VIDEO_REVIEW.md`, `MONSTER_MERGE_V0.2_CLAUDE_CODE_TASKS.md`, GDD v0.1 và README_SETUP.md. Kiểm tra các script, scene, prefab, editor builder hiện tại trước khi sửa. Triển khai v0.2 **trên prototype đã có**, không làm lại từ đầu. Ưu tiên TASK-01, TASK-02, rồi TASK-03–05. Giữ game là physics merge casual, không thêm chiến đấu, không thêm tier 6 hoặc ads. Chạy test thật, ghi chính xác pass/fail/skipped, tạo báo cáo triển khai và bảng playtest A/B. Đặc biệt tránh `Build Prototype Scene` ghi đè chỉnh sửa. Nếu thiếu Android modules thì đánh dấu BLOCKED, không tự nhận APK đã test. Cho tôi biết những bước test tay còn cần thực hiện.
