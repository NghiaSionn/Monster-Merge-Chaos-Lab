# V0.2 Implementation Report

Cập nhật theo từng task. Yêu cầu gốc: `production/monster-merge-tasks-v0.2.md`. Quyết định phạm vi: `production/v0.2-scope-notes.md`.

## Bàn giao sprint v0.2 (10/11/2026)

| Task | Trạng thái |
|---|---|
| TASK-01 Luật cấp cuối A/B | IMPLEMENTED, AUTO-TEST PASS |
| TASK-02 Phản hồi nam châm | IMPLEMENTED, AUTO-TEST PASS |
| TASK-03 Nhãn số đứng thẳng | IMPLEMENTED, AUTO-TEST PASS |
| TASK-04 Juice hợp nhất | IMPLEMENTED, AUTO-TEST PASS |
| TASK-05 Hướng dẫn, vạch thả, cảnh báo tràn | IMPLEMENTED, AUTO-TEST PASS |
| TASK-06 Kiểm thử hồi quy tự động | AUTO-TEST PASS: 54/54, soak cả hai chế độ |
| TASK-07 Chơi tay A/B | **DEFERRED** |
| TASK-08 Android | APK Development đã build; test trên máy **DEFERRED** |

**Quyết định ngày 10/11/2026: chỉ test thủ công khi game hoàn thiện.** Người phát triển bỏ qua các bước test thủ công trong sprint này. Vì vậy chưa có bằng chứng nào về cảm nhận khi chơi, âm thanh, hay việc chạy trên điện thoại. Không mục nào trong báo cáo này được hiểu là "đã chơi thử đạt" hay "APK đã kiểm chứng".

**Việc phải làm khi game hoàn thiện:**
1. Chơi 6 ván A/B theo `production/qa/V02_PLAYTEST_FORM.md`, thêm một ván tắt `Juice Enabled` và một ván dùng nam châm ngay khi sáng.
2. Kiểm tra Best Score sau khi thoát Play Mode, và sau khi đóng/mở app trên điện thoại.
3. Cài APK (`Tools > Monster Merge > Build Android Dev APK`) lên máy thật: chạm một ngón, safe area, pause/resume, FPS, popup, âm thanh. Xác nhận rủi ro đọc cú chạm ghi trong `V02_TEST_REPORT.md`.

**Điểm cần lưu ý khi chọn A/B:** soak cho thấy với `OverloadBurst`, bot ngẫu nhiên không thua ván nào trong 10 phút, trong khi `ClassicCap` thua sau khoảng 90 giây. Luật B có thể đã xóa điều kiện thua. Xem `V02_TEST_REPORT.md`.

**Vấn đề đã biết nhưng chưa xử lý:** năng lượng hồi quá nhanh (đầy ở giây 5–6) và combo quá dễ đạt; không task nào của v0.2 xử lý hai vấn đề này.

## TASK-01: Luật cấp cuối A/B — IMPLEMENTED, AUTO-TEST PASS

