# V0.2 Implementation Report

Cập nhật theo từng task. Yêu cầu gốc: `production/monster-merge-tasks-v0.2.md`. Quyết định phạm vi: `production/v0.2-scope-notes.md`.

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
