#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Overun.Core;

namespace Overun.Editor
{
    /// <summary>
    /// Editor utility to auto-create common prefabs: VFX, Projectiles, UI elements.
    /// Access via menu: Overun → Create Prefab → [Type]
    /// </summary>
    public static class PrefabGenerator
    {
        private const string PREFAB_ROOT = "Assets/_Project/Prefabs";
        private const string VFX_PATH = PREFAB_ROOT + "/VFX";
        private const string WEAPONS_PATH = PREFAB_ROOT + "/Weapons";
        private const string UI_PATH = PREFAB_ROOT + "/UI";
        
        #region Menu Items
        
        [MenuItem("Overun/Create Prefab/VFX/Muzzle Flash")]
        public static void CreateMuzzleFlash()
        {
            CreateVFXPrefab("MuzzleFlash", CreateMuzzleFlashEffect);
        }
        
        [MenuItem("Overun/Create Prefab/VFX/Hit Effect")]
        public static void CreateHitEffect()
        {
            CreateVFXPrefab("HitEffect", CreateHitEffectParticle);
        }
        
        [MenuItem("Overun/Create Prefab/VFX/Bullet Trail")]
        public static void CreateBulletTrail()
        {
            CreateVFXPrefab("BulletTrail", CreateBulletTrailEffect);
        }
        
        [MenuItem("Overun/Create Prefab/Weapons/Projectile")]
        public static void CreateProjectile()
        {
            CreateWeaponPrefab("Projectile", CreateProjectilePrefab);
        }
        
        [MenuItem("Overun/Create Prefab/Player/ObjectPool Manager")]
        public static void CreateObjectPoolManager()
        {
            CreateObjectPoolInScene();
        }
        
        [MenuItem("Overun/Create Prefab/UI/Health Bar")]
        public static void CreateHealthBar()
        {
            CreateUIPrefab("HealthBar", CreateHealthBarUI);
        }
        
        [MenuItem("Overun/Create Prefab/UI/Stamina Bar")]
        public static void CreateStaminaBar()
        {
            CreateUIPrefab("StaminaBar", CreateStaminaBarUI);
        }
        
        [MenuItem("Overun/Create Prefab/UI/Crosshair")]
        public static void CreateCrosshair()
        {
            CreateUIPrefab("Crosshair", CreateCrosshairUI);
        }
        
        [MenuItem("Overun/Create Prefab/UI/Ammo Counter")]
        public static void CreateAmmoCounter()
        {
            CreateUIPrefab("AmmoCounter", CreateAmmoCounterUI);
        }
        
        [MenuItem("Overun/Setup Scene/Complete Player Setup")]
        public static void SetupCompletePlayer()
        {
            SetupPlayerInScene();
        }
        
        [MenuItem("Overun/Setup Scene/Create Game UI Canvas")]
        public static void CreateGameUICanvas()
        {
            SetupGameUI();
        }
        
        #endregion
        
        #region VFX Creators
        
        private static void CreateVFXPrefab(string name, System.Action<GameObject> setupAction)
        {
            EnsureDirectory(VFX_PATH);
            
            GameObject go = new GameObject(name);
            setupAction(go);
            
            string path = $"{VFX_PATH}/{name}.prefab";
            SavePrefab(go, path);
        }
        
        private static void CreateMuzzleFlashEffect(GameObject go)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.startLifetime = 0.05f;
            main.startSpeed = 0f;
            main.startSize = 0.3f;
            main.startColor = new Color(1f, 0.8f, 0.3f, 1f); // Orange-yellow
            main.maxParticles = 1;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
            
            var shape = ps.shape;
            shape.enabled = false;
            
            // Add light
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.7f, 0.3f);
            light.intensity = 2f;
            light.range = 3f;
            