### File code

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Monsters/MergeManager.cs` | Thêm enum `MaxTierMergeMode`, các trường `maxTierMode`, `burstScore`, `burstPushImpulse`, `burstPushRadius`; bộ đếm `MergeCount`, `BurstCount`; `ExecuteBurst`; `MergeResult.IsBurst` | Burst dùng chung khóa cặp và sự kiện `Merged`, nên điểm, năng lượng và combo được cộng đúng một lần như merge thường |
| `Scripts/Core/GameManager.cs` | Ghi số liệu cuối ván ra Console, bật/tắt bằng `logRoundStats` | So sánh A/B |
| `Scripts/Gameplay/MergeEffects.cs` | File mới: vòng nổ khi burst, tự hủy sau 0,35 giây, tối đa 8 cái cùng lúc | Hiệu ứng ngắn theo TASK-01 |
| `Scripts/Editor/MonsterMergeSceneBuilder.cs` | Thêm và nối `MergeEffects` vào `Managers` | Thay đổi scene phải đi qua builder để không mất khi dựng lại |
| `Tests/MonsterMergePlayTests.cs` | Thêm 6 test `V02_T01*` | Xem `V02_TEST_REPORT.md` |
| `Tests/MonsterMergeScreenshotTests.cs` | Lưu ảnh vào `production/qa/evidence/prototype-v0.2/`; thêm ảnh burst | Ảnh trong `Temp/` từng bị mất |

### Inspector và scene

- `GameScene` > `Managers` > `MergeManager` > nhóm **Cấp cuối (A/B)**:

  | Trường | Mặc định | Ý nghĩa |
  |---|---|---|
  | `Max Tier Mode` | `ClassicCap` | Chế độ A/B |
  | `Burst Score` | 300 | Điểm thưởng mỗi cặp |
  | `Burst Push Impulse` | 0 | Lực đẩy khi burst, 0 là tắt |
  | `Burst Push Radius` | 1.5 | Bán kính lực đẩy |

- `Managers` > `MergeEffects`: màu, thời gian, kích thước, giới hạn số hiệu ứng.
- `Managers` > `GameManager` > `Log Round Stats`: bật mặc định.
- `GameScene` đã được dựng lại bằng builder. Builder sinh lại toàn bộ ID nên diff của file scene rất lớn; thay đổi thực chỉ là thêm `MergeEffects` và các trường mới.

### Quyết định

- Burst được tính như một lần hợp nhất: cộng `energyPerMerge` và tăng combo.
- Burst không sinh tier 6, đúng ràng buộc của sprint.
- Restart nạp lại scene nên chế độ quay về giá trị trong Inspector. Muốn thử B thì phải đổi trong Inspector, không đổi lúc đang chơi.

### Chưa xong (TASK-01)

- Màu hiệu ứng trùng với Crag; xử lý ở TASK-04.
- Chưa xác minh popup "+300" bằng mắt.
- Lực đẩy khi burst chưa có test.

## TASK-02: Điều kiện và phản hồi của Magni — IMPLEMENTED, AUTO-TEST PASS

### File code

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Gameplay/AbilityManager.cs` | Thêm enum `MagnetAvailability` (chưa đủ năng lượng, cần 2 Magni, không có cặp trong tầm, sẵn sàng); tính lại mỗi frame chỉ trên các Magni, dùng lại danh sách nên không cấp phát; nút chỉ vào chế độ chọn khi sẵn sàng; làm sáng Magni có cặp; hướng dẫn một lần; bộ đếm và log cuối ván | Đúng yêu cầu TASK-02 |
| `Scripts/Monsters/Monster.cs` | Thêm `SetHighlight`, màu tính từ mức sáng ổn định và flash | Làm sáng nhẹ, không nhấp nháy |
| `Scripts/UI/GameUI.cs` | Nút đổi chữ theo lý do và đổi màu theo trạng thái; hiện hướng dẫn 3,5 giây | Người chơi biết vì sao chưa dùng được |
| `Scripts/Editor/MonsterMergeSceneBuilder.cs` | Dời ô thông báo lên dải trống giữa HUD và miệng bể, cho phép 2 dòng | Ảnh chụp cho thấy vị trí cũ làm chữ đè lên bể |
| `Tests/MonsterMergePlayTests.cs`, `MonsterMergeInputTests.cs` | Thêm 5 test, điều chỉnh 5 test cũ; lưu và trả lại key hướng dẫn | Xem `V02_TEST_REPORT.md` |
| `Tests/MonsterMergeScreenshotTests.cs` | Thêm ảnh "Cần 2 Magni" và trạng thái sẵn sàng | Bằng chứng hình ảnh |

### Inspector

- `Managers` > `AbilityManager` > nhóm **Phản hồi**:

  | Trường | Mặc định | Ý nghĩa |
  |---|---|---|
  | `Magni Highlight` | 0.4 | Mức sáng của Magni, 0 là tắt |
  | `Log Stats` | bật | Ghi số liệu nam châm khi hết ván, chỉ trong Editor |

