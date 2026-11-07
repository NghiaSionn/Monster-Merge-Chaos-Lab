# Playtest: người phát triển, Unity Editor

| Mục | Giá trị |
|---|---|
| Ngày | 07/11/2026, khoảng 00:19–00:21 |
| Người chơi | Người phát triển |
| Môi trường | Unity Editor 6000.6.8f1, build target Android, Game View tỉ lệ dọc, chuột thật |
| Nguồn | Video 2 phút 3 giây `2026-11-07 00-19-35.mp4`, lưu trên máy người phát triển, không đưa vào repo |
| Bằng chứng | `production/qa/evidence/prototype-v0.1/` |

**Giới hạn của báo cáo:** nội dung dưới đây do Claude rút ra từ 13 khung hình trích từ video, không phải xem liên tục. Những gì xảy ra giữa các khung là không biết. Video không có lời nhận xét của người chơi, nên các câu về cảm nhận được để trống. Vị trí và số lượng đếm bằng mắt trên ảnh.

## Diễn biến ván chơi

| Thời điểm | Điểm | Năng lượng | Quan sát |
|---:|---:|---:|---|
| 0:05 | 10 | 25% | Kỷ lục đã là 5340 từ trước |
| 0:26 | 265 | 100% | Combo x3, đã có Magni |
| 0:46 | 915 | 100% | Đã có 2 Crag |
| 1:03 | 1290 | 0% | Năng lượng đã được dùng ít nhất một lần trước đó |
| 1:05 | 1360 | 100% | Từ 0% lên 100% trong khoảng 2,5 giây, Combo x4 |
| 1:07 | 1605 | 0% | Dùng nam châm, hiện "Hút!", hai Magni sáng lên, Combo x8 |
| 1:09 | 1605 | 0% | Hai Magni vẫn ở nguyên chỗ |
| 1:12 | 1885 | 100% | Năng lượng lại đầy |
| 1:27 | 2585 | 100% | Combo x4 |
| 1:48 | 3565 | 100% | Combo x6 |
| 1:58 | 4030 | 100% | Cột bên phải vượt vạch, Combo x9 |
| 2:01 | 4030 | 100% | Game Over "Bể đã tràn!", kỷ lục giữ 5340 |

## Hoạt động đúng

- Thả, hợp nhất, điểm, combo, thanh năng lượng và nút nam châm chạy bằng chuột thật.
- Game Over xảy ra tự nhiên do bể đầy. Điểm cuối 4030 và Kỷ lục 5340 hiển thị đúng; nút Nam châm bị khóa.
- Không thấy quái thoát khỏi bể hay nhân bản trong các khung đã xem.

## Vấn đề cần xử lý

| # | Vấn đề | Bằng chứng | Ý nghĩa |
|---|---|---|---|
| 1 | **Crag chiếm bể.** Khi thua có 8 Crag, không hợp nhất được, chiếm khoảng 40% diện tích bể dưới vạch (tính theo bán kính, chưa kể khe hở) | `04`, `05` | Đúng rủi ro "Ván bị kẹt ở Crag" trong GDD mục 6.3. Crag đầu tiên xuất hiện trước 0:46, nên phần lớn ván là tích Crag |
| 2 | **Năng lượng hồi quá nhanh.** 0% lên 100% trong khoảng 2,5 giây; mọi khung từ 1:12 đến hết ván đều báo 100% | Bảng trên | 4 merge là quá rẻ khi quái nhỏ dồn nhiều. Quyết định "dùng lúc nào" mà GDD mục 4.3 mong muốn gần như biến mất |
| 3 | **Nam châm tốn 100% mà không có kết quả.** Lúc 1:07 hai Magni nằm kề nhau, bị Crag chặn, không di chuyển sau 2 giây | `02`, `03` | GDD chấp nhận hút không đảm bảo merge, nhưng người chơi mất năng lượng mà không thấy gì xảy ra |
| 4 | **Combo quá dễ đạt.** Thường thấy x4–x9 | `01`, bảng trên | Cửa sổ 1,5 giây dài so với cooldown thả 0,5 giây, nên combo khó tạo cảm giác đặc biệt |
| 5 | **Chữ khó đọc.** Số xoay theo quái; "+điểm" chồng nhau và lẫn vào quái đang sáng | `01`, `02` | Ảnh hưởng nhỏ, chỉ cần xử lý khi làm art thật |

## Câu hỏi playtest (GDD mục 6.1)

| Câu hỏi | Trả lời |
|---|---|
| Người mới có tự hiểu sau 30–60 giây? | Chưa trả lời được: người chơi là tác giả |
| Quái nảy và ma sát có vui hơn không? | **Người chơi cần tự trả lời** |
| Có hiểu và muốn dùng nam châm không? | Có dùng ít nhất 2 lần. Lúc thua năng lượng vẫn đầy 100%. Có muốn dùng hay không: **người chơi cần tự trả lời** |
| Thua do mình hay do RNG/vật lý? | **Người chơi cần tự trả lời** |
| Có tự bấm Restart không? | Video kết thúc ở màn Game Over, chưa thấy bấm |

## Hướng cân bằng cần quyết định

Chưa có thay đổi nào được áp dụng. Sprint v0.2 chọn so sánh A/B cho vấn đề Crag và loại tier 6; ba hướng còn lại chưa có task, xem `production/v0.2-scope-notes.md`.

1. Crag: đã chọn thử `ClassicCap` với `OverloadBurst` (TASK-01).
2. Năng lượng: giảm `energyPerMerge`, hoặc chỉ cộng cho merge từ cấp 3 trở lên.
3. Nam châm: tăng lực, hoặc không trừ năng lượng nếu hai Magni không dịch chuyển.
4. Combo: rút ngắn `comboWindow`.
