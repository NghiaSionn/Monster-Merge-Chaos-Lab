# MONSTER MERGE: CHAOS LAB
## Game Design Document (GDD) — Prototype v0.1

> **Trạng thái:** Đã chốt định hướng; thông số cân bằng vẫn là giả định cần playtest.  
> **Cập nhật:** 06/11/2026  
> **Mục đích:** Tài liệu thiết kế thống nhất cho người phát triển và Claude Code.  
> **Ưu tiên:** Ra được bản **chơi thử được** trước; không làm game hoàn chỉnh ngay.

---

## 1. Tổng quan

| Thuộc tính | Quyết định |
|---|---|
| Tên tạm | **Monster Merge: Chaos Lab** |
| Thể loại | **2D Physics Merge Puzzle**; định hướng Hybrid-Casual nếu phát triển thêm tiến trình sau này |
| Nền tảng | **Android / Google Play**; phát triển và thử nghiệm đầu tiên trong Unity Editor |
| Engine | **Unity 2D, C#** |
| Màn hình | **Dọc 9:16**, điều khiển bằng một ngón tay; chuột trong Editor |
| Đối tượng | Người chơi thích game giải trí ngắn, dễ hiểu, hiệu ứng vui nhộn |
| Chế độ đầu tiên | **Endless / High Score** (một màn chơi, một bể chứa) |
| Cảm hứng cốt lõi | Suika: thả vật thể, va chạm vật lý, hợp nhất cùng cấp, quản lý không gian |
| Khác biệt dự kiến | **Mỗi cấp quái có đặc tính vật lý riêng** + **kích hoạt kỹ năng chủ động bằng năng lượng hợp nhất** |
| Phong cách | Quái vật tròn, đáng yêu, hài hước; phòng thí nghiệm màu sắc tươi sáng |
| Mô hình kinh doanh tương lai | Quảng cáo thưởng và/hoặc quảng cáo giữa ván ở thời điểm phù hợp **sau khi có bằng chứng game thú vị** |
| Ràng buộc nguồn lực | Một lập trình viên Unity, khoảng **5–10 giờ/tuần** |

### 1.1. Elevator pitch

> Thả những quái vật tròn đáng yêu vào một bể thí nghiệm. Hai con cùng cấp chạm nhau sẽ hợp nhất thành quái vật cấp cao hơn. Nhưng mỗi cấp có tính chất vật lý khác nhau: có con nảy mạnh, có con rất bám, có con nặng. Hợp nhất để tích năng lượng và **chọc vào quái nam châm** để kéo hai quái vật cùng cấp lại gần nhau, tạo combo, giải phóng không gian và phá kỷ lục trước khi bể tràn.

### 1.2. Trải nghiệm mục tiêu

1. **Hiểu trong khoảng 10 giây:** kéo để chọn vị trí, thả để rơi, hai con giống nhau sẽ hợp nhất.
2. **Thỏa mãn khi hợp nhất:** hình ảnh, âm thanh và chuyển động tạo cảm giác thưởng ngay lập tức.
3. **Vui vì vật lý:** lăn, bật, va chạm, xếp chồng tạo tình huống bất ngờ, nhưng **không hỗn loạn đến mức mất kiểm soát**.
4. **Có quyết định nhỏ nhưng đáng giá:** dùng năng lượng đúng lúc để chủ động tạo hợp nhất.
5. **Chơi ngắn, chơi lại dễ:** không ép theo cốt truyện, không cần hệ thống chiến đấu.

### 1.3. Chưa làm / ngoài phạm vi (NON-GOALS)

**Không có trong Prototype v0.1:**
- **Không chiến đấu**: không AI địch, không máu, không damage, không auto-battle, không phòng thủ tháp.
- Không nhân vật di chuyển độc lập/đi lại trong bể; chuyển động chỉ do **Rigidbody2D, va chạm, lực**.
- Không multiplayer, leaderboard online, đăng nhập, cloud save.
- Không cửa hàng, gacha, tiền tệ, nâng cấp dài hạn, skin/collection, nhiệm vụ hằng ngày.
- Không nhiều bản đồ, màn chơi hoặc các loại bể phức tạp.
- Không tích hợp quảng cáo, IAP, analytics/SDK ngoài ở prototype.
- Không vẽ asset/animation đẹp trước khi xác nhận gameplay.
- Không mô phỏng nước/slime thật, soft-body, khớp vật lý dính hoặc custom physics engine.