- `Canvas` > `GameUI`:

  | Trường | Mặc định | Ý nghĩa |
  |---|---|---|
  | `Tip Seconds` | 3.5 | Thời gian hiện hướng dẫn |
  | `Magnet Ready Color` | | Màu nút khi sẵn sàng |
  | `Magnet Waiting Color` | | Màu nút khi chưa có mục tiêu |

- PlayerPrefs key `MonsterMerge.MagnetTipSeen` ghi nhận đã hiện hướng dẫn. Xóa key này để xem lại.
- `GameScene` đã được dựng lại bằng builder.

### Hành vi

- Đầy năng lượng nhưng chưa có mục tiêu: nút vẫn bấm được để báo lý do, nhưng mờ đi và không vào chế độ chọn.
- Năng lượng chỉ bị trừ khi bắt đầu hút hợp lệ. Hai Magni bị kẹt không được hoàn năng lượng, theo quyết định ngày 07/11/2026 giữ như GDD.
- Đang chọn mà cặp Magni hợp nhất mất: chế độ chọn vẫn giữ, chạm vào sẽ báo lý do và không trừ năng lượng.
- Log cuối ván: `[MonsterMerge] Nam châm: đầy lần đầu=..., có cặp lần đầu=..., kích hoạt=..., thành công=..., thất bại=...`.

### Chưa xong (TASK-02)

- Chưa thử bằng chuột thật hay cảm ứng.
- Chữ trên nút và thông báo vẫn viết thẳng trong code, chưa có hệ thống bản địa hóa. Prototype chỉ có tiếng Việt.

## TASK-03: Nhãn số luôn đứng thẳng — IMPLEMENTED, AUTO-TEST PASS

### File code

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Monsters/Monster.cs` | Thêm `LateUpdate` đặt góc xoay của nhãn về 0 mỗi frame; thêm `LabelTransform` cho test | Thân vẫn xoay theo Rigidbody2D, chỉ nhãn được giữ đứng |
| `Tests/MonsterMergePlayTests.cs` | Thêm 3 test `V02_T03*` | Xem `V02_TEST_REPORT.md` |
| `Tests/MonsterMergeScreenshotTests.cs` | Thêm ảnh `mm_labels_rotated_9x16.jpg` | Bằng chứng hình ảnh |

### Quyết định

- Dùng nhãn con có sẵn, không tạo UI overlay, đúng gợi ý của task.
- Chạy trong `LateUpdate` vì Rigidbody2D (Interpolate) cập nhật transform trước `Update`; đặt góc ở đây là bước cuối trước khi vẽ.
- Nhãn không có collider, nên đổi góc nhãn không tạo lực. `T03b` xác nhận quái vẫn quay bình thường.
- Không đổi prefab hay scene, nên không cần chạy builder.

### Chưa xong (TASK-03)

- Font trên Android: MANUAL TEST PENDING, chưa build APK.
- Thân quái là hình tròn đều nên ảnh chụp không cho thấy thân đã xoay; bằng chứng thân xoay nằm ở `T03a`.

## TASK-04: Juice hợp nhất tối thiểu — IMPLEMENTED, AUTO-TEST PASS

### File code

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Gameplay/MergeEffects.cs` | Viết lại: công tắc `JuiceEnabled`; vòng sáng merge; tiếng bíp tự sinh với 4 nguồn phát và khoảng cách tối thiểu; vòng burst đổi sang vàng nhạt, vẽ sau quái | Phản hồi hình và tiếng, có giới hạn |
| `Scripts/Monsters/Monster.cs` | Thêm `PlayVisualPop` và `VisualPopScale`: nảy thân và số, không đụng transform gốc | Task yêu cầu animate visual child, không animate physics root |
| `Scripts/UI/GameUI.cs` | Giới hạn 6 popup cùng lúc; combo từ x3 đổi màu và phóng to khi bật juice | Không phủ kín bể khi merge dồn dập |
| `Scripts/Editor/MonsterMergeSceneBuilder.cs` | Sinh `Art/Ring.png` (vòng rỗng); nối `GameUI.effects` | Thay đổi scene đi qua builder |
| `Tests/MonsterMergePlayTests.cs` | Thêm 4 test `V02_T04*` | Xem `V02_TEST_REPORT.md` |
| `Tests/MonsterMergeScreenshotTests.cs` | Thêm ảnh `mm_merge_ring_9x16.jpg` | Bằng chứng hình ảnh |

