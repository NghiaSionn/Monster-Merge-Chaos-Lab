# Monster Merge: Chaos Lab — Prototype v0.1

Tài liệu thiết kế: `MONSTER_MERGE_GDD_v0.1.md`. Danh sách việc: `MONSTER_MERGE_CLAUDE_CODE_TASKS_v0.1.md`.

## Chạy thử trong Editor

1. Mở `Assets/MonsterMerge/Scenes/GameScene.unity`.
2. Chạy menu **Tools > Monster Merge > Set Game View 9-16**. Game View dẹt làm nút UI đè lên bể.
3. Bấm Play.

| Thao tác | Chuột (Editor) | Cảm ứng (Android) |
|---|---|---|
| Chọn vị trí thả | Giữ chuột, kéo ngang | Giữ ngón tay, kéo ngang |
| Thả quái | Nhả chuột | Nhấc ngón tay |
| Hủy lượt thả | Kéo lên nút UI rồi nhả | Như chuột |
| Dùng nam châm | Đầy 100% → bấm **Kích hoạt Nam châm** → bấm vào một Magni (số 4) | Như chuột |
| Hủy chọn nam châm | Bấm lại nút (đang hiện **Hủy**) | Như chuột |

## Menu công cụ

| Menu | Tác dụng |
|---|---|
| Tools > Monster Merge > Build Prototype Scene | Dựng lại `GameScene` và `Monster.prefab`. Thay đổi tay trong hai file này sẽ mất. `MonsterData` và sprite đã có được giữ nguyên |
| Tools > Monster Merge > Set Game View 9-16 | Đặt Game View 1080x1920 |
| Tools > Monster Merge > Run Play Mode Tests | Chạy test, ghi kết quả vào `Temp/MonsterMergeTestResults.txt` |

## Chỉnh cân bằng

- Từng cấp: `Assets/MonsterMerge/Data/MonsterTier1..5.asset`.
- Trên object `Managers`:
  - `DropController`: tỷ lệ spawn, cooldown, random seed.
  - `AbilityManager`: năng lượng, tầm/lực/thời gian hút.
  - `DangerZone`: thời gian tràn, thời gian ân hạn.
  - `MergeManager`: cửa sổ combo, tốc độ tối đa.
- Hình học bể: `TankBounds` trên object `Tank`. Tường và vạch được dựng từ số liệu này, nên sau khi đổi cần chạy lại Build Prototype Scene.

**Khác với GDD:** thêm `rollResistance` (`Rigidbody2D.angularDamping`). Đo thực tế cho thấy với friction 1.0 theo GDD, Goo lăn gần bằng Blip (1.66 so với 1.70 unit), vì friction không làm chậm hình tròn đang lăn. Với `rollResistance = 6`, Goo lăn 1.07 unit. Các cấp khác giữ 0.05 (mặc định của Unity).

## Khảo sát project

| Hạng mục | Kết quả |
|---|---|
| Unity | 6000.6.8f1 |
| Render pipeline | URP, Renderer2D. Sprite dùng `Sprite-Unlit-Default` vì scene không có đèn 2D |
| Input | Chỉ New Input System (`activeInputHandler: 1`), giữ nguyên |
| Enter Play Mode | Tắt Domain Reload: biến static không tự reset. Code không dùng static event |
| Assembly | `MonsterMerge.Runtime`, `MonsterMerge.Editor`, `MonsterMerge.Tests` |
| Package | Không cài thêm package nào |
| Git | Project chưa phải git repository và chưa có `.gitignore` |
| Android | Chưa cài. Xem mục Android |

**Thay đổi ngoài `Assets/MonsterMerge/`:**
- Player Settings: orientation từ AutoRotation sang Portrait.
- Build Settings: thêm `GameScene` lên đầu. `SampleScene` vẫn ở vị trí thứ hai.

## Kết quả test

Lần chạy cuối ngày 06/11/2026, Unity 6000.6.8f1, Game View 1080x1920: **29 pass, 0 fail** (28 test và 1 test chụp màn hình). Console không có lỗi nào từ game.

Ký hiệu:
- **Tự động**: PlayMode test đã chạy thật trong Unity.
- **Input giả lập**: như trên, đi qua Pointer và EventSystem thật bằng `InputTestFixture`, nhưng không dùng chuột hoặc ngón tay vật lý.
- **Chưa chạy**: cần người hoặc thiết bị.

