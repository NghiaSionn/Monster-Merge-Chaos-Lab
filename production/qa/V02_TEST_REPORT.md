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
| 07/11/2026 | Sau TASK-03, chưa commit | 43 | 0 | 0 | Thêm 3 test; lần chạy đầu 42/1 do `T03b` đo sai thời điểm (xem dưới); Console không có lỗi |
| 07/11/2026 | Sau TASK-04, commit `16c0c32` | 47 | 0 | 0 | Thêm 4 test; scene dựng lại; Console không có lỗi |
| 07/11/2026 | Sau TASK-05, chưa commit | 53 | 0 | 0 | Thêm 6 test; lần chạy đầu 52/1 do test touch xử lý sự kiện sai thời điểm (xem dưới); Console không có lỗi |

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

## Test mới của TASK-03

| Test | Kịch bản | Kết quả | Khi tắt dòng giữ nhãn đứng |
|---|---|---|---|
| `V02_T03a_RotatedMonster_LabelStaysUpright` | Thân xoay 90°, 180°, 270°: nhãn lệch dưới 0,5° | AUTO-TEST PASS | Fail: lệch 90° |
| `V02_T03b_SpinningMonster_KeepsSpinning_LabelUpright` | Quái quay 360°/giây: nhãn đứng ở mọi frame, quái vẫn quay | AUTO-TEST PASS | Fail: lệch 1,8° |
| `V02_T03c_MergedMonster_LabelUprightAndNoLeftoverLabels` | Sau merge nhãn đứng; sau Restart chỉ còn nhãn của preview | AUTO-TEST PASS | Vẫn pass |

Đã chạy lại 3 test này khi tạm tắt dòng giữ nhãn đứng để chắc test bắt được lỗi, rồi khôi phục code. `T03c` vẫn pass vì quái mới sinh luôn bắt đầu ở góc 0°, nên phần kiểm tra nhãn sau merge không phân biệt được; giá trị thật của test này là phần kiểm tra không còn nhãn sót sau Restart.

`T03b` lần đầu fail với code đúng, lệch 1,4°. Nguyên nhân là test đo ngay sau `yield return null`, trước khi `LateUpdate` chạy. Đã đổi sang đo ở cuối frame (`WaitForEndOfFrame`), giữ nguyên ngưỡng 0,5°.

## Test mới của TASK-04

| Test | Kịch bản | Kết quả |
|---|---|---|
| `V02_T04a_TwentyFourMerges_FxPopupsAndSfxStayBounded` | 24 cặp Blip hợp nhất trong 2 đợt. Thực tế 40 merge (có chain), tối đa 8 vòng sáng và 6 popup cùng lúc (đúng giới hạn), 5 tiếng không chồng nhau; sau 1 giây không còn hiệu ứng | AUTO-TEST PASS |
| `V02_T04b_VisualPop_AnimatesBodyNotPhysicsRoot` | Phần hình phình trên 5%, gốc vật lý không vượt đường kính, collider giữ bán kính 0,5, kết thúc về đúng kích thước | AUTO-TEST PASS |
| `V02_T04c_JuiceDisabled_NoRingNoSfxNoPop_PopupKept` | Tắt juice: không vòng sáng, không tiếng, không pop; popup điểm của v0.1 vẫn còn | AUTO-TEST PASS |
| `V02_T04d_ComboFromThree_IsEmphasized_OnlyWithJuice` | Combo x3: chữ đổi màu nhấn và phóng to; tắt juice thì không | AUTO-TEST PASS |

Số tiếng ít hơn nhiều so với số merge vì nhiều merge rơi vào cùng frame và tiếng sau bị bỏ do khoảng cách tối thiểu 0,05 giây. Test chỉ chứng minh tiếng có phát và không chồng nhau; **chưa ai nghe thử** âm thanh.

## Test mới của TASK-05