> **Quy tắc:** Cái gì không cần để chứng minh vòng lặp **Thả → Hợp nhất → Tích năng lượng → Kích hoạt nam châm → Tạo combo → Tránh tràn** thì không đưa vào v0.1.

---

## 2. Luật chơi cốt lõi

### 2.1. Điều khiển

- Mỗi lượt có một **quái vật hiện tại** ở phía trên bể; có thể hiện **quái vật kế tiếp** trên HUD.
- Người chơi **chạm/kéo ngang** để di chuyển vị trí thả theo trục X, sau đó **thả ngón tay** để quái vật rơi tự do.
- Khi chạy trong Unity Editor: dùng chuột tương đương thao tác cảm ứng.
- Không cho thả ra ngoài thành bể; phạm vi X được tính theo **bán kính Collider2D của cấp đang thả**.
- Thả xong có một **cooldown ngắn** trước khi thả tiếp nhằm tránh spam và lỗi va chạm.
- Không có nút nhảy, kéo trực tiếp các quái vật đã rơi, hoặc điều khiển khi chúng đang rơi.

### 2.2. Hợp nhất

- Hai quái vật **cùng cấp** va chạm hợp lệ sẽ được thay bằng **một quái vật cấp kế tiếp** tại vị trí gần trung điểm của cặp.
- Khối lượng, kích cỡ, độ nảy và ma sát của quái mới lấy từ dữ liệu cấp quái, không lấy ngẫu nhiên.
- Hợp nhất có hiệu ứng tức thì (scale pop hoặc particle tối giản) và tăng điểm.
- Một con chỉ được tham gia **tối đa một lần hợp nhất** trong cùng thời điểm. Khóa trạng thái merge trước khi tạo/hủy để tránh nhân đôi đối tượng.
- Một chuỗi quái mới lập tức chạm quái cùng cấp có thể tiếp tục hợp nhất, tạo **combo**.
- **Giới hạn prototype có 5 cấp:** cặp cấp 5 **chưa hợp nhất** (giữ nguyên) trong v0.1. Đây là giới hạn kỹ thuật để kiểm nghiệm core loop, **không phải luật vĩnh viễn**. Nếu khiến ván chơi kẹt quá nhanh, cần playtest rồi mới quyết định bổ sung cấp hoặc quy tắc giải phóng bể.

### 2.3. Sinh quái mới

- Prototype chỉ thả **cấp 1–2** (cấu hình được tỷ lệ, mặc định gợi ý 80% cấp 1 và 20% cấp 2).
- Quái vật từ cấp 3 trở lên chỉ tạo thông qua hợp nhất.
- Hàng đợi current/next phải nhất quán; không đổi cấp hiển thị khi người chơi đang giữ ngón tay.
- Random dùng `UnityEngine.Random`, có thể thay seed trong giai đoạn test để tái hiện lỗi.

### 2.4. Điểm số và combo

- Điểm tăng theo **cấp được tạo** sau khi hợp nhất; bảng điểm để cấu hình thay vì hardcode.
- **Giá trị khởi đầu tham khảo:** tạo cấp 2/3/4/5 lần lượt +10/+25/+60/+150 điểm.
- **Combo tham khảo:** các lần hợp nhất liên tiếp cách nhau tối đa 1,5 giây cộng bộ đếm combo và hiệu ứng hiển thị. Chưa bắt buộc cộng hệ số điểm ở prototype đầu; chỉ thêm nếu không gây rối việc kiểm tra.
- Hiển thị **Score** và **Best Score**; lưu Best Score trên máy bằng `PlayerPrefs` (không có tài khoản).

### 2.5. Tràn bể / kết thúc

- Có một **danger line** nằm ngang gần phía trên bể, bên dưới vùng spawn.
- Nếu **một hay nhiều quái vật đã được thả** chiếm vùng nguy hiểm **liên tục ~2 giây**, kết thúc ván.
- Không tính quái vật preview chưa thả; không tính trạng thái chồng vật thể ngắn do spawn/merge trong 1–2 frame.
- Khi hết ván: chặn input thả/kỹ năng, dừng logic spawn, hiển thị Game Over, Score, Best, nút **Restart**.
- Restart phải reset sạch object runtime, timer, năng lượng, combo và trạng thái hợp nhất, giữ Best Score.

---