### Inspector

`Managers` > `MergeEffects`:

| Nhóm | Trường chính | Mặc định |
|---|---|---|
| Juice | `Juice Enabled` | bật |
| Pop phần hình | `Visual Pop Duration`, `Visual Pop Overshoot` | 0,16 giây; 18% |
| Vòng sáng merge | thời gian, kích thước đầu/cuối, độ pha trắng, độ trong | 0,25 giây; 1,0 đến 1,8 lần đường kính |
| Burst | màu, thời gian, kích thước | vàng nhạt; 0,35 giây |
| Giới hạn | `Max Active`, `Sorting Order` | 8; 3 (sau quái) |
| Âm thanh | `Merge Clip`, `Sfx Volume`, `Sfx Muted`, `Sfx Min Interval`, `Sfx Voices`, cao độ | trống (dùng bíp tự sinh); 0,5; tắt; 0,05 giây; 4 |

`Canvas` > `GameUI`: `Max Popups` (6), nhóm Combo (từ x3, màu, độ phóng, thời gian).

### Quyết định

- **Tắt `JuiceEnabled` thì quay về cảm giác v0.1:** vẫn giữ flash, popup điểm và việc gốc quái lớn dần 70% lên 100%. Tắt pop phần hình, vòng sáng merge, âm thanh, nhấn combo.
- **Vòng nổ burst luôn bật** vì là phản hồi chức năng của TASK-01. Tiếng của burst thì theo công tắc juice.
- **Giữ việc gốc quái lớn dần** để collider không đẩy văng quái bên cạnh. Pop mới chỉ chạy trên phần hình.
- **Âm thanh:** project chưa có file âm thanh, nên dùng tiếng bíp sinh bằng code: sóng sin 660 Hz, 0,09 giây. Gắn clip vào `Merge Clip` để thay. Cấp càng cao cao độ càng thấp; burst thấp nhất.
- Chưa có nút bật/tắt âm trong game; chỉnh trong Inspector.

### Chưa xong (TASK-04)

- Chưa ai nghe thử âm thanh; chưa thử trên Android.
- Khi chạm giới hạn, merge mới sẽ không có vòng sáng hoặc popup. Đây là chủ ý, nhưng cần người chơi đánh giá có bị thiếu phản hồi không.

## TASK-05: Giao diện hướng dẫn ngắn — IMPLEMENTED, AUTO-TEST PASS

