# Changelog

Ghi các thay đổi người chơi và người phát triển nhận thấy được, theo từng phiên bản prototype.

## v0.2 "Fun & Balance" — 10/11/2026

Yêu cầu: `production/monster-merge-tasks-v0.2.md`. Báo cáo: `production/qa/V02_IMPLEMENTATION_REPORT.md`, `production/qa/V02_TEST_REPORT.md`.

### Thêm
- Luật cặp cấp cuối chọn được trong Inspector (`MaxTierMergeMode`): `ClassicCap` giữ như v0.1, `OverloadBurst` cho hai Crag biến mất, +300 điểm, có vòng nổ.
- Nút nam châm báo lý do khi chưa dùng được ("Cần 2 Magni", "Không có cặp trong tầm"); Magni có cặp trong tầm được làm sáng; hướng dẫn một lần khi nam châm lần đầu dùng được.
- Juice khi hợp nhất: phần hình nảy lên, vòng sáng tại điểm hợp nhất, tiếng bíp, combo từ x3 được nhấn; bật/tắt bằng `JuiceEnabled`.
- Vạch dọc mờ chỉ vị trí thả; vạch tràn dày lên và nhấp nháy khi thật sự sắp thua.
- Log số liệu cuối ván trong Console: chế độ, điểm, thời gian, số merge, số burst, số liệu nam châm.
- Menu `Tools > Monster Merge > Build Android Dev APK`.

### Thay đổi
- Số trên quái luôn đứng thẳng khi thân lăn.
- Hướng dẫn lần đầu rút còn 2 câu trong ô nhỏ hơn.
- Ô thông báo dời lên dải trống phía trên bể, không còn đè lên đáy bể.
- Tối đa 6 popup điểm và 8 vòng hiệu ứng cùng lúc.

### Test
- 54/54 test tự động pass; 29 test v0.1 không hồi quy. Chi tiết trong `V02_TEST_REPORT.md`.
- **Chưa test thủ công:** chơi tay A/B và test trên điện thoại được hoãn đến khi game hoàn thiện.

### Ghi chú
- Mặc định vẫn là `ClassicCap`. Soak cho thấy với `OverloadBurst`, bot ngẫu nhiên không thua ván nào trong 10 phút; cần cân nhắc khi chọn chế độ.

## v0.1 — 06/11/2026

Yêu cầu: `production/monster-merge-tasks-v0.1.md`. Review: `production/qa/prototype-v0.1-review.md`.

### Thêm
- Core loop: kéo để chọn vị trí, nhả để thả, hai quái cùng cấp hợp nhất, điểm và kỷ lục, danger line, Game Over, Chơi lại.
- 5 cấp quái với vật lý riêng: Blip, Boing (nảy), Goo (khó lăn), Magni, Crag (nặng).
- Năng lượng và kỹ năng Magnetic Pull.
- Editor tool dựng scene và bộ PlayMode test.