## 3. Hệ thống 5 cấp quái vật

**Quyết định đã chốt:** 5 cấp cho prototype, mỗi cấp có hành vi vật lý khác biệt, **chỉ một kỹ năng chủ động: Nam châm**.

| Cấp | Tên tạm | Tính cách / nhận diện | Vật lý / vai trò | Kỹ năng chủ động |
|---:|---|---|---|---|
| 1 | **Blip** | Xanh dương, mầm nhỏ | Nhẹ, lăn bình thường; quái cơ bản | Không |
| 2 | **Boing** | Vàng, vui nhộn | **Nảy** rõ hơn khi va chạm | Không |
| 3 | **Goo** | Xanh lá, dính | **Ma sát cao**, khó lăn; tạo chướng ngại/điểm tựa | Không |
| 4 | **Magni** | Tím, hình ảnh nam châm | Vật lý trung tính; cấp có năng lực | **Hút một Magni khác gần đó khi được kích hoạt** |
| 5 | **Crag** | Cam/đất, đá tròn | **Nặng, ít nảy**, thay đổi cách xếp lớp | Không ở v0.1 |

> **Lưu ý thiết kế:** "Goo dính" trong prototype là **ma sát cao**, **không phải liên kết hai Rigidbody2D bằng joint**. "Crag nặng" là mass + bounce thấp, **không có sát thương hay kỹ năng dậm đất** ở v0.1. Khác biệt nằm ở vật lý, không làm thành 5 hệ kỹ năng.

### 3.1. Thông số ban đầu để test — KHÔNG phải thông số đã cân bằng

Đơn vị kích thước là Unity world unit; tham số phải chỉnh được bằng `ScriptableObject` hoặc Inspector.

| Cấp | Bán kính Collider2D | Mass | Bounciness | Friction |
|---:|---:|---:|---:|---:|
| 1 | 0.22 | 0.6 | 0.05 | 0.4 |
| 2 | 0.29 | 0.8 | 0.65 | 0.2 |
| 3 | 0.37 | 1.0 | 0.03 | 1.0 |
| 4 | 0.47 | 1.1 | 0.12 | 0.5 |
| 5 | 0.60 | 2.2 | 0.00 | 0.8 |

- Mọi cấp dùng `Rigidbody2D (Dynamic)` và `CircleCollider2D`.
- Cho phép dùng một prefab chung, thay SpriteRenderer/scale/collider/mass/material lúc khởi tạo.
- **Không** tự động kích hoạt lực hút khi các Magni ở cạnh nhau; chỉ hút khi người chơi chủ động dùng kỹ năng.
- `PhysicsMaterial2D` có thể được tạo riêng theo cấp hoặc dùng cấu hình từ dữ liệu.

### 3.2. Art direction (để về sau)

- Mỗi con có silhouette **tròn** để đơn giản collider, nhưng nhận diện khác nhau bởi mắt, sừng, họa tiết.
- Màu tươi, nét rõ, không tạo hiệu ứng che tầm nhìn.
- Một bộ sprite thống nhất phong cách; **không dùng hình của IP/game có bản quyền**.
- Prototype: **5 hình tròn khác màu + số/cấp** là đủ.

---

## 4. Cơ chế khác biệt: Năng lượng và "Chọc quái vật"

### 4.1. Luật năng lượng

- Mỗi lần hợp nhất hợp lệ cộng năng lượng.
- Gợi ý khởi đầu: **+25% mỗi lần**, tối đa **100%**.
- Chỉ khi đủ 100% mới dùng kỹ năng; không tự tiêu hao theo thời gian.
- Khi dùng kỹ năng thành công, **tiêu hết 100%**, trở về 0.
- Nếu không có mục tiêu hợp lệ, **không tiêu năng lượng**.

### 4.2. Kỹ năng Magni: Magnetic Pull

1. Khi năng lượng 100%, UI thể hiện trạng thái sẵn sàng.
2. Người chơi **nhấn nút Activate** để vào chế độ chọn mục tiêu; sau đó **chạm vào một Magni đang nằm trong bể**. (Tránh nhầm với thao tác kéo/thả.)
3. Nếu có **ít nhất một Magni khác** trong bán kính cho phép, chọn con gần nhất một cách ổn định.
4. Trong khoảng **0,6 giây**, tạo lực hút có giới hạn để hai Magni tiến lại gần; va chạm cùng cấp sẽ được `MergeManager` xử lý như bình thường.
5. Trừ năng lượng **khi bắt đầu kích hoạt hợp lệ**. Việc hút không cần đảm bảo 100% hợp nhất, nhưng phải có phản hồi rõ ràng.
6. Nhấn Cancel hoặc nhấn nút kỹ năng lần nữa để thoát chọn mục tiêu mà không tiêu năng lượng.