            // Auto-disable script
            var autoDestroy = go.AddComponent<AutoDisableAfterTime>();
        }
        
        private static void CreateHitEffectParticle(GameObject go)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = 0.3f;
            main.startSpeed = 5f;
            main.startSize = 0.1f;
            main.startColor = Color.white;
            main.gravityModifier = 1f;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.1f;
            
            // Add auto-destroy
            var autoDestroy = go.AddComponent<AutoDisableAfterTime>();
        }
        
        private static void CreateBulletTrailEffect(GameObject go)
        {
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.1f;
            trail.startWidth = 0.05f;
            trail.endWidth = 0.01f;
            trail.startColor = new Color(1f, 0.9f, 0.5f, 1f);
            trail.endColor = new Color(1f, 0.9f, 0.5f, 0f);
            trail.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
        }
        
        #endregion
        
        #region Weapon Creators
        
        private static void CreateWeaponPrefab(string name, System.Action<GameObject> setupAction)
        {
            EnsureDirectory(WEAPONS_PATH);
            
            GameObject go = new GameObject(name);
            setupAction(go);
            
            string path = $"{WEAPONS_PATH}/{name}.prefab";
            SavePrefab(go, path);
        }
        
        private static void CreateProjectilePrefab(GameObject go)
        {
            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(go.transform);
            visual.transform.localScale = Vector3.one * 0.2f;
            
            // Remove default collider from visual
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            
            // Add components to root
            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            var collider = go.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            collider.isTrigger = true;
            
            // Add projectile script
            go.AddComponent<Overun.Weapons.Projectile>();
            
            // Add trail
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.05f;
            trail.endWidth = 0.01f;
            trail.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
        }
        
        #endregion
        
        #region UI Creators
        
        private static void CreateUIPrefab(string name, System.Action<GameObject> setupAction)
        {
            EnsureDirectory(UI_PATH);
            
            GameObject go = new GameObject(name);
            setupAction(go);
            
            string path = $"{UI_PATH}/{name}.prefab";
            SavePrefab(go, path);
        }
        
        private static void CreateHealthBarUI(GameObject go)
        {
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 20);
            
            // Background
            var bg = CreateUIImage(go.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 0.8f));
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            
            // Fill - set as Filled type for slider behavior
            var fill = CreateUIImage(go.transform, "Fill", new Color(0.2f, 0.8f, 0.3f, 1f));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            
            var fillImage = fill.GetComponent<UnityEngine.UI.Image>();
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;
        }
        
        private static void CreateCrosshairUI(GameObject go)
        {
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(20, 20);
            
            // Center dot
            var dot = CreateUIImage(go.transform, "Dot", Color.white);
            dot.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 4);
            
            // Lines
            CreateCrosshairLine(go.transform, "Top", new Vector2(0, 10), new Vector2(2, 8));
            CreateCrosshairLine(go.transform, "Bottom", new Vector2(0, -10), new Vector2(2, 8));
            CreateCrosshairLine(go.transform, "Left", new Vector2(-10, 0), new Vector2(8, 2));
            CreateCrosshairLine(go.transform, "Right", new Vector2(10, 0), new Vector2(8, 2));
        }
        
        private static void CreateCrosshairLine(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var line = CreateUIImage(parent, name, Color.white);
            var rect = line.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        
        private static void CreateAmmoCounterUI(GameObject go)
        {
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(150, 40);
            
            // Background
            var bg = CreateUIImage(go.transform, "Background", new Color(0, 0, 0, 0.5f));
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Text placeholder
            var textGo = new GameObject("AmmoText");
            textGo.transform.SetParent(go.transform);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
        }
        
        private static GameObject CreateUIImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<RectTransform>();
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            return go;
        }
        
        private static void CreateStaminaBarUI(GameObject go)
        {
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 12);
            
            // Background
            var bg = CreateUIImage(go.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 0.8f));
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            
            // Fill - blue color for stamina
            var fill = CreateUIImage(go.transform, "Fill", new Color(0.2f, 0.6f, 0.9f, 1f));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            
            // Set fill to horizontal fill
            var fillImage = fill.GetComponent<UnityEngine.UI.Image>();
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        }

        
        #endregion
        
        #region Scene Setup
        
        private static void CreateObjectPoolInScene()
        {
            // Check if already exists
            if (Object.FindObjectOfType<ObjectPool>() != null)
            {
                EditorUtility.DisplayDialog("ObjectPool Exists", 
                    "An ObjectPool already exists in the scene.", "OK");
                return;
            }
            
            var go = new GameObject("ObjectPool");
            var pool = go.AddComponent<ObjectPool>();
            
            Selection.activeGameObject = go;
            EditorUtility.DisplayDialog("ObjectPool Created", 
                "ObjectPool manager created. Add pools in the Inspector.", "OK");
        }
        
        private static void SetupPlayerInScene()
        {
            // Find or create player
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.tag = "Player";
            }
            
            // Ensure components
            if (!player.GetComponent<CharacterController>())
            {
                var cc = player.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0, 1, 0);
            }
            
            if (!player.GetComponent<Overun.Player.PlayerController>())
            {
                player.AddComponent<Overun.Player.PlayerController>();
            }
            
            if (!player.GetComponent<Overun.Player.PlayerHealth>())
            {
                player.AddComponent<Overun.Player.PlayerHealth>();
            }
            
            if (!player.GetComponent<Overun.Player.WeaponController>())
            {
                player.AddComponent<Overun.Player.WeaponController>();
            }
            
            // Create camera target
            var cameraTarget = player.transform.Find("CameraTarget");
            if (cameraTarget == null)
            {
                var targetGo = new GameObject("CameraTarget");
                targetGo.transform.SetParent(player.transform);
                targetGo.transform.localPosition = new Vector3(0, 1.6f, 0);
            }
            
            // Create muzzle point
            var muzzlePoint = player.transform.Find("MuzzlePoint");
            if (muzzlePoint == null)
            {
                var muzzleGo = new GameObject("MuzzlePoint");
                muzzleGo.transform.SetParent(player.transform);
                muzzleGo.transform.localPosition = new Vector3(0.3f, 1.2f, 0.5f);
            }
            
            Selection.activeGameObject = player;
            EditorUtility.DisplayDialog("Player Setup Complete", 
                "Player configured with:\n" +
                "- CharacterController\n" +
                "- PlayerController\n" +
                "- PlayerHealth\n" +
                "- WeaponController\n" +
                "- CameraTarget\n" +
                "- MuzzlePoint", "OK");
        }
        
        private static void SetupGameUI()
        {
            // Find or create Canvas
            var existingCanvas = Object.FindObjectOfType<UnityEngine.Canvas>();
            GameObject canvasGo;
            
            if (existingCanvas == null)
            {
                canvasGo = new GameObject("GameUI");
                var canvas = canvasGo.AddComponent<UnityEngine.Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            else
            {
                canvasGo = existingCanvas.gameObject;
            }
            
            // Create Health Bar
            var healthBarGo = new GameObject("HealthBar");
            healthBarGo.transform.SetParent(canvasGo.transform);
            var healthRect = healthBarGo.AddComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0, 1);
            healthRect.anchorMax = new Vector2(0, 1);
            healthRect.pivot = new Vector2(0, 1);
            healthRect.anchoredPosition = new Vector2(20, -20);
            healthRect.sizeDelta = new Vector2(200, 20);
            
            var healthBg = CreateUIImage(healthBarGo.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 0.8f));
            var healthBgRect = healthBg.GetComponent<RectTransform>();
            healthBgRect.anchorMin = Vector2.zero;
            healthBgRect.anchorMax = Vector2.one;
            healthBgRect.offsetMin = Vector2.zero;
            healthBgRect.offsetMax = Vector2.zero;
            
            var healthFill = CreateUIImage(healthBarGo.transform, "Fill", new Color(0.2f, 0.8f, 0.3f, 1f));
            var healthFillRect = healthFill.GetComponent<RectTransform>();
            healthFillRect.anchorMin = Vector2.zero;
            healthFillRect.anchorMax = Vector2.one;
            healthFillRect.offsetMin = new Vector2(2, 2);
            healthFillRect.offsetMax = new Vector2(-2, -2);
            var healthFillImg = healthFill.GetComponent<UnityEngine.UI.Image>();
            healthFillImg.type = UnityEngine.UI.Image.Type.Filled;
            healthFillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            
            healthBarGo.AddComponent<Overun.UI.HealthBarUI>();
            
            // Create Stamina Bar
            var staminaBarGo = new GameObject("StaminaBar");
            staminaBarGo.transform.SetParent(canvasGo.transform);
            var staminaRect = staminaBarGo.AddComponent<RectTransform>();
            staminaRect.anchorMin = new Vector2(0, 1);
            staminaRect.anchorMax = new Vector2(0, 1);
            staminaRect.pivot = new Vector2(0, 1);
            staminaRect.anchoredPosition = new Vector2(20, -45);
            staminaRect.sizeDelta = new Vector2(200, 12);
            
            var staminaBg = CreateUIImage(staminaBarGo.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 0.8f));
            var staminaBgRect = staminaBg.GetComponent<RectTransform>();
            staminaBgRect.anchorMin = Vector2.zero;
            staminaBgRect.anchorMax = Vector2.one;
            staminaBgRect.offsetMin = Vector2.zero;
            staminaBgRect.offsetMax = Vector2.zero;
            
            var staminaFill = CreateUIImage(staminaBarGo.transform, "Fill", new Color(0.2f, 0.6f, 0.9f, 1f));
            var staminaFillRect = staminaFill.GetComponent<RectTransform>();
            staminaFillRect.anchorMin = Vector2.zero;
            staminaFillRect.anchorMax = Vector2.one;
            staminaFillRect.offsetMin = new Vector2(2, 2);
            staminaFillRect.offsetMax = new Vector2(-2, -2);
            var staminaFillImg = staminaFill.GetComponent<UnityEngine.UI.Image>();
            staminaFillImg.type = UnityEngine.UI.Image.Type.Filled;
            staminaFillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            
            staminaBarGo.AddComponent<Overun.UI.StaminaBarUI>();
            
            // Create Crosshair
            var crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform);
            var crosshairRect = crosshairGo.AddComponent<RectTransform>();
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;
            crosshairRect.sizeDelta = new Vector2(20, 20);
            
            var dot = CreateUIImage(crosshairGo.transform, "Dot", Color.white);
            dot.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 4);
            
            Selection.activeGameObject = canvasGo;
            EditorUtility.DisplayDialog("Game UI Created", 
                "UI Canvas configured with:\n" +
                "- Health Bar (top-left)\n" +
                "- Stamina Bar (below health)\n" +
                "- Crosshair (center)", "OK");
        }
        
        #endregion
        
        #region Utilities
        
        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }
                    current = next;
                }
            }
        }
        
        private static void SavePrefab(GameObject go, string path)
        {
            // Check if exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                if (!EditorUtility.DisplayDialog("Prefab Exists", 
                    $"Prefab '{path}' already exists. Overwrite?", "Overwrite", "Cancel"))
                {
                    Object.DestroyImmediate(go);
                    return;
                }
            }
            
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            
            // Select the created prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"[PrefabGenerator] Created prefab: {path}");
        }
        
        #endregion
    }
}
#endif
