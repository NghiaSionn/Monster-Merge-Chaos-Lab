# Game Brief: Monster Merge: Chaos Lab

<!-- Bản một trang rút từ design/gdd/monster-merge-gdd-v0.1.md. Chi tiết luật chơi và thông số xem GDD. -->

**One-sentence pitch:** Thả những quái vật tròn có vật lý riêng vào bể thí nghiệm, hợp nhất chúng để tích năng lượng, rồi dùng nam châm kéo hai quái cùng cấp lại gần nhau trước khi bể tràn.

## Core loop
- Kéo ngang chọn vị trí, nhả để thả quái cấp 1–2.
- Hai quái cùng cấp chạm nhau hợp nhất thành cấp cao hơn, cộng điểm và năng lượng.
- Đầy 100% năng lượng thì chọn một Magni để hút Magni gần nhất, tạo thêm hợp nhất và giải phóng chỗ.
- Thua thì chơi lại ngay để phá kỷ lục.

## Player goal & fail state — what "working" looks like
- Mục tiêu: đạt điểm cao nhất trong một ván Endless và phá Best Score.
- Thua khi quái đã thả nằm trên danger line liên tục khoảng 2 giây. Một ván hoàn chỉnh gồm thả, hợp nhất, dùng nam châm, tràn bể, Game Over, Restart.

## MVP — what must exist to be the game
- Bể vật lý với thả quái bằng một ngón tay hoặc chuột.
- 5 cấp quái: Blip, Boing (nảy), Goo (khó lăn), Magni, Crag (nặng).
- Hợp nhất an toàn: không nhân bản, không tính điểm hai lần, có chain và combo.
- Điểm, Best Score, danger line, Game Over, Restart.
- Năng lượng và kỹ năng chủ động duy nhất Magnetic Pull.
- Phản hồi tối thiểu: pop, +điểm, chỉ dẫn lần đầu.

## Out of scope — not building this
- Chiến đấu, AI địch, máu, damage.
- Multiplayer, leaderboard online, đăng nhập, cloud save.
- Shop, gacha, tiền tệ, nâng cấp, skin, nhiệm vụ hằng ngày.
- Quảng cáo, IAP, analytics, SDK ngoài ở prototype.
- Nhiều màn, nhiều loại bể, nhiều kỹ năng.
- Soft-body, joint dính, mô phỏng nước, custom physics.

## Build order
1. Core loop: thả, hợp nhất, điểm, Game Over, Restart. **Đã xong.**
2. Vật lý theo cấp, năng lượng, Magnetic Pull. **Đã xong.**
3. Cân bằng sau playtest: Crag chiếm bể, năng lượng hồi quá nhanh, nam châm bị kẹt. **Đang mở**, xem `production/qa/playtests/2026-11-07-dev-editor-playtest.md`.
4. Build APK và test trên máy Android thật. **Chưa làm.**
5. Cho 5–10 người chưa biết game chơi thử. **Chưa làm.**

---
**Who it's for / what they feel:** Người thích game giải trí ngắn, một tay, muốn cảm giác vật lý vui và bất ngờ nhưng không mất kiểm soát.

**Art & audio direction:** Quái tròn đáng yêu, phòng thí nghiệm màu tươi. Prototype chỉ dùng 5 hình tròn khác màu có số.

**Reference game:** Suika Game — giữ vòng lặp thả và hợp nhất trong một bể; thêm vật lý theo cấp và kỹ năng nam châm để tạo khác biệt.
