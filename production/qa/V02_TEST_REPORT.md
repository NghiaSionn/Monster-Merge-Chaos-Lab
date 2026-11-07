# V0.2 Test Report

Nhãn: `AUTO-TEST PASS` (test tự động chạy thật), `MANUAL TEST PENDING` (cần người chơi), `NOT RUN`.

## Môi trường

| Mục | Giá trị |
|---|---|
| Unity | 6000.6.8f1, build target Android |
| Game View | 1080x1920 |
| Cách chạy | Tools > Monster Merge > Run Play Mode Tests, kết quả ghi vào `Temp/MonsterMergeTestResults.txt` |

## Lần chạy

| Ngày | Mốc code | Pass | Fail | Skip | Ghi chú |
|---|---|---:|---:|---:|---|
| 07/11/2026 | Baseline v0.1, commit `8d6199e` | 29 | 0 | 0 | 28 test và 1 test chụp màn hình |
| 07/11/2026 | Sau TASK-01, chưa commit | 35 | 0 | 0 | 29 cũ không hồi quy, thêm 6 test mới; Console không có lỗi |
| 07/11/2026 | Sau TASK-02, chưa commit | 40 | 0 | 0 | Thêm 5 test mới, điều chỉnh 5 test cũ (xem dưới); chạy lại sau khi dời ô thông báo, vẫn 40/40 |

## Test mới của TASK-01

| Test | Kịch bản | Kết quả |
|---|---|---|
| `V02_T01a_ClassicCap_IsDefault_CragsStay` | Mặc định là ClassicCap; 2 Crag không hợp nhất | AUTO-TEST PASS |
| `V02_T01b_OverloadBurst_TwoCrags_RemovedAndRewardedOnce` | 2 Crag biến mất, đúng 1 sự kiện, +300 điểm, +25 năng lượng, không sinh quái | AUTO-TEST PASS |
| `V02_T01c_OverloadBurst_ThreeCrags_OnlyOnePairBursts` | 3 Crag chạm nhau: 1 cặp biến mất, 1 con ở lại, thưởng 1 lần | AUTO-TEST PASS |
| `V02_T01d_BurstFx_AppearsThenCleansUp` | Hiệu ứng xuất hiện rồi tự hủy | AUTO-TEST PASS |
| `V02_T01e_BurstFx_CappedAtMaxActive` | Gọi 20 lần chỉ tạo tối đa 8 hiệu ứng | AUTO-TEST PASS |
| `V02_T01f_RestartRightAfterBurst_NoLeaks` | Restart ngay sau burst không còn quái, hiệu ứng hay bộ đếm cũ | AUTO-TEST PASS |

## Test mới của TASK-02

| Test | Kịch bản | Kết quả |
|---|---|---|
| `V02_T02a_ValidPair_HighlightsOnlyWhenEnergyFull` | Có cặp nhưng năng lượng 75%: không sáng, nút khóa. Đủ 100%: hai Magni sáng, nút "Kích hoạt Nam châm" | AUTO-TEST PASS |
| `V02_T02b_NoTarget_ButtonShowsReason` | 1 Magni: nút ghi "Cần 2 Magni"; thêm Magni trong tầm thì sẵn sàng | AUTO-TEST PASS |
| `V02_T02c_Tip_ShownOnlyOnceAcrossRestart` | Hướng dẫn hiện đúng 1 lần, kể cả sau khi vào/hủy chọn và Restart | AUTO-TEST PASS |
| `V02_T02d_Stats_CountActivationsSuccessesAndFailures` | Đếm đúng kích hoạt, chạm hụt, hút thành công | AUTO-TEST PASS |
| `V02_T02e_GameOver_ClearsHighlightAndButton` | Game Over tắt sáng và khóa nút | AUTO-TEST PASS |

Kịch bản 2 Magni và đầy năng lượng dùng thành công: `T13` (gọi hàm) và `T13b` (chạm chuột giả lập).

### Test cũ được điều chỉnh vì hành vi đổi

TASK-02 yêu cầu không để UI trông như dùng được khi chưa có mục tiêu, nên nút không còn vào chế độ chọn khi thiếu cặp Magni.

| Test | Trước | Sau |
|---|---|---|
| `T12` | Vào chế độ chọn, chạm Magni bị từ chối | Nút từ chối ngay, báo "Cần 2 Magni"; năng lượng giữ 100 |
| `T12b` | Như trên với 2 Magni ngoài tầm | Nút từ chối, báo "Không có cặp trong tầm"; năng lượng giữ 100 |
| `T10` | Bấm nút khi bể trống | Dựng sẵn cặp Magni trước khi bấm |
| `T14` | Hủy chọn khi bể trống | Dựng sẵn cặp Magni |
| `T16` | Game Over khi đang chọn, bể trống | Dựng sẵn cặp Magni, thêm assert đang ở chế độ chọn trước khi thua |

Mục đích của từng test không đổi: không mất năng lượng sai, bấm UI không thả quái, hủy chọn được, Game Over chặn mọi thao tác.

## Soak (bot ngẫu nhiên, chỉ ClassicCap)

| Ván | Game-time | Thả | Merge | Burst | Điểm |
|---:|---:|---:|---:|---:|---:|
| 1 | 93s | 181 | 149 | 0 | 4915 |
| 2 | 95s | 185 | 163 | 0 | 5325 |
| 3 | 97s | 189 | 162 | 0 | 5090 |

Soak cho OverloadBurst chưa chạy, sẽ thêm ở TASK-06. Bot không đại diện cho người chơi thật.

Số liệu nam châm từ soak sau TASK-02 (bot không dùng kỹ năng): năng lượng đầy lần đầu ở giây 5–6, cặp Magni đầu tiên ở giây 11–13. Khớp với nhận xét năng lượng hồi quá nhanh trong báo cáo playtest.

## Bằng chứng hình ảnh

Lưu tại `production/qa/evidence/prototype-v0.2/`: `mm_playing_9x16.jpg`, `mm_gameover_9x16.jpg`, `mm_playing_9x20.jpg`, `mm_overload_burst_9x16.jpg`, `mm_magni_can_2_9x16.jpg`, `mm_magni_ready_9x16.jpg`.

Quan sát từ ảnh TASK-02:
- Chỉ có 1 Magni: nút mờ, ghi "Cần 2 Magni".
- Có cặp trong tầm: hai Magni sáng, nút sáng, hướng dẫn một lần hiện phía trên bể.
- Lần chụp đầu, ô thông báo nằm dưới đáy bể nên câu hướng dẫn bị cắt và đè lên bể. Đã dời ô lên dải trống giữa HUD và miệng bể, chụp lại thì đọc rõ. Vị trí thông báo ở tỉ lệ 9:20 chưa được chụp.

Quan sát từ ảnh burst:
- Điểm 300 và năng lượng 25% khớp với test.
- Hiệu ứng cùng màu cam với Crag nên dễ bị nhìn nhầm thành một Crag còn sót. Xử lý ở TASK-04.
- Không thấy popup "+300". Cách chụp phải đổi kiểu Canvas, nên chưa biết đây là do cách chụp hay lỗi thật.

## Chưa chạy

| Hạng mục | Trạng thái |
|---|---|
| `burstPushImpulse` lớn hơn 0 | NOT RUN: mặc định 0, chưa có test |
| Soak OverloadBurst | NOT RUN: thuộc TASK-06 |
| Chơi tay 6 ván A/B | MANUAL TEST PENDING |
| Android | MANUAL TEST PENDING: chưa build APK |
