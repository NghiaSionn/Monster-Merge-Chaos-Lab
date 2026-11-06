# Review Prototype v0.1: task đã làm và kết quả test

| Mục | Giá trị |
|---|---|
| Cập nhật | 07/11/2026 |
| Unity | 6000.6.8f1, URP Renderer2D, New Input System |
| Phạm vi | P0 và P1 trong `production/monster-merge-tasks-v0.1.md`; P2 chưa làm |
| Thiết kế | `design/game-brief.md`, `design/gdd/monster-merge-gdd-v0.1.md` |
| Hướng dẫn chạy | `Assets/MonsterMerge/README_SETUP.md` |
| Playtest | `production/qa/playtests/2026-11-07-dev-editor-playtest.md` |

## Tóm tắt

Core loop chơi được trong Unity Editor. Lần chạy test cuối ngày 06/11/2026 đạt 29 pass, 0 fail: 28 test và 1 test chụp màn hình. Ngày 07/11/2026 người phát triển chơi một ván bằng chuột thật đến Game Over. Chưa build APK, chưa thử trên máy thật, chưa có người mới chơi thử.

Sau lần chạy test đó không có thay đổi nào về code game.

## Trạng thái task

Ký hiệu theo task list: `[x]` đã làm và có bằng chứng, `[ ]` chưa đủ bằng chứng.

| Mục | Trạng thái | Bằng chứng hoặc phần còn thiếu |
|---|---|---|
| P0.1 Nền, bể, camera | [x] | Ảnh trong playtest; đã xem ảnh 9:16 và 9:20 ngày 06/11 nhưng file không được giữ lại |
| P0.2 Dữ liệu 5 cấp, prefab | [x] | `Assets/MonsterMerge/Data/`, `Prefabs/Monster.prefab` |
| P0.3 Thả quái | [ ] | Chuột thật đã chạy; còn cảm ứng trên thiết bị |
| P0.4 Hợp nhất an toàn | [x] | T03, T04, T04b, T04c, T05, T06, Soak |
| P0.5 Điểm, Game Over, Restart | [ ] | Game Over thật đã thấy; còn bấm Restart tay nhiều lần và đóng/mở game |
| Checkpoint P0 | [ ] | Còn xác nhận Restart bằng tay |
| P1.1 Vật lý theo cấp | [ ] | Đã đo được khác biệt; còn đánh giá cảm nhận của người chơi |
| P1.2 Năng lượng | [x] | T11, T14; chạy thật trong playtest |
| P1.3 Magnetic Pull | [x] | T12, T12b, T13, T13b, T15, T16; chạy thật trong playtest |
| P1.4 Phản hồi chơi thử | [ ] | Đã có pop, flash, +điểm, combo, chỉ dẫn; chưa thử với người mới |
| Checkpoint P1 | [ ] | Mới có ván 2 phút, chưa đủ 10–15 phút |
| P2 Android | [ ] | Đã cài Android Build Support và chuyển target; chưa build APK |

## Kết quả test

Loại kiểm chứng:
- **Tự động:** PlayMode test chạy thật trong Unity.
- **Input giả lập:** đi qua Pointer và EventSystem thật bằng `InputTestFixture`.
- **Chuột thật:** quan sát trong video playtest.

| ID | Tình huống | Kết quả | Loại |
|---|---|---|---|
| T01 | Thả sát thành trái/phải | Pass | Tự động |
| T02 | Nhả 1 lần chỉ rơi 1 quái | Pass | Input giả lập, chuột thật |
| T03 | 2 quái cùng cấp hợp nhất, điểm 1 lần | Pass | Tự động |
| T04 | 3 quái chạm gần đồng thời | Pass | Tự động |
| T05 | Khác cấp không hợp nhất | Pass | Tự động |
| T06 | 2 Crag không hợp nhất | Pass | Tự động |
| T07 | Boing nảy qua vạch dưới 2 giây | Pass | Tự động |
| T08 | Tràn thật, Game Over 1 lần | Pass | Tự động, chuột thật |
| T09 | Restart 3 lần | Pass | Tự động |
| T10 | Chạm UI không gây thả | Pass | Input giả lập |
| T11 | 4 merge đạt 100, không vượt | Pass | Tự động |
| T12 | Chỉ 1 Magni, không mất năng lượng | Pass | Tự động |
| T13 | 2 Magni, hút và hợp nhất | Pass | Tự động, input giả lập |
| T14 | Hủy chọn mục tiêu | Pass | Tự động |
| T15 | Restart lúc đang hút | Pass | Tự động |
| T16 | Thua lúc đang chọn mục tiêu | Pass | Tự động |
| T17 | Android, cảm ứng 1 ngón | **Chưa chạy** | |
| T18 | Đóng/mở app giữ Best Score | **Chưa chạy** | Kỷ lục 5340 có sẵn đầu video là dấu hiệu tốt nhưng không chứng minh được đã đóng/mở app |

