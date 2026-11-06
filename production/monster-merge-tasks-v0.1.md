# MONSTER MERGE: CHAOS LAB — TASK LIST CHO CLAUDE CODE
## Prototype v0.1 · Unity 2D · Android Portrait

> **Đọc trước:** [`MONSTER_MERGE_GDD_v0.1.md`](./MONSTER_MERGE_GDD_v0.1.md) là **source of truth** về ý tưởng và phạm vi.  
> **Vai trò Claude Code:** hỗ trợ triển khai vào **dự án Unity hiện có** (nếu đã có), hoặc tạo hướng dẫn/cấu trúc ban đầu nếu dự án chưa tồn tại.  
> **Ưu tiên:** Chạy được, kiểm chứng gameplay, ít phụ thuộc, không over-engineering.  
> **Quy ước:** `[ ]` chưa làm · `[x]` đã làm và có bằng chứng · `[!]` bị chặn (ghi lý do).

---

## 0. Lệnh khởi đầu cho Claude Code (đọc kỹ)

**Bạn là Unity 2D C# developer. Hãy triển khai Monster Merge prototype dựa trên GDD kèm theo.**

1. **Đầu tiên phải khảo sát repository**: đường dẫn Unity project, `ProjectSettings/ProjectVersion.txt`, Unity version, render pipeline, Input System (Old/New/Both), cấu trúc `Assets/`, scene và script hiện có, `.gitignore`, assembly definitions nếu có. **Không tự khẳng định những thứ chưa kiểm tra.**
2. Nếu dự án đã tồn tại, **giữ nguyên code/tài sản hiện có** và tạo file mới ở thư mục riêng `Assets/MonsterMerge/` trừ khi người dùng đã chỉ định cấu trúc khác. **Không xóa hoặc ghi đè scene/prefab/config đang dùng nếu chưa xem xét tác động.**
3. **Không cài package ngoài** trừ khi thật sự bắt buộc; ưu tiên Unity built-in 2D Physics, UI và Input có sẵn. Xử lý input tương thích với cấu hình project (New Input System/Old/Both), không tự ý đổi Active Input Handling làm hỏng project.
4. Không được tự thêm combat, AI địch, IAP, quảng cáo, shop, nâng cấp, nhiều chế độ hoặc hệ thống backend.
5. Làm theo **milestone** từ P0 đến P2, mỗi milestone phải compile/kiểm thử được trước khi sang milestone tiếp theo.
6. Sau mỗi milestone: báo cáo **file đã tạo/sửa, hướng dẫn chạy trong Editor, kết quả test thực tế, giới hạn chưa test, lỗi còn tồn tại**. **Không ghi `[x]` nếu chưa thực hiện và xác minh.**
7. Nếu môi trường Claude Code **không chạy Unity Editor được**, vẫn tạo code + Editor tooling/setup instructions, nhưng **không khẳng định build/Play Mode đã thành công**. Cung cấp checklist để con người xác minh.
8. Dùng tiếng Việt trong giải thích và commit/tiến độ (tên class/file C# có thể tiếng Anh).

### Tiêu chí bàn giao cuối

- Có **một scene chơi thử** (hoặc menu Editor tạo scene tự động có thể chạy) với bể, quái vật, UI và game loop.
- Kéo/thả → va chạm → merge → điểm → game over → restart hoạt động.
- Boing/Goo/Crag cho cảm nhận vật lý khác nhau; có năng lượng và Magni Pull hoạt động theo luật.
- Chạy được trong Unity Editor; chỉ kết luận chạy Android sau khi **đã build và test trên thiết bị**.
- Không có lỗi compile; merge không tạo quái vật trùng/lặp hay `MissingReferenceException`.
- Có hướng dẫn `README` chạy thử và bảng test thủ công.

---

## 1. Đề xuất cấu trúc file

```text
Assets/
└── MonsterMerge/
    ├── Scenes/
    │   └── GameScene.unity
    ├── Scripts/
    │   ├── Core/
    │   │   ├── GameManager.cs
    │   │   └── GameState.cs               (chỉ nếu cần)
    │   ├── Monsters/
    │   │   ├── Monster.cs
    │   │   ├── MonsterData.cs
    │   │   └── MergeManager.cs
    │   ├── Gameplay/
    │   │   ├── DropController.cs
    │   │   ├── AbilityManager.cs
    │   │   └── DangerZone.cs
    │   ├── UI/
    │   │   └── GameUI.cs
    │   └── Editor/
    │       └── MonsterMergeSceneBuilder.cs (nếu cần)
    ├── Prefabs/
    │   └── Monster.prefab
    ├── Data/
    │   ├── MonsterTier1.asset
    │   ├── MonsterTier2.asset
    │   ├── MonsterTier3.asset
    │   ├── MonsterTier4.asset
    │   └── MonsterTier5.asset
    ├── PhysicsMaterials/             (nếu dùng asset riêng)
    ├── Art/                          (placeholder shapes/colors)
    └── README_SETUP.md
```

**Lưu ý về asset Unity:** các file `.unity`, `.prefab`, `.asset` phải hợp lệ theo serialization Unity và có tham chiếu đúng; **không bịa GUID, không tự viết YAML nửa vời**. Nếu Claude Code không tạo được bằng Editor API, hãy tạo **Editor Menu `Tools/Monster Merge/Build Prototype Scene`** để Unity Editor tạo scene, prefab, ScriptableObject và dây tham chiếu bằng code Editor (phải đặt trong `Editor/`), hoặc cung cấp thao tác Inspector ngắn gọn để hoàn tất. Không coi chỉ có script là hoàn thành scene.

---

## 2. Milestone P0 — Core loop bắt buộc (ưu tiên tuần đầu)

### P0.1 — Kiểm tra project và dựng nền

- [ ] Kiểm tra project, Unity version, Input System, scene và script hiện có. Ghi tóm tắt vào `Assets/MonsterMerge/README_SETUP.md`.
- [ ] Tạo thư mục tối thiểu, scene `GameScene`, camera orthographic, background màu đơn giản.
- [ ] Cấu hình màn hình dọc: game view tham chiếu 9:16, UI Canvas Scaler và anchor hợp lý; không hardcode vị trí pixel cho mọi thiết bị.
- [ ] Tạo bể từ `LeftWall`, `RightWall`, `Floor` dùng static `BoxCollider2D` (không đóng kín phần miệng).
- [ ] Tạo `DangerLine` nhìn thấy được, dùng logic riêng để tính tràn.
- [ ] Phân tách layer UI, gameplay, walls nếu cần; các vật lý quái phải có collision với nhau và với tường.

**Acceptance:** Vào Play Mode, camera thấy trọn bể và HUD placeholder, vật lý quái không xuyên đáy/tường.

### P0.2 — Cấu hình cấp quái + prefab

- [ ] Viết `MonsterData : ScriptableObject` gồm: `tier`, `displayName`, `sprite` (optional), `color`, `radius`, `mass`, `friction`, `bounciness`, `mergeScore`.
- [ ] Tạo 5 dữ liệu cấp từ GDD; tham số cân bằng phải sửa được trong Inspector.
- [ ] Tạo **một prefab Monster** chứa `Rigidbody2D (Dynamic)`, `CircleCollider2D`, `SpriteRenderer`, `Monster`.
- [ ] `Monster.Initialize(data)` cấu hình scale/collider radius/mass/material, sprite/color, layer; không giả định prefab scale luôn là (1,1,1) nếu việc scale ảnh hưởng radius.
- [ ] `Monster` có `IsMerging`, cờ `HasBeenDropped` (để exclude preview), và cách tra cứu tier/data an toàn.
- [ ] Collider phải dùng bán kính thế giới **nhất quán**: chọn **một chiến lược duy nhất** (scale transform với collider radius cố định, **hoặc** transform scale cố định và collider.radius theo dữ liệu). Không áp cả hai khiến kích thước bị nhân đôi.

**Acceptance:** Spawn thử 5 cấp trong Editor; cả 5 có màu/kích thước đúng, mass/bounce/friction đọc đúng và không bị biến dạng collider.

### P0.3 — Thả quái (mouse + touch)

- [ ] Viết `DropController`: current tier, next tier, preview ở vị trí trên bể, điều khiển bằng chuột/touch.
- [ ] Kéo ngang để chọn X; clamp theo biên trong bể **trừ bán kính quái**.
- [ ] Nhả ngón/chuột để spawn vật lý vào bể; preview không va chạm/không tính danger.
- [ ] Chỉ cho thả trong trạng thái `Playing`, không đang `AbilityTargeting`, không khi thao tác trên UI.
- [ ] Có cooldown thả (gợi ý 0.4–0.6s). Tier spawn mặc định 1/2 theo trọng số 80/20, dễ cấu hình.
- [ ] Hiển thị current/next tier đơn giản (sprite/color hoặc text) và luôn cập nhật đúng thứ tự.
- [ ] Có thể thao tác hoàn toàn bằng chuột trong Editor, touch trên thiết bị.

**Acceptance:** Thả liên tục 20 lần, không spawn ngoài bể, không nhận tap lên UI như lệnh thả, không spawn 2 lần cho một lần release.

### P0.4 — Hợp nhất an toàn

- [ ] Viết `MergeManager` xử lý cặp cùng tier, trạng thái `!IsMerging`, và giới hạn cấp tối đa.
- [ ] **Khóa cả hai Monster trước khi enqueue**. Nên xử lý hàng đợi trong pha phù hợp vòng đời physics, tránh `Destroy/Instantiate` lặp trong callback collision.
- [ ] Tạo tier+1 tại gần trung điểm, có biện pháp tránh spawn đè mạnh làm bắn vật thể không kiểm soát; giữ collision vật lý hợp lý.
- [ ] Không merge cặp khác tier; không merge cặp tier 5 trong prototype.
- [ ] Cộng điểm chỉ **1 lần mỗi cặp merge**; cập nhật best score.
- [ ] Không bị double merge từ `OnCollisionEnter2D`/`OnCollisionStay2D`; không ném lỗi khi object bị Destroy trong cùng frame.
- [ ] Hỗ trợ chain merge: quái mới có thể tiếp tục merge ở bước physics tiếp theo.

**Acceptance:** Tạo nhiều cặp cùng cấp (kể cả 3–4 quái chạm nhau gần đồng thời) không bị nhân bản, không double score, không `MissingReferenceException`.

### P0.5 — Điểm, tràn bể, game over, restart

- [ ] `GameManager` quản lý `Playing/GameOver`, score, best score (`PlayerPrefs`).
- [ ] `GameUI` hiển thị Score/Best/Next; panel Game Over với nút Restart.
- [ ] `DangerZone` kiểm tra quái **đã thả** ở trên line liên tục ~2 giây; reset timer khi hết nguy hiểm.
- [ ] Không game over do preview hoặc một lần quái vừa nảy qua vạch trong thời gian ngắn.
- [ ] Restart reset scene hoặc runtime objects/timers/energy/combo, không giữ reference cũ; giữ best score.

**Acceptance:** Có thể chơi một ván hoàn chỉnh → Game Over → Restart → chơi tiếp ít nhất 3 lần không lỗi, Best Score được lưu khi đóng/mở game.

### CHECKPOINT P0

- [ ] Người dùng có thể chơi bằng **5 hình tròn**, thả, merge, đạt điểm, thua, restart.
- [ ] Không bắt buộc kỹ năng nam châm ở checkpoint này; **không làm art/ads trước P0**.
- [ ] Nếu không thể chạy Editor trong môi trường Claude Code, lập file test thủ công + ghi rõ trạng thái *chưa kiểm chứng trên Unity*.

---

## 3. Milestone P1 — Điểm khác biệt Monster Merge

### P1.1 — Vật lý theo cấp

- [ ] Đảm bảo **Boing (tier 2) nảy rõ** nhưng không tự bắn văng vô lý.
- [ ] **Goo (tier 3) có ma sát lớn** nhưng không tạo joint/soft-body.
- [ ] **Crag (tier 5) nặng, nảy thấp**, không có attack hay slam.
- [ ] Giới hạn vận tốc/lực nếu cần, có thể chỉnh theo inspector; kiểm tra không xuyên tường ở tốc độ cao (điều chỉnh collision detection phù hợp).
- [ ] Playtest đối chiếu với tất cả quái cùng PhysicsMaterial2D: sự khác biệt phải dễ cảm nhận **và** không gây khó chịu.

**Acceptance:** Thả cùng kịch bản và thấy phản ứng khác nhau, không phá game.

### P1.2 — Năng lượng hợp nhất

- [ ] `AbilityManager` có `Energy` [0..100], gợi ý +25 mỗi merge hợp lệ.
- [ ] UI năng lượng cập nhật theo sự kiện merge; tối đa 100, không vượt giá trị.
- [ ] Nút `Activate Magnet` **disabled** khi chưa đủ năng lượng hoặc game over.
- [ ] Khi đầy năng lượng: bật chế độ **Targeting** tách biệt với `DropController`, có nút Cancel hoặc chạm Activate lần nữa để hủy.

**Acceptance:** 4 merge hợp lệ từ 0 đạt 100; không trừ nếu hủy; không thả quái khi đang chọn mục tiêu.

### P1.3 — Magnetic Pull (kỹ năng DUY NHẤT)

- [ ] Chỉ chọn **Magni (tier 4)** đã được thả, chưa merge, còn tồn tại.
- [ ] Tìm **Magni khác gần nhất trong bán kính 2.5 units** (tham số cấu hình); loại trừ chính nó, preview, object đang merging/destroyed.
- [ ] Nếu không có mục tiêu: phản hồi UI nhỏ (`Không có Magni gần đó`) và **giữ nguyên năng lượng**.
- [ ] Nếu hợp lệ: trừ 100 năng lượng và tạo lực hút hai Magni tiến lại gần trong khoảng 0.6 giây; giới hạn lực/vận tốc để không xuyên vật cản.
- [ ] Dừng coroutine/physics effect an toàn khi quái bị merge/hủy hoặc Game Over/Restart.
- [ ] Nếu chạm UI khi targeting, không chọn nhầm world object; nhấn Cancel thoát chế độ.
- [ ] Cặp Magni vẫn hợp nhất bằng **logic chuẩn của MergeManager** khi va chạm; kỹ năng không tạo merge cưỡng bức.

**Acceptance:** Với 2 Magni gần nhau, dùng kỹ năng gây dịch chuyển rõ, có thể tạo merge; với 1 Magni không tốn năng lượng; restart trong khi hút không lỗi.

### P1.4 — Phản hồi chơi thử

- [ ] Hiệu ứng merge tối giản (pop/flash/text score), không dùng package VFX ngoài.
- [ ] Hiển thị combo nếu dễ làm; combo không cần hệ số điểm nếu tăng complexity.
- [ ] Có chỉ dẫn lần đầu: "Kéo để thả · 2 quái cùng cấp sẽ hợp nhất · Đầy năng lượng để dùng nam châm".
- [ ] Đảm bảo UI không che bể và nút kỹ năng không đụng khu vực thả.

**Acceptance:** Người chưa biết game nhìn HUD và thử 1 phút có thể hiểu luật cơ bản.

### CHECKPOINT P1

- [ ] Core loop **Drop → Merge → Energy → Magnet → Chain Merge** có thể trải nghiệm.
- [ ] Thử chơi liên tục 10–15 phút trong Editor, không lỗi logic làm kẹt/restart, không log lỗi runtime.

---

## 4. Milestone P2 — Đóng gói bản chơi thử Android

- [ ] Xác minh Android Build Support/SDK/JDK/NDK có sẵn; nếu thiếu, **ghi cách cài**, không tự báo build thành công.
- [ ] Thiết lập Android portrait, safe area và Canvas Scaler phù hợp thiết bị tỉ lệ khác nhau.
- [ ] Build dev APK nếu môi trường cho phép (không cần ký release/Google Play ở prototype).
- [ ] Test cảm ứng trên ít nhất **1 máy Android thực**, ưu tiên thêm màn nhỏ/dài nếu có.
- [ ] Kiểm tra: thả bằng 1 ngón, không chạm nhầm UI, frame rate ổn định, pause/resume app không gây double input, danger ổn định.
- [ ] Không tích hợp AdMob ở giai đoạn này.
- [ ] Cập nhật `README_SETUP.md`: cách chạy, build, phím/nút, known issues và video/screenshot nếu có thể.

**Acceptance:** Bản build chơi thử ổn định trên thiết bị được kiểm tra; ghi rõ tên thiết bị/Android version và kết quả thật, không suy diễn.

---

## 5. Thiết kế kỹ thuật và hợp đồng class (gợi ý)

| Thành phần | Trách nhiệm | Không nên làm |
|---|---|---|
| `MonsterData` | Dữ liệu từng tier, physics, điểm | Giữ state runtime |
| `Monster` | Rigidbody, tier, cờ merging/dropped, báo va chạm | Tạo score, update UI, spawn cặp mới trực tiếp |
| `MergeManager` | Duy nhất quyết định cặp hợp lệ và sinh tier mới | Nhận input người chơi |
| `DropController` | Preview/current/next, drag/drop, cooldown | Tính điểm/merge |
| `GameManager` | State, score, best, restart | Xử lý lực hút chi tiết |
| `AbilityManager` | Energy, target selection, Magnetic Pull | Thay đổi luật merge |
| `DangerZone` | Vùng tràn + timer | Dựa vào trigger preview |
| `GameUI` | UI binding, button handlers | Chứa physics/gameplay logic |

### Nguyên tắc code

- Ưu tiên `SerializeField`, inspector validation, null checks hữu ích, và log lỗi rõ khi thiếu tham chiếu.
- Event chỉ khi giúp giảm coupling (ví dụ `OnMerged`, `OnScoreChanged`, `OnEnergyChanged`, `OnGameOver`). Unsubscribe sự kiện đúng lifecycle.
- Hạn chế `FindObjectOfType`/`GameObject.Find` trong `Update`; nếu cần resolve tham chiếu hãy làm lúc setup.
- Không dùng string name/GameObject tag để xác định tier; dùng dữ liệu/script.
- Không gọi `Destroy` lặp cùng Monster; khi merge cần khóa trước.
- Không phụ thuộc vào asset store hoặc các class không tồn tại trong project.
- Xử lý API khác phiên bản Unity dựa trên **phiên bản thực tế đã đọc** (Rigidbody2D `velocity`/`linearVelocity`, Input API, v.v.).
- Ưu tiên giữ được tính testability; code không bám cứng vào UI object name.

---

## 6. Danh sách test bắt buộc

| ID | Tình huống | Kết quả mong đợi |
|---|---|---|
| T01 | Thả 1 con ở sát thành trái/phải | Không xuyên hoặc spawn ngoài bể |
| T02 | Nhả chuột/touch 1 lần | Chỉ 1 quái vật rơi |
| T03 | Thả 2 quái cùng tier chạm nhau | 1 quái tier+1; điểm tăng đúng 1 lần |
| T04 | 3 quái cùng tier chạm gần đồng thời | Không nhân bản, không 2 lần tính điểm cùng cặp |
| T05 | 2 quái khác tier chạm nhau | Không hợp nhất |
| T06 | 2 Crag (tier5) chạm nhau | Không merge ở v0.1; không lỗi |
| T07 | Quái Boing nảy ngang qua danger line trong <2 giây | Không game over |
| T08 | Bể thực sự đầy trên line hơn 2 giây | Game Over chỉ một lần |
| T09 | Restart 3 lần liên tục | Không còn old objects/timers/reference; Best giữ nguyên |
| T10 | Chạm UI trong lúc kéo/thả | UI không gây drop ngoài ý muốn |
| T11 | 4 lần merge từ energy 0 | Energy đạt 100 (không vượt) |
| T12 | Chọn Magni khi chỉ có 1 con | Không dùng kỹ năng, không trừ energy |
| T13 | Dùng Magnet khi có 2 Magni gần nhau | Hút hợp lệ, có thể merge, trừ 100 energy |
| T14 | Cancel target mode | Không mất energy; quay lại drop bình thường |
| T15 | Restart lúc kỹ năng đang hút | Không MissingReference/Coroutine error |
| T16 | Thua lúc đang ở target mode | Không thể tiếp tục thả/kích hoạt |
| T17 | Chạy Android, touch một ngón | Drop/activate hoạt động, UI đúng tỷ lệ |
| T18 | Đóng/mở app | Best Score vẫn còn |

> Claude Code phải phân biệt **test đã chạy thực tế**, **test code có thể tự động chạy**, và **test chỉ hướng dẫn người dùng chạy thủ công**.

---

## 7. Thứ tự làm tối ưu cho 5–10 giờ/tuần

| Đợt | Dự kiến | Nhiệm vụ | Điều kiện qua đợt |
|---|---:|---|---|
| 1 | 2–3 giờ | P0.1 + P0.2 | Scene, bể, Monster prefab và 5 tier hợp lệ |
| 2 | 2–3 giờ | P0.3 | Chơi được thao tác thả |
| 3 | 3–4 giờ | P0.4 + P0.5 | Merge, Score, Game Over, Restart |
| 4 | 3–5 giờ | P1.1 + P1.2 | Physics khác biệt, thanh năng lượng |
| 5 | 3–5 giờ | P1.3 + P1.4 | Magnet hoạt động, có feedback |
| 6 | 4–8 giờ | P2 + bugfix | APK test được nếu có môi trường |

**Ước lượng chỉ để lập kế hoạch**, không phải thời gian cam kết. Cần dành thêm buffer cho bug vật lý/serialization. Nếu quá tải, **cắt polish trước**, không cắt logic merge/game over.

---

## 8. Quy trình làm việc đề xuất cho Claude Code

Sau mỗi đợt triển khai, hãy trả lời theo mẫu:

```markdown
## Milestone: [P0.1 ...]
- Đã thực hiện: [các tính năng]
- File đã tạo/sửa: [đường dẫn]
- Cách kiểm thử trong Unity: [các bước chính xác]
- Đã kiểm chứng thực tế: [đã chạy / chưa chạy, vì sao]
- Known issues: [nếu có]
- Các ô task được đổi sang [x]: [mã task]
- Bước đề xuất tiếp theo: [duy nhất milestone kế tiếp]
```

**Quan trọng:** Không tự đánh dấu toàn bộ checklist `[x]` khi chỉ viết script. Setup Scene/Prefab/Inspector phải tồn tại hoặc có công cụ tạo tự động đã được thử trong Unity. Nếu thiếu môi trường Editor để xác minh, ghi `[ ]` và báo chặn rõ ràng.

---

## 9. Prompt gửi Claude Code (copy nguyên khối)

```text
Bạn hãy đọc kỹ hai file:
1) MONSTER_MERGE_GDD_v0.1.md
2) MONSTER_MERGE_CLAUDE_CODE_TASKS_v0.1.md

Bạn là Unity 2D C# developer triển khai prototype Monster Merge: Chaos Lab.

Hãy bắt đầu bằng khảo sát repo hiện tại, Unity version, Input System và các scene/script có sẵn. Không tự ý ghi đè code hiện có. Làm theo checklist, bắt đầu P0.1–P0.2 rồi tiếp tục từng milestone khi có thể. Ưu tiên sản phẩm chạy được trong Unity Editor, không thêm bất kỳ combat/AI/ads/shop nào. Nếu cần tạo prefab/scene/ScriptableObject từ code, dùng Editor tooling đúng chuẩn Unity (không bịa GUID/YAML).

Bạn có thể triển khai tiếp các milestone trong cùng phiên làm việc nếu đã có thể kiểm chứng milestone trước; đừng đánh dấu hoàn tất khi chưa có bằng chứng. Kết thúc, báo cáo đường dẫn file, các bước test cụ thể, trạng thái kiểm chứng, lỗi còn tồn tại và cập nhật checklist trung thực. Đừng tuyên bố đã build Android khi chưa build trên môi trường thực tế.
```

---

## 10. Ghi chú cuối cho team 1 người

**Câu hỏi duy nhất cần trả lời sau prototype:** _Các đặc tính vật lý + một kỹ năng Magni có tạo ra trải nghiệm vui và đáng chơi lại hơn một bản Suika đổi skin không?_  
Nếu **chưa**, sửa cơ chế và cân bằng trước. **Không cứu gameplay bằng cách thêm chiến đấu hay 20 kỹ năng.**
