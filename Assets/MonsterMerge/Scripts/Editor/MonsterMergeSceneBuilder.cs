using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMerge.EditorTools
{
    /// <summary>
    /// Tạo toàn bộ asset của prototype bằng Editor API. Chạy lại an toàn: dữ liệu MonsterData và
    /// sprite đã có được giữ nguyên để không mất thông số đã cân bằng; prefab và scene được dựng lại.
    /// </summary>
    public static class MonsterMergeSceneBuilder
    {
        private const string Root = "Assets/MonsterMerge";
        private const string ScenePath = Root + "/Scenes/GameScene.unity";
        private const string PrefabPath = Root + "/Prefabs/Monster.prefab";
        private const string CirclePath = Root + "/Art/Circle.png";
        private const string SquarePath = Root + "/Art/Square.png";
        private const string UnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        private const float WallThickness = 0.2f;
        private const float WallVisualTopY = 3.3f;
        private const float WallColliderTopY = 12f;

        // Không có trong bảng GDD: đo thực tế cho thấy friction một mình không làm Goo khó lăn.
        private const float GooRollResistance = 6f;

        private const int MonsterSortingOrder = 5;
        private const int LabelSortingOrder = 6;

        private static readonly Color Background = new(0.13f, 0.15f, 0.22f);
        private static readonly Color TankBack = new(0.19f, 0.22f, 0.31f);
        private static readonly Color TankWall = new(0.62f, 0.68f, 0.8f);
        private static readonly Color PanelColor = new(0.1f, 0.11f, 0.17f, 0.85f);
        private static readonly Color AccentColor = new(0.56f, 0.36f, 0.95f);

        private readonly struct TierSpec
        {
            public readonly string Name;
            public readonly Color Color;
            public readonly float Radius;
            public readonly float Mass;
            public readonly float Bounce;
            public readonly float Friction;
            public readonly float RollResistance;
            public readonly int Score;

            public TierSpec(string name, Color color, float radius, float mass, float bounce, float friction, float rollResistance, int score)
            {
                RollResistance = rollResistance;
                Name = name;
                Color = color;
                Radius = radius;
                Mass = mass;
                Bounce = bounce;
                Friction = friction;
                Score = score;
            }
        }

        // Giá trị khởi đầu từ GDD mục 3.1 và 2.4. Chưa cân bằng.
        private static readonly TierSpec[] Specs =
        {
            new("Blip", new Color(0.25f, 0.6f, 1f), 0.22f, 0.6f, 0.05f, 0.4f, 0.05f, 0),
            new("Boing", new Color(1f, 0.82f, 0.2f), 0.29f, 0.8f, 0.65f, 0.2f, 0.05f, 10),
            new("Goo", new Color(0.3f, 0.82f, 0.4f), 0.37f, 1f, 0.03f, 1f, GooRollResistance, 25),
            new("Magni", new Color(0.66f, 0.38f, 0.95f), 0.47f, 1.1f, 0.12f, 0.5f, 0.05f, 60),
            new("Crag", new Color(0.85f, 0.5f, 0.22f), 0.6f, 2.2f, 0f, 0.8f, 0.05f, 150),
        };

        [MenuItem("Tools/Monster Merge/Build Prototype Scene")]
        private static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Monster Merge",
                    "GameScene và Monster.prefab sẽ được dựng lại; thay đổi tay trong hai file đó sẽ mất.\nMonsterData không bị ảnh hưởng.",
                    "Dựng lại", "Hủy"))
            {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Build();
            }
        }

        /// <summary>Game View dẹt làm nút UI đè lên bể; luôn chơi thử ở tỉ lệ dọc của thiết bị.</summary>
        [MenuItem("Tools/Monster Merge/Set Game View 9-16")]
        public static void SetPortraitGameView()
        {
            PlayModeWindow.SetCustomRenderingResolution(1080, 1920, "Monster Merge 9:16");
        }

        public static void Build()
        {
            EnsureFolders();

            // Phải tạo scene TRƯỚC khi nạp asset: NewScene(Single) dọn các object chưa được tham chiếu,
            // biến C# đang giữ chúng sẽ thành null và scene lưu ra mất tham chiếu.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Sprite circle = EnsureSprite(CirclePath, CreateCircleTexture(256), 256);
            Sprite square = EnsureSprite(SquarePath, CreateSquareTexture(4), 4);
            Material unlit = AssetDatabase.LoadAssetAtPath<Material>(UnlitMaterialPath);
            if (unlit == null)
            {
                Debug.LogError($"[MonsterMerge] Không tìm thấy {UnlitMaterialPath}.");
                return;
            }

            MonsterData[] tiers = EnsureTierData();
            Monster prefab = BuildMonsterPrefab(circle, unlit);

            BuildScene(scene, tiers, prefab, circle, square, unlit);
            AddSceneToBuildSettings();

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            SetPortraitGameView();

            AssetDatabase.SaveAssets();
            Debug.Log("[MonsterMerge] Đã dựng xong GameScene.");
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { "Scenes", "Prefabs", "Data", "Art" })
            {
                string path = $"{Root}/{folder}";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder(Root, folder);
                }
            }
        }

        private static Texture2D CreateCircleTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                    // Viền tối nhẹ để hai quái cùng màu vẫn tách biệt khi nằm sát nhau.
                    float shade = distance > radius - size * 0.035f ? 0.72f : 1f;
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSquareTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels(Enumerable.Repeat(Color.white, size * size).ToArray());
            texture.Apply();
            return texture;
        }

        private static Sprite EnsureSprite(string path, Texture2D texture, int pixelsPerUnit)
        {
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Object.DestroyImmediate(texture);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static MonsterData[] EnsureTierData()
        {
            var result = new MonsterData[Specs.Length];

            for (int i = 0; i < Specs.Length; i++)
            {
                string path = $"{Root}/Data/MonsterTier{i + 1}.asset";
                MonsterData data = AssetDatabase.LoadAssetAtPath<MonsterData>(path);

                if (data == null)
                {
                    TierSpec spec = Specs[i];
                    data = ScriptableObject.CreateInstance<MonsterData>();
                    data.tier = i + 1;
                    data.displayName = spec.Name;
                    data.color = spec.Color;
                    data.radius = spec.Radius;
                    data.mass = spec.Mass;
                    data.bounciness = spec.Bounce;
                    data.friction = spec.Friction;
                    data.rollResistance = spec.RollResistance;
                    data.mergeScore = spec.Score;
                    AssetDatabase.CreateAsset(data, path);
                }

                result[i] = data;
            }

            return result;
        }

        private static (SpriteRenderer body, TextMesh label) CreateVisual(Transform parent, Sprite circle, Material unlit, int sortingOffset)
        {
            var bodyObject = new GameObject("Body");
            bodyObject.transform.SetParent(parent, false);
            var body = bodyObject.AddComponent<SpriteRenderer>();
            body.sprite = circle;
            body.sharedMaterial = unlit;
            body.sortingOrder = MonsterSortingOrder + sortingOffset;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font = font;
            label.fontSize = 64;
            label.characterSize = 0.1f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.1f, 0.1f, 0.15f);
            label.text = "1";

            var labelRenderer = labelObject.GetComponent<MeshRenderer>();
            labelRenderer.sharedMaterial = font.material;
            labelRenderer.sortingOrder = LabelSortingOrder + sortingOffset;

            return (body, label);
        }

        private static Monster BuildMonsterPrefab(Sprite circle, Material unlit)
        {
            var root = new GameObject("Monster");

            var rigidbody = root.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.sleepMode = RigidbodySleepMode2D.StartAwake;

            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            (SpriteRenderer body, TextMesh label) = CreateVisual(root.transform, circle, unlit, 0);

            var monster = root.AddComponent<Monster>();
            Wire(monster,
                ("body", body),
                ("label", label),
                ("rigidbody2d", rigidbody),
                ("circleCollider", collider));

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved.GetComponent<Monster>();
        }

        private static void BuildScene(Scene scene, MonsterData[] tiers, Monster prefab, Sprite circle, Sprite square, Material unlit)
        {
            Camera camera = BuildCamera();
            BuildEventSystem();

            var tankObject = new GameObject("Tank");
            var tank = tankObject.AddComponent<TankBounds>();
            SpriteRenderer dangerLine = BuildTank(tank, square, unlit);

            var monsters = new GameObject("Monsters");
            var managers = new GameObject("Managers");
            var game = managers.AddComponent<GameManager>();
            var merge = managers.AddComponent<MergeManager>();
            var ability = managers.AddComponent<AbilityManager>();
            var drop = managers.AddComponent<DropController>();
            var danger = managers.AddComponent<DangerZone>();
            var effects = managers.AddComponent<MergeEffects>();

            var preview = new GameObject("Preview");
            (SpriteRenderer previewBody, TextMesh previewLabel) = CreateVisual(preview.transform, circle, unlit, 2);

            Wire(game, ("merge", merge));

            Wire(merge,
                ("monsterPrefab", prefab),
                ("monsterRoot", monsters.transform),
                ("tank", tank),
                ("game", game));
            SetObjectArray(merge, "tiers", tiers);

            Wire(ability,
                ("game", game),
                ("merge", merge),
                ("worldCamera", camera));

            Wire(drop,
                ("game", game),
                ("merge", merge),
                ("ability", ability),
                ("tank", tank),
                ("worldCamera", camera),
                ("previewRoot", preview.transform),
                ("previewBody", previewBody),
                ("previewLabel", previewLabel));
            SetSpawnTable(drop, (tiers[0], 80f), (tiers[1], 20f));

            Wire(danger,
                ("game", game),
                ("merge", merge),
                ("tank", tank),
                ("lineRenderer", dangerLine));

            Wire(effects,
                ("merge", merge),
                ("ringSprite", circle),
                ("material", unlit));

            BuildUi(game, merge, drop, ability, camera, circle);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static Camera BuildCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;

            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<PortraitCameraFit>();
            return camera;
        }

        private static void BuildEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static SpriteRenderer BuildTank(TankBounds tank, Sprite square, Material unlit)
        {
            float left = tank.InnerLeft;
            float right = tank.InnerRight;
            float floor = tank.FloorY;
            float width = right - left;
            float centerX = (left + right) * 0.5f;
            float visualHeight = WallVisualTopY - floor;

            // Vách sau chỉ để nhìn.
            CreateSprite("Backdrop", tank.transform, square, unlit, TankBack, -1,
                new Vector2(centerX, floor + visualHeight * 0.5f), new Vector2(width, visualHeight));

            // Collider tường cao hơn phần nhìn thấy để Boing nảy mạnh không vọt ra ngoài; miệng bể vẫn mở.
            float colliderHeight = WallColliderTopY - floor;
            float colliderCenterY = floor + colliderHeight * 0.5f;
            float wallVisualCenterY = floor + visualHeight * 0.5f;
            const float ColliderThickness = 1f;

            CreateWall("LeftWall", tank.transform, square, unlit,
                new Vector2(left - ColliderThickness * 0.5f, colliderCenterY), new Vector2(ColliderThickness, colliderHeight),
                new Vector2(left - WallThickness * 0.5f, wallVisualCenterY), new Vector2(WallThickness, visualHeight));

            CreateWall("RightWall", tank.transform, square, unlit,
                new Vector2(right + ColliderThickness * 0.5f, colliderCenterY), new Vector2(ColliderThickness, colliderHeight),
                new Vector2(right + WallThickness * 0.5f, wallVisualCenterY), new Vector2(WallThickness, visualHeight));

            CreateWall("Floor", tank.transform, square, unlit,
                new Vector2(centerX, floor - ColliderThickness * 0.5f), new Vector2(width + ColliderThickness * 2f, ColliderThickness),
                new Vector2(centerX, floor - WallThickness * 0.5f), new Vector2(width + WallThickness * 2f, WallThickness));

            return CreateSprite("DangerLine", tank.transform, square, unlit, new Color(1f, 0.45f, 0.4f, 0.55f), 4,
                new Vector2(centerX, tank.DangerY), new Vector2(width, 0.04f));
        }

        private static void CreateWall(string name, Transform parent, Sprite square, Material unlit,
            Vector2 colliderCenter, Vector2 colliderSize, Vector2 visualCenter, Vector2 visualSize)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.position = colliderCenter;

            var box = wall.AddComponent<BoxCollider2D>();
            box.size = colliderSize;

            CreateSprite("Visual", wall.transform, square, unlit, TankWall, 1, visualCenter, visualSize);
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, Sprite sprite, Material material,
            Color color, int sortingOrder, Vector2 center, Vector2 size)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = center;
            item.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static void BuildUi(GameManager game, MergeManager merge, DropController drop, AbilityManager ability, Camera camera, Sprite circle)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasObject = new GameObject("Canvas") { layer = LayerMask.NameToLayer("UI") };
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform safeArea = CreateRect("SafeArea", canvasObject.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            // HUD trên
            Text score = CreateText("ScoreText", safeArea, font, "Điểm: 0", 52, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(480f, 80f), new Vector2(0f, 1f));
            Text best = CreateText("BestText", safeArea, font, "Kỷ lục: 0", 52, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(480f, 80f), new Vector2(1f, 1f));

            CreateText("NextLabel", safeArea, font, "Tiếp theo", 36, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(40f, -120f), new Vector2(180f, 70f), new Vector2(0f, 1f));

            Image nextIcon = CreateImage("NextIcon", safeArea, Color.white, false);
            nextIcon.sprite = circle;
            Place(nextIcon.rectTransform, new Vector2(0f, 1f), new Vector2(225f, -120f), new Vector2(70f, 70f), new Vector2(0f, 1f));

            Text nextTier = CreateText("NextTierText", nextIcon.rectTransform, font, "1", 40, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 70f), new Vector2(0.5f, 0.5f));
            nextTier.color = new Color(0.1f, 0.1f, 0.15f);

            Text combo = CreateText("ComboText", safeArea, font, string.Empty, 46, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(-40f, -120f), new Vector2(400f, 70f), new Vector2(1f, 1f));
            combo.color = new Color(1f, 0.85f, 0.3f);

            // Đặt ở dải trống giữa HUD và miệng bể; vị trí cũ dưới đáy bể làm chữ đè lên bể (v0.2 TASK-02).
            Text message = CreateText("MessageText", safeArea, font, string.Empty, 42, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(1000f, 120f), new Vector2(0.5f, 1f));
            message.color = new Color(1f, 0.9f, 0.55f);

            // Năng lượng và nút kỹ năng ở dưới, ngoài vùng bể
            Image energyBack = CreateImage("EnergyBar", safeArea, new Color(0.05f, 0.06f, 0.1f), false);
            Place(energyBack.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(860f, 56f), new Vector2(0.5f, 0f));

            Image energyFill = CreateImage("EnergyFill", energyBack.rectTransform, AccentColor, false);
            RectTransform fillRect = energyFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Text energyText = CreateText("EnergyText", energyBack.rectTransform, font, "Năng lượng 0%", 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 56f), new Vector2(0.5f, 0.5f));

            (Button magnetButton, Text magnetText) = CreateButton("MagnetButton", safeArea, font, "Kích hoạt Nam châm", 42, AccentColor,
                new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(860f, 120f), new Vector2(0.5f, 0f));

            // Chỉ dẫn lần đầu: không chặn raycast để vẫn kéo thả được xuyên qua
            Image hint = CreateImage("HintPanel", safeArea, PanelColor, false);
            Place(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(860f, 300f), new Vector2(0.5f, 0.5f));
            Text hintText = CreateText("HintText", hint.rectTransform, font,
                "Kéo để chọn vị trí, nhả để thả\n2 quái cùng cấp sẽ hợp nhất\nĐầy năng lượng để dùng nam châm", 42, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 280f), new Vector2(0.5f, 0.5f));
            hintText.lineSpacing = 1.2f;

            RectTransform floating = CreateRect("FloatingRoot", safeArea);
            Stretch(floating);

            // Game Over: panel phủ toàn màn hình, chặn mọi chạm phía sau
            Image gameOver = CreateImage("GameOverPanel", canvasObject.transform, new Color(0.05f, 0.05f, 0.1f, 0.82f), true);
            Stretch(gameOver.rectTransform);

            CreateText("GameOverTitle", gameOver.rectTransform, font, "Bể đã tràn!", 96, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1000f, 140f), new Vector2(0.5f, 0.5f));
            Text finalScore = CreateText("FinalScoreText", gameOver.rectTransform, font, "Điểm: 0", 60, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(1000f, 90f), new Vector2(0.5f, 0.5f));
            Text finalBest = CreateText("FinalBestText", gameOver.rectTransform, font, "Kỷ lục: 0", 60, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1000f, 90f), new Vector2(0.5f, 0.5f));
            (Button restart, _) = CreateButton("RestartButton", gameOver.rectTransform, font, "Chơi lại", 56, AccentColor,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(560f, 140f), new Vector2(0.5f, 0.5f));

            var ui = canvasObject.AddComponent<GameUI>();
            Wire(ui,
                ("game", game),
                ("merge", merge),
                ("drop", drop),
                ("ability", ability),
                ("worldCamera", camera),
                ("scoreText", score),
                ("bestText", best),
                ("nextIcon", nextIcon),
                ("nextTierText", nextTier),
                ("comboText", combo),
                ("messageText", message),
                ("hintPanel", hint.gameObject),
                ("floatingRoot", floating),
                ("energyFill", fillRect),
                ("energyText", energyText),
                ("magnetButton", magnetButton),
                ("magnetButtonText", magnetText),
                ("gameOverPanel", gameOver.gameObject),
                ("finalScoreText", finalScore),
                ("finalBestText", finalBest),
                ("restartButton", restart));
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var item = new GameObject(name, typeof(RectTransform)) { layer = LayerMask.NameToLayer("UI") };
            item.transform.SetParent(parent, false);
            return (RectTransform)item.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Image CreateImage(string name, Transform parent, Color color, bool raycastTarget)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static Text CreateText(string name, Transform parent, Font font, string value, int fontSize, TextAnchor alignment,
            Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            RectTransform rect = CreateRect(name, parent);
            Place(rect, anchor, position, size, pivot);

            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static (Button, Text) CreateButton(string name, Transform parent, Font font, string label, int fontSize, Color color,
            Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            Image image = CreateImage(name, parent, color, true);
            Place(image.rectTransform, anchor, position, size, pivot);

            var button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);
            button.colors = colors;

            Text text = CreateText("Label", image.rectTransform, font, label, fontSize, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size, new Vector2(0.5f, 0.5f));
            return (button, text);
        }

        private static void Wire(Object target, params (string field, Object value)[] references)
        {
            var serialized = new SerializedObject(target);
            foreach ((string field, Object value) in references)
            {
                SerializedProperty property = serialized.FindProperty(field);
                if (property == null)
                {
                    Debug.LogError($"[MonsterMerge] {target.GetType().Name} không có field '{field}'.");
                    continue;
                }

                RequireReference(value, target, field);
                property.objectReferenceValue = value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(Object target, string field, IReadOnlyList<Object> values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                RequireReference(values[i], target, $"{field}[{i}]");
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RequireReference(Object value, Object target, string field)
        {
            if (value == null)
            {
                Debug.LogError($"[MonsterMerge] Tham chiếu null khi gán {target.GetType().Name}.{field}.");
            }
        }

        private static void SetSpawnTable(DropController drop, params (MonsterData data, float weight)[] entries)
        {
            var serialized = new SerializedObject(drop);
            SerializedProperty table = serialized.FindProperty("spawnTable");
            table.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty entry = table.GetArrayElementAtIndex(i);
                RequireReference(entries[i].data, drop, $"spawnTable[{i}]");
                entry.FindPropertyRelative("data").objectReferenceValue = entries[i].data;
                entry.FindPropertyRelative("weight").floatValue = entries[i].weight;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(scene => scene.path != ScenePath)
                .ToList();

            // Đặt lên đầu để là scene khởi động; các scene sẵn có được giữ nguyên.
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