Test bổ sung:

| Test | Kết quả |
|---|---|
| T02b | 20 lần thả đều trong bể, cooldown chặn spam |
| T02c | Kéo rồi nhả dùng đúng X cuối, clamp sát tường |
| T04b | Chain merge đến cấp 3, combo x2 |
| T04c | Cùng cấp nhưng chưa chạm thì không hợp nhất |
| T07b | Thả liên tục không gây tràn giả |
| T10b | Kéo từ bể sang UI rồi nhả là hủy thả |
| T12b | Magni ngoài tầm 2.5 không mất năng lượng |
| T19 | Không có UI bấm được che vùng chơi ở 9:16 |
| Nảy từ 1.5m | Blip 0.010, Boing 0.676, Crag 0.000 |
| Quãng lăn 0.8s, v=3 | Blip 1.70, Goo 1.07 |
| Soak | Bot ngẫu nhiên, 3 ván: 86–101 giây, 165–196 lần thả, điểm 4460–5490 |

Bot không dùng nam châm và không có chiến thuật. Ván của người chơi trong video dài khoảng 2 phút, điểm 4030.

## Lỗi đã tìm và sửa khi phát triển

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| Cả bộ test fail ở SetUp, tier null | Builder tạo asset trước `NewScene`, thao tác này dọn object nên scene lưu tham chiếu rỗng | Tạo scene trước; `Wire` báo lỗi khi gặp null |
| `runInBackground` bị đổi thành True | Runner tự bật cờ, Test Framework khôi phục đè lên | Xóa đoạn code thừa, trả setting về False |
| Test input không nhận chuột | Input System phụ thuộc focus của Editor | Dùng `InputTestFixture` |
| Chạm Magni bị bỏ qua | Game View 1465x334 làm nút UI đè lên bể | Builder đặt Game View 9:16; thêm test T19 |
| Test tắt mất chỉ dẫn lần đầu | Test không khôi phục `HintSeen` | Test tự lưu và trả lại giá trị |
| Chỉ dẫn chồng lên màn Game Over | Hint vẫn bật khi thua | Tắt hint khi Game Over |
| Goo lăn gần bằng Blip (1.66 so với 1.70) | Friction không làm chậm hình tròn đang lăn | Thêm `rollResistance`, Goo = 6 |

## Khác với GDD

- Thêm `MonsterData.rollResistance` (`Rigidbody2D.angularDamping`). Bảng GDD mục 3.1 không có trường này.
- Bỏ qua quái vừa thả hoặc vừa hợp nhất trong 1 giây khi tính tràn (`settleGrace`), để quái đang rơi không giữ timer chạy.
- Kéo sang UI rồi nhả được tính là hủy thả.

## Rủi ro còn mở

- Cân bằng: xem 4 hướng cần quyết định trong báo cáo playtest.
- Chưa thử trên thiết bị: touch, safe area, font tiếng Việt, hiệu năng, pause/resume.
- `project.yaml` chưa đặt engine, và `CLAUDE.md` vẫn đang ghi mẫu Godot; cần chạy `/setup-engine`.
- Test nằm ở `Assets/MonsterMerge/Tests/`, không theo `Assets/Tests/PlayMode/` của Game Studios. Unity vẫn compile được, nhưng skill có thể không tìm thấy.