### File code

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Gameplay/DangerZone.cs` | Thêm `IsWarning`: chỉ bật khi bộ đếm tràn chạy quá `warnAfter` (0,25 giây). Khi cảnh báo, vạch dày 2,5 lần và nhấp nháy 4 lần/giây; hết nguy hiểm thì về như cũ | Cảnh báo rõ khi sắp thua, không chớp vì quái rơi hay nảy qua |
| `Scripts/Gameplay/DropController.cs` | Thêm vạch dọc `dropGuide` từ preview xuống đáy, ẩn cùng preview; thêm thuộc tính cho test | Phân biệt preview đang thả với "Tiếp theo", thấy rõ quái sẽ rơi ở đâu |
| `Scripts/Editor/MonsterMergeSceneBuilder.cs` | Tạo `DropGuide`; rút hướng dẫn còn 2 câu, ô 860x170 thay vì 860x300 | Hướng dẫn nhỏ, không khóa input |
| `Tests/MonsterMergePlayTests.cs`, `MonsterMergeInputTests.cs` | Thêm 6 test, gồm 2 test chạm cảm ứng giả lập | Xem `V02_TEST_REPORT.md` |
| `Tests/MonsterMergeScreenshotTests.cs` | Kiểm tra UI không che vùng chơi ở 9:20; thêm ảnh cảnh báo | Bằng chứng ở hai tỉ lệ |

### Inspector

- `Managers` > `DangerZone` > nhóm **Cảnh báo sắp tràn**: `Warn After` (0,25 giây), `Warn Thickness` (2,5), `Warn Pulse Hz` (4).
- `Tank` > `DropGuide`: màu và độ trong của vạch dọc. Vì đối tượng này do builder tạo, muốn đổi lâu dài thì sửa trong builder.

### Quyết định

- Hướng dẫn chỉ còn phần kéo/thả và hợp nhất; phần nam châm đã có hướng dẫn riêng từ TASK-02. Ô hướng dẫn không chặn chạm và tắt sau lần thả đầu, như v0.1.
- Cảnh báo chỉ nằm trên vạch, không thêm chữ, vì dải trống phía trên bể đã dùng cho thông báo.
- Nút nam châm cao 120 và nút chơi lại cao 140 theo độ phân giải tham chiếu 1080x1920, nên đủ lớn để chạm. Chưa đo trên máy thật.

### Chưa xong (TASK-05)

- Chạm thật trên Android chưa thử. Xem rủi ro `wasPressedThisFrame` trong `V02_TEST_REPORT.md`.
- Thứ tự vẽ giữa vạch dọc và ô hướng dẫn trong game thật chưa xác minh bằng mắt.

## TASK-06: Kiểm thử hồi quy tự động — AUTO-TEST PASS

| File | Thay đổi |
|---|---|
| `Tests/MonsterMergePlayTests.cs` | Tách soak thành hàm dùng chung `SoakRounds`; thêm `Soak_OverloadBurst_FullGames_NoErrors` (thời gian chờ 15 phút); log thêm số merge và số burst mỗi ván |

- Toàn bộ 29 test v0.1 chạy lại không hồi quy; tổng cộng 54 test pass.
- Báo cáo đặt ở `production/qa/V02_TEST_REPORT.md` thay vì `Assets/MonsterMerge/Docs/`, theo quyết định phạm vi số 6.
- Soak `OverloadBurst` mất khoảng 5 phút vì cả 3 ván đều chạm giới hạn 600 giây game-time.

## TASK-07: Chơi tay A/B — DEFERRED

Hoãn đến khi game hoàn thiện. Phiếu `production/qa/V02_PLAYTEST_FORM.md` đã có hướng dẫn đổi chế độ và bật/tắt juice. Log cuối ván trong Console cung cấp sẵn điểm, thời gian, số merge, số burst và số liệu nam châm.

## TASK-08: Android — APK BUILT, DEVICE TEST DEFERRED

| File | Thay đổi | Lý do |
|---|---|---|
| `Scripts/Editor/MonsterMergeAndroidBuild.cs` | Menu `Tools > Monster Merge > Build Android Dev APK`: build Development, chỉ `GameScene`, xuất ra `Builds/Android/`, ghi kết quả vào `Temp/MonsterMergeBuildResult.txt` | MCP không cho phép gọi build, nên người phát triển bấm menu |
| `.gitignore` | Thêm `/.utmp/` | Thư mục tạm do lần build Android tạo ra |

- Build thành công ngày 08/11/2026: 12 phút 9 giây, APK 40,4 MB, 0 lỗi, 1 cảnh báo vô hại của IL2CPP.
- Lần thử kết nối điện thoại ngày 08/11/2026 thất bại do lỗi USB. Test trên máy hoãn đến khi game hoàn thiện.
- Sau lần build, Unity tự cập nhật thiết lập lọc shader URP và `preloadedAssets`. `UnityConnectSettings` chuyển sang bật kết nối Unity Services; thay đổi này chưa được commit, chờ người phát triển xác nhận.