**Các tham số thử nghiệm:** tầm hút 2.5 unit, thời gian 0.6 giây, lực hút 8–15 (tùy timestep/mass), max velocity được giới hạn; có thể tinh chỉnh trong Inspector.

**Edge cases cần giải quyết:** target đã bị merge/hủy trong lúc hút; hai Magni bị quái khác cản; năng lượng đạt 100 khi đang kéo quái mới; kích hoạt ở chế độ game over; click/tap vào UI không làm rơi quái; bể có đúng một Magni.

### 4.3. Vì sao cơ chế này là trọng tâm

Không chỉ tăng số lượng quái: người chơi phải quyết định **dùng năng lượng lúc nào và trên con nào**. Đây là **giả thuyết khác biệt** cần kiểm thử; nếu kỹ năng luôn là lựa chọn hiển nhiên hoặc khó hiểu, phải sửa cơ chế thay vì thêm quá nhiều kỹ năng mới.

---

## 5. Camera, bể, giao diện

### 5.1. Màn hình gameplay (portrait)

```text
┌────────────────────────────────┐
│  SCORE: 0000       BEST: 0000  │
│  Next: (quái kế tiếp)          │
│                                │
│      [quái sắp thả / preview]  │
│       ┌────────────────┐       │
│       │ - - Danger - - │       │
│       │                │       │
│       │   o   O        │       │
│       │ o O  o   O     │       │
│       │ O o O o O      │       │
│       └────────────────┘       │
│  Energy: ███████░░░ 75%       │
│  [Activate Magnet]             │
└────────────────────────────────┘
```

- Bể có **đáy + hai tường** bằng `BoxCollider2D` (không dùng collider quanh cả bể gây khóa miệng trên).
- `DangerZone` là vùng kiểm tra độc lập, không phải tường vật lý.
- UI dùng Canvas với anchor/safe area phù hợp màn Android có tai thỏ; HUD không được che vùng thả.
- Khi game over hiển thị overlay có nút chơi lại.

### 5.2. Âm thanh và hiệu ứng

- **Prototype tối thiểu:** tween/scale nảy nhỏ khi merge, hiển thị `+Score`, phản hồi khi kích hoạt hút, thông báo game over.
- Âm thanh và VFX hoàn thiện **sau** khi vòng lặp vật lý đã hoạt động và được playtest; không được chặn tiến độ prototype.

---

## 6. Cân bằng và tiêu chí thành công

### 6.1. Câu hỏi phải trả lời bằng playtest

1. Sau 30–60 giây, người chơi có tự hiểu cách thả/hợp nhất không?
2. Quái nảy và quái ma sát cao có **vui hơn** quái chỉ khác kích cỡ không?
3. Người chơi có hiểu và **muốn dùng** năng lượng nam châm không?
4. Khi bể đầy, họ cảm thấy do quyết định của mình hay do RNG/vật lý thiếu công bằng?
5. Sau khi thua, họ có tự nguyện bấm **Restart** không?

### 6.2. Kiểm thử sơ bộ

- Test trước bằng người phát triển, sau đó đưa cho **5–10 người** chưa biết game; nếu có điều kiện mở rộng **10–20 người**.
- Ghi lại: thời lượng ván, có dùng nam châm không, số lần chơi lại, lỗi merge, tình huống khó chịu, phản ứng khi combo.
- **Không** coi các mốc thời lượng là sự thật thị trường; phiên bản đầu chỉ nhằm chứng minh gameplay đủ hấp dẫn.

### 6.3. Các rủi ro cần cảnh giác