| Test | Kịch bản | Kết quả |
|---|---|---|
| `V02_T05a_DropGuide_FollowsPreviewDownToFloor_HiddenWithPreview` | Vạch dọc cùng X với preview, chạy từ dưới preview đến đáy; ẩn cùng preview khi cooldown | AUTO-TEST PASS |
| `V02_T05b_ContinuousDropSpam_NeverWarns` | Thả liên tục 12 lần: vạch không lần nào báo động | AUTO-TEST PASS |
| `V02_T05c_RealOverflow_WarnsClearlyBeforeGameOver` | Quái kẹt trên vạch: cảnh báo bật khi còn đang chơi, vạch dày hơn 2 lần, sau đó mới thua | AUTO-TEST PASS |
| `V02_T05d_WarningClears_WhenTankIsSafeAgain` | Hết nguy hiểm: tắt cảnh báo, bộ đếm về 0, vạch về độ dày cũ | AUTO-TEST PASS |
| `V02_T05e_TouchTap_DropsExactlyOne_AtFingerX` | Chạm cảm ứng giả lập trong bể: bắt đầu ngắm, thả đúng 1 quái tại X của ngón tay | AUTO-TEST PASS |
| `V02_T05f_TouchOnMagnetButton_FiresButton_DoesNotDrop` | Chạm cảm ứng lên nút nam châm: nút nhận chạm, không thả quái | AUTO-TEST PASS |
| Trong `CaptureScreens` | Ở 9:20 không có UI bấm được nào che vùng chơi | AUTO-TEST PASS |

### Test touch lần đầu fail

`T05e` lần đầu fail: chạm trong bể không bắt đầu ngắm. Chẩn đoán cho thấy `Pointer.current` đúng là touchscreen, vị trí đúng, đang nhấn, nhưng khi `DropController.Update` chạy thì `wasPressedThisFrame` đã là `false`. Nguyên nhân: test xử lý sự kiện chạm ngay giữa hai frame; tới frame sau Input System chạy thêm một lượt, và touchscreen tự cập nhật lại trạng thái ở mỗi lượt nên mất dấu "vừa nhấn". Đã sửa test để xếp hàng sự kiện (`queueEventOnly: true`), giống cách sự kiện tới trên máy thật.

**Rủi ro còn lại:** `DropController` và `AbilityManager` đọc cú chạm bằng `wasPressedThisFrame`. Nếu trên thiết bị có lượt cập nhật input nào chạy giữa lúc chạm và lúc `Update` đọc, cú chạm sẽ bị bỏ lỡ. Theo cấu hình mặc định thì không xảy ra, nhưng **chỉ T17 trên máy Android thật mới xác nhận được**.

## Soak (bot ngẫu nhiên, chỉ ClassicCap)

| Ván | Game-time | Thả | Merge | Burst | Điểm |
|---:|---:|---:|---:|---:|---:|
| 1 | 93s | 181 | 149 | 0 | 4915 |
| 2 | 95s | 185 | 163 | 0 | 5325 |
| 3 | 97s | 189 | 162 | 0 | 5090 |

Soak cho OverloadBurst chưa chạy, sẽ thêm ở TASK-06. Bot không đại diện cho người chơi thật.

Số liệu nam châm từ soak sau TASK-02 (bot không dùng kỹ năng): năng lượng đầy lần đầu ở giây 5–6, cặp Magni đầu tiên ở giây 11–13. Khớp với nhận xét năng lượng hồi quá nhanh trong báo cáo playtest.

## Bằng chứng hình ảnh

Lưu tại `production/qa/evidence/prototype-v0.2/`: `mm_playing_9x16.jpg`, `mm_gameover_9x16.jpg`, `mm_playing_9x20.jpg`, `mm_overload_burst_9x16.jpg`, `mm_magni_can_2_9x16.jpg`, `mm_magni_ready_9x16.jpg`, `mm_labels_rotated_9x16.jpg`, `mm_merge_ring_9x16.jpg`, `mm_danger_warning_9x16.jpg`.

Quan sát từ ảnh TASK-05:
- Cảnh báo: vạch dày và đỏ rõ khi có Goo kẹt trên vạch.
- Vạch dọc mờ từ preview xuống đáy; ô hướng dẫn chỉ còn 2 câu, nhỏ hơn.
- 9:20: bố cục gọn, không chồng.
- Trong ảnh, vạch dọc vẽ đè lên ô hướng dẫn. Đây là hạn chế của cách chụp (phải đổi kiểu Canvas); trong game thật UI Overlay luôn nằm trên, nhưng chưa xác minh bằng mắt.

Quan sát từ ảnh TASK-04:
- Vòng nổ burst giờ là vòng rỗng vàng nhạt, không còn nhầm với Crag.
- Merge cặp Goo: vòng tím nhạt vẽ sau quái nên không che số; quái mới đang sáng và nảy. Vòng vàng ngoài cùng là vòng burst trước đó chưa tắt hẳn.
- Popup "+60" không thấy trong ảnh, cùng hạn chế của cách chụp như popup "+300".

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
