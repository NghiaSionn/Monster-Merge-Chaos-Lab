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