| Rủi ro | Hậu quả | Hướng xử lý |
|---|---|---|
| Giống Suika quá nhiều | Không có lý do để chọn game này | Chứng minh sự hữu ích của Magnet/physics khác biệt |
| Boing bật lên vượt vạch quá dễ | Cảm giác bất công | Giảm bounciness, giảm velocity, tăng tolerance danger |
| Goo dính bằng ma sát khiến kẹt | Bể đầy nhanh, bực bội | Tinh chỉnh friction/size/khối lượng |
| Chỉ 5 cấp quá ít | Ván bị kẹt ở Crag | Cân bằng spawn/size trước; nếu chưa đạt, đề xuất mở rộng sau playtest |
| Kỹ năng vô dụng hoặc quá mạnh | Mất chiều sâu | Giới hạn tầm, lực, năng lượng và chọn mục tiêu |
| Quá tham tính năng | Không ra được bản thử | Giữ chặt NON-GOALS |

---

## 7. Kiến trúc kỹ thuật đề xuất (không bắt buộc tuyệt đối)

**Các vai trò chính, tránh tạo class thừa:**

- `MonsterData` (`ScriptableObject`): tier, màu/sprite, bán kính, mass, friction, bounce, điểm.
- `Monster` (`MonoBehaviour`): tier/runtime state, `Rigidbody2D`, `Collider2D`, khóa merge; không tự quản lý điểm/UI.
- `DropController`: nhận chuột/cảm ứng, preview, spawn và cooldown; bỏ qua tương tác khi click UI.
- `MergeManager`: nhận yêu cầu merge, khóa hai nguồn, thực hiện hợp nhất an toàn, phát sự kiện cộng điểm/năng lượng.
- `GameManager`: vòng đời game: Playing / GameOver / Restart.
- `AbilityManager`: năng lượng, trạng thái chọn kỹ năng, tìm mục tiêu, hút và hủy an toàn.
- `DangerZone`: giám sát vùng nguy hiểm theo timer liên tục.
- `GameUI`: hiển thị Score/Best/Next/Energy/GameOver.

**Kỹ thuật:** Unity Physics2D + prefab dùng chung, `ScriptableObject` để tinh chỉnh, event/callback vừa đủ, không cần ECS, DOTS, networking hoặc nhiều thư viện ngoài. Thiết kế tránh phụ thuộc cứng vào bản Unity cụ thể (ví dụ cách truy cập vận tốc Rigidbody2D).

---

## 8. Mốc phát triển / Definition of Done

### M0 — Lõi Suika có thể chơi
- Chạy trong Unity Editor, bể vật lý, thả bằng chuột, 5 cấp quái.
- Hai con cùng cấp hợp nhất chính xác, có điểm, không duplicate spawn/merge.
- Có giới hạn tràn, Game Over, Restart và Best Score.

### M1 — Khác biệt Monster Merge
- Khác biệt vật lý giữa Boing/Goo/Crag có thể cảm nhận.
- Có thanh năng lượng, chế độ chọn kỹ năng, Magni hút Magni hợp lệ.
- Vẫn ổn định khi xảy ra combo nhiều lần.

### M2 — Bản thử Android
- Chơi được bằng cảm ứng một tay ở màn hình dọc.
- Layout phù hợp tỉ lệ màn hình phổ biến; không crash, không bị mất input do UI.
- Có checklist kiểm thử, lỗi rõ ràng được sửa và ghi nhận.

> **Kết thúc prototype = đủ để đưa cho người khác chơi và đánh giá**; không đồng nghĩa đủ điều kiện để phát hành công khai hoặc bắt đầu kiếm tiền.

---

## 9. Vùng quyết định: CHỐT và CHƯA CHỐT

### ĐÃ CHỐT
- Game giải trí Unity 2D Android màn hình dọc, một tay.
- Monster Merge: Chaos Lab, cảm hứng Suika, vật lý khác nhau theo cấp.
- **Không có chiến đấu** ở bản đầu.
- **5 cấp quái** cho prototype, **1 kỹ năng chủ động Magni**, **Endless/High Score**.
- Prototype gameplay trước, sprite/ads/metagame sau.

### CHƯA CHỐT — phải đánh giá sau playtest
- Giá trị số học cụ thể (mass, friction, radius, thời gian danger, năng lượng).
- Hệ combo có nhân điểm hay không; khả năng thả level cao hơn; cấp tối đa sau prototype.
- Đồ họa/âm thanh cuối, lore, tên thương mại chính thức.
- Retention, chiến lược quảng cáo và phát hành thực tế.

**Thứ tự ưu tiên khi có xung đột:** (1) game chạy được → (2) hợp nhất đúng, công bằng → (3) vui, dễ hiểu → (4) cơ chế nam châm thực sự có ý nghĩa → (5) polish.