| ID | Tình huống | Trạng thái | Test |
|---|---|---|---|
| T01 | Thả sát thành trái/phải | Tự động: pass | `T01_` |
| T02 | Nhả 1 lần chỉ rơi 1 quái | Input giả lập: pass | `T02_`, `T02c_` |
| T03 | 2 quái cùng cấp hợp nhất, điểm 1 lần | Tự động: pass | `T03_` |
| T04 | 3 quái chạm gần đồng thời | Tự động: pass | `T04_`, `T04b_`, `T04c_` |
| T05 | Khác cấp không hợp nhất | Tự động: pass | `T05_` |
| T06 | 2 Crag không hợp nhất | Tự động: pass | `T06_` |
| T07 | Boing nảy qua vạch dưới 2 giây | Tự động: pass | `T07_`, `T07b_` |
| T08 | Tràn thật, Game Over 1 lần | Tự động: pass (quái bị giữ cố định trên vạch) | `T08_` |
| T09 | Restart 3 lần | Tự động: pass | `T09_` |
| T10 | Chạm UI không gây thả | Input giả lập: pass | `T10_`, `T10b_` |
| T11 | 4 merge đạt 100, không vượt | Tự động: pass | `T11_` |
| T12 | Chỉ 1 Magni, không mất năng lượng | Tự động: pass | `T12_`, `T12b_` |
| T13 | 2 Magni, hút và hợp nhất | Tự động: pass. Chạm chọn: input giả lập | `T13_`, `T13b_` |
| T14 | Hủy chọn mục tiêu | Tự động: pass | `T14_` |
| T15 | Restart lúc đang hút | Tự động: pass | `T15_` |
| T16 | Thua lúc đang chọn mục tiêu | Tự động: pass | `T16_` |
| T17 | Android, cảm ứng 1 ngón | **Chưa chạy**: chưa cài Android | |
| T18 | Đóng/mở app giữ Best Score | **Chưa chạy**: T09 chỉ chứng minh qua Restart | |

Test bổ sung ngoài bảng:

| Test | Kết quả |
|---|---|
| `T19_` | Không có UI bấm được che vùng chơi ở 9:16 |
| `T02b_` | 20 lần thả đều nằm trong bể, cooldown chặn spam |
| `P1_1_SameDrop_` | Nảy lại từ 1.5m: Blip 0.010, Boing 0.676, Crag 0.000 |
| `P1_1_SamePush_` | Quãng lăn 0.8s: Blip 1.70, Goo 1.07 |
| `Soak_` | Bot thả ngẫu nhiên, 3 ván: 86–101 giây game-time, 165–196 lần thả, điểm 4460–5490, đều đạt cấp 5 |

Bot không dùng nam châm và không có chiến thuật, nên số liệu trên chỉ chứng minh độ ổn định, không phản ánh độ dài ván của người chơi thật.

## Trạng thái checklist

| Mục | Trạng thái | Ghi chú |
|---|---|---|
| P0.1 Nền, bể, camera | [x] | Xác minh bằng ảnh chụp 9:16 và 9:20 |
| P0.2 Dữ liệu 5 cấp, prefab | [x] | |
| P0.3 Thả quái | [ ] | Đã pass với chuột giả lập. Còn thử chuột thật và cảm ứng |
| P0.4 Hợp nhất an toàn | [x] | |
| P0.5 Điểm, Game Over, Restart | [ ] | Còn xác nhận Best Score sau khi đóng/mở game |
| Checkpoint P0 | [ ] | Cần người chơi thử một ván thật |
| P1.1 Vật lý theo cấp | [ ] | Đã đo được khác biệt. Cảm nhận "dễ thấy, không khó chịu" cần người đánh giá |
| P1.2 Năng lượng | [x] | |
| P1.3 Magnetic Pull | [x] | |
| P1.4 Phản hồi chơi thử | [ ] | Đã có pop, flash, +điểm, combo, chỉ dẫn. Chưa thử với người mới |
| Checkpoint P1 | [ ] | Chưa có phiên chơi tay 10–15 phút |
| P2 Android | [!] | Bị chặn: chưa cài Android Build Support |

## Cần người kiểm tra trong Editor

1. Chơi hết một ván bằng chuột thật, gồm kéo, thả, thua và bấm **Chơi lại**.
2. Thử cảm giác Boing, Goo và Crag. Goo có quá dính hoặc chưa đủ dính không?
3. Dùng nam châm trong tình huống thật. Năng lượng có đầy đúng lúc, kỹ năng có đáng dùng không?
4. Thoát Play, vào lại, xác nhận Kỷ lục vẫn còn.
5. Chơi liên tục 10–15 phút và theo dõi Console.

## Android

Editor hiện chỉ có module Windows và WebGL. Máy chưa có Android SDK, JDK và `adb`. **Chưa build APK nào.**

Cách cài:
1. Mở Unity Hub > Installs > 6000.6.8f1 > Add modules.
2. Chọn **Android Build Support**, kèm **OpenJDK** và **Android SDK & NDK Tools**.
3. Trong Unity: File > Build Profiles > Android > Switch Platform.
4. Bật Development Build, chọn Build, cài APK lên máy và chạy T17, T18.
5. Kiểm tra thêm mục P2: một ngón, không chạm nhầm UI, frame rate, pause/resume.
6. Ghi tên máy, phiên bản Android và kết quả thật vào đây.

## Known issues

- **Chưa thử trên thiết bị:** safe area, touch, đa điểm chạm và font tiếng Việt trên Android. UI dùng font `LegacyRuntime`, dựa vào font hệ thống của máy.
- **Giới hạn 5 cấp:** cặp Crag không hợp nhất nên bể sẽ dần đầy Crag. GDD đã ghi nhận rủi ro này, cần playtest để quyết định.
- **Bounce:** Unity lấy giá trị lớn hơn của hai vật khi va chạm, nên quái nảy thấp chạm Boing vẫn bật theo 0.65.
- **Test chụp màn hình** có gắn `[Explicit]` nhưng vẫn chạy trong menu Run Play Mode Tests. Nó tạm đổi Game View rồi trả về 1080x1920, và ghi ảnh vào `Temp/`.
- **Scene:** bấm Build Prototype Scene sẽ ghi đè mọi chỉnh sửa tay trong `GameScene`.
