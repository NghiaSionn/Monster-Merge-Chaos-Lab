# MONSTER MERGE v0.2 — PHIẾU CHƠI THỬ A/B

Người chơi: __________  Ngày: __________  Bản build/commit: __________

**Chơi 3 ván mỗi chế độ.** A = ClassicCap (Lv5 không merge), B = OverloadBurst (Lv5+Lv5 biến mất + thưởng điểm). Thứ tự thử tốt nhất: A1, B1, A2, B2, A3, B3; tránh chơi hết A rồi mới B khi đang làm quen.

| Ván | Chế độ | Thời lượng (phút) | Điểm | Số lần dùng Magni thành công | Cảm giác còn cách cứu khi gần thua? (1–5) | Quá dễ/quá khó? | Ghi chú |
|---|---|---:|---:|---:|---:|---|---|
| 1 | A | | | | | | |
| 2 | B | | | | | | |
| 3 | A | | | | | | |
| 4 | B | | | | | | |
| 5 | A | | | | | | |
| 6 | B | | | | | | |

## Câu hỏi sau 6 ván

- Luật chơi có dễ hiểu trong 30 giây đầu không? __________
- Có lúc nào bạn không biết phải làm gì tiếp không? __________
- Khi đạt 100% năng lượng, bạn có biết vì sao Magni dùng được/không dùng được không? __________
- Combo/hiệu ứng có khiến bạn muốn tiếp tục thả không? __________
- Điều gì gây khó chịu nhất? __________
- Chế độ A hay B vui hơn và vì sao? __________
- Bạn có muốn chơi thêm một ván không? Vì sao? __________

> Không coi 6 ván của một người là dữ liệu đủ để kết luận game có khả năng giữ chân thị trường. Mục tiêu là phát hiện vấn đề trước khi nhờ thêm 5–10 người thử.

## Bổ sung theo TASK-07

Phiếu mẫu chưa có các mục dưới đây, nên được thêm vào để đủ yêu cầu của TASK-07.

| Ván | Lý do Game Over |
|---|---|
| 1 (A) | |
| 2 (B) | |
| 3 (A) | |
| 4 (B) | |
| 5 (A) | |
| 6 (B) | |

- Ván cố tình dùng Magni ngay khi đủ điều kiện: phản hồi có rõ không? __________
- So sánh `JuiceEnabled=false` và `true`: cảm giác merge khác thế nào? __________
- Best Score còn sau khi thoát Play Mode và vào lại? __________
- Best Score còn sau khi đóng/mở app Android? __________
- Hai vấn đề khó chịu nhất: __________

## Cách đổi chế độ A/B

1. Mở `Assets/MonsterMerge/Scenes/GameScene.unity`.
2. Chọn object `Managers`, tìm component `MergeManager`, nhóm **Cấp cuối (A/B)**.
3. Đặt `Max Tier Mode` thành `ClassicCap` (A) hoặc `OverloadBurst` (B), rồi bấm Play. Không đổi khi đang chơi: bấm Chơi lại sẽ quay về giá trị trong Inspector.
4. Cuối mỗi ván, Console ghi `[MonsterMerge] Hết ván: mode=..., điểm=..., thời gian=..., merge=..., burst=...`, có thể chép vào bảng trên.

**Bật/tắt juice:** `Managers` > `MergeEffects` > `Juice Enabled`. Tắt thì quay về cảm giác v0.1 (vẫn có flash và popup điểm); bật thì có thêm pop, vòng sáng, tiếng bíp và nhấn combo. Đổi trước khi bấm Play. Chưa nên điền phiếu trước khi TASK-02 đến TASK-05 xong, vì các ván phải chơi trên cùng một bản build.
