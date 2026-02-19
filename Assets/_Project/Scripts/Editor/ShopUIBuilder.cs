using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Overun.UI;
using Overun.Shop;

namespace Overun.Editor
{
    public class ShopUIBuilder : EditorWindow
    {
        [MenuItem("Overun/Build Shop UI")]
        public static void ShowWindow()
        {
            GetWindow<ShopUIBuilder>("Shop UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Shop UI Auto-Builder", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            GUILayout.Label("Story 6-5: Inventory Panel", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Build Inventory Panel & Tooltip"))
            {
                BuildUI();
            }
            
            EditorGUILayout.Space(10);
            GUILayout.Label("Story 6-6: Lock Mechanic", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Add Lock UI to ShopItems"))
            {
                AddLockUIToShopItems();
            }
        }

        private static void BuildUI()
        {
            ShopUI shopUI = FindObjectOfType<ShopUI>();
            if (shopUI == null)
            {
                // Try to find the canvas or create it?
                // For now, assume it exists as per user statement
                Debug.LogError("ShopUI component not found in scene! Please open the Shop scene or add ShopUI to your canvas.");
                return;
            }

            GameObject shopPanel = shopUI.transform.Find("ShopPanel")?.gameObject;
            if (shopPanel == null)
            {
                Debug.LogError("ShopPanel not found under ShopUI!");
                return;
            }

            Undo.RegisterCompleteObjectUndo(shopUI.gameObject, "Build Shop UI");

            // 1. Create Inventory Panel
            CreateInventoryPanel(shopPanel.transform);

            // 2. Create Weapon Tooltip
            CreateWeaponTooltip(shopUI.transform);

            Debug.Log("Shop UI components built successfully!");
        }

        private static void CreateInventoryPanel(Transform parent)
        {
            Transform existing = parent.Find("InventoryPanel");
            if (existing != null)
            {
                Debug.Log("InventoryPanel already exists.");
                return;
            }

            GameObject panelObj = new GameObject("InventoryPanel", typeof(RectTransform));
            panelObj.transform.SetParent(parent, false);
            
            // RectTransform Setup (Bottom Center)
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f); // Bottom Center
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 20);
            rect.sizeDelta = new Vector2(600, 100);

            // Background (Optional)
            // Image bg = panelObj.AddComponent<Image>();
            // bg.color = new Color(0, 0, 0, 0.4f);

            // Add Script
            ShopInventoryUI inventoryUI = panelObj.AddComponent<ShopInventoryUI>();

            // Container for Slots
            GameObject containerObj = new GameObject("SlotsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            containerObj.transform.SetParent(panelObj.transform, false);
            RectTransform containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero; // Stretch

            HorizontalLayoutGroup layout = containerObj.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 15;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Template Slot
            GameObject slotTemplate = CreateSlotTemplate(panelObj.transform);
            slotTemplate.name = "SlotTemplate";
            slotTemplate.SetActive(false); // Hide template

            // Assign References via SerializedObject
            SerializedObject so = new SerializedObject(inventoryUI);
            so.FindProperty("_slotsContainer").objectReferenceValue = containerObj.transform;
            so.FindProperty("_slotPrefab").objectReferenceValue = slotTemplate.GetComponent<InventorySlotUI>();
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateSlotTemplate(Transform parent)
        {
            GameObject slotObj = new GameObject("SlotTemplate", typeof(RectTransform), typeof(Image), typeof(InventorySlotUI));
            slotObj.transform.SetParent(parent, false);
            
            RectTransform rect = slotObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);
            
            Image bg = slotObj.GetComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark Grid

            // Filled State
            GameObject filled = CreateRectChild(slotObj.transform, "Filled");
            Image filledBg = filled.AddComponent<Image>();
            filledBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            GameObject border = CreateRectChild(filled.transform, "Border");
            Image borderImg = border.AddComponent<Image>();
            borderImg.type = Image.Type.Sliced;
            borderImg.color = Color.white;
            border.GetComponent<RectTransform>().sizeDelta = Vector2.zero; // Stretch
            
            GameObject icon = CreateRectChild(filled.transform, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.preserveAspect = true;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;

            // Empty State
            GameObject empty = CreateRectChild(slotObj.transform, "Empty");
            Image emptyImg = empty.AddComponent<Image>();
            emptyImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            // Assign to script
            InventorySlotUI ui = slotObj.GetComponent<InventorySlotUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_filledState").objectReferenceValue = filled;
            so.FindProperty("_emptyState").objectReferenceValue = empty;
            so.FindProperty("_iconImage").objectReferenceValue = iconImg;
            so.FindProperty("_rarityBorder").objectReferenceValue = borderImg;
            so.ApplyModifiedProperties();

            return slotObj;
        }

        private static void CreateWeaponTooltip(Transform parent)
        {
            if (parent.Find("WeaponTooltip") != null) return;

            GameObject tooltipObj = new GameObject("WeaponTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(WeaponTooltipUI));
            tooltipObj.transform.SetParent(parent, false);

            RectTransform rect = tooltipObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 150);
            rect.pivot = new Vector2(0, 1); // Top Left pivot for following mouse? Or logic handles it.

            Image bg = tooltipObj.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);

            VerticalLayoutGroup layout = tooltipObj.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            ContentSizeFitter fitter = tooltipObj.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Texts
            TextMeshProUGUI nameTxt = CreateText(tooltipObj.transform, "NameText", "Weapon Name", 18, FontStyles.Bold);
            TextMeshProUGUI typeTxt = CreateText(tooltipObj.transform, "TypeText", "Type", 14, FontStyles.Italic);
            TextMeshProUGUI rarityTxt = CreateText(tooltipObj.transform, "RarityText", "Common", 14, FontStyles.Bold);
            TextMeshProUGUI statsTxt = CreateText(tooltipObj.transform, "StatsText", "Dmg: 10", 14, FontStyles.Normal);

            // Assign
            WeaponTooltipUI ui = tooltipObj.GetComponent<WeaponTooltipUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_nameText").objectReferenceValue = nameTxt;
            so.FindProperty("_typeText").objectReferenceValue = typeTxt;
            so.FindProperty("_rarityText").objectReferenceValue = rarityTxt;
            so.FindProperty("_statsText").objectReferenceValue = statsTxt;
            so.FindProperty("_panel").objectReferenceValue = tooltipObj; // Itself
            so.ApplyModifiedProperties();
            
            tooltipObj.SetActive(false); // Hide by default
        }

        private static GameObject CreateRectChild(Transform parent, string name)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform r = obj.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            return obj;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            
            TextMeshProUGUI txt = obj.GetComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Left;
            
            return txt;
        }

        // ============================
        // Story 6-6: Lock UI
        // ============================
        
        private static void AddLockUIToShopItems()
        {
            // Find the ShopUI to get the item template
            ShopUI shopUI = FindObjectOfType<ShopUI>();
            if (shopUI == null)
            {
                Debug.LogError("ShopUI not found in scene!");
                return;
            }
            
            // Get the template via serialized field
            SerializedObject shopSO = new SerializedObject(shopUI);
            GameObject template = shopSO.FindProperty("_itemTemplate").objectReferenceValue as GameObject;
            
            if (template == null)
            {
                Debug.LogError("ItemTemplate not assigned on ShopUI!");
                return;
            }
            
            ShopItemUI itemUI = template.GetComponent<ShopItemUI>();
            if (itemUI == null)
            {
                Debug.LogError("ShopItemUI component not found on ItemTemplate!");
                return;
            }
            
            Undo.RegisterCompleteObjectUndo(template, "Add Lock UI");
            
            // Check if already added
            SerializedObject itemSO = new SerializedObject(itemUI);
            if (itemSO.FindProperty("_lockButton").objectReferenceValue != null)
            {
                Debug.Log("Lock UI already exists on ItemTemplate.");
                return;
            }
            
            // Create Lock UI elements
            CreateLockElements(template.transform, itemUI);
            
            EditorUtility.SetDirty(template);
            Debug.Log("Lock UI added to ShopItemUI template!");
        }
        
        private static void CreateLockElements(Transform itemRoot, ShopItemUI itemUI)
        {
            // 1. Lock Overlay - semi-transparent gold tint over the whole card
            GameObject lockOverlay = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
            lockOverlay.transform.SetParent(itemRoot, false);
            RectTransform overlayRect = lockOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            Image overlayImg = lockOverlay.GetComponent<Image>();
            overlayImg.color = new Color(1f, 0.85f, 0.2f, 0.15f); // Subtle gold tint
            overlayImg.raycastTarget = false;
            lockOverlay.SetActive(false); // Hidden by default
            
            // 2. Lock Button - positioned at top-right corner
            GameObject lockBtnObj = new GameObject("LockButton", typeof(RectTransform), typeof(Image), typeof(Button));
            lockBtnObj.transform.SetParent(itemRoot, false);
            RectTransform btnRect = lockBtnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 1f); // Top-right
            btnRect.anchorMax = new Vector2(1f, 1f);
            btnRect.pivot = new Vector2(1f, 1f);
            btnRect.anchoredPosition = new Vector2(-4, -4);
            btnRect.sizeDelta = new Vector2(32, 32);
            
            Image btnBg = lockBtnObj.GetComponent<Image>();
            btnBg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f); // Dark semi-transparent
            
            Button lockBtn = lockBtnObj.GetComponent<Button>();
            // Setup button colors
            ColorBlock colors = lockBtn.colors;
            colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            colors.selectedColor = colors.normalColor;
            lockBtn.colors = colors;
            lockBtn.targetGraphic = btnBg;
            
            // 3. Lock Icon - inside the button
            GameObject lockIconObj = new GameObject("LockIcon", typeof(RectTransform), typeof(Image));
            lockIconObj.transform.SetParent(lockBtnObj.transform, false);
            RectTransform iconRect = lockIconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.sizeDelta = Vector2.zero;
            
            Image lockIcon = lockIconObj.GetComponent<Image>();
            lockIcon.color = Color.gray; // Unlocked state = gray
            lockIcon.raycastTarget = false;
            
            // 4. Lock Label ("🔒" text fallback if no sprite)
            GameObject lockLabelObj = new GameObject("LockLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            lockLabelObj.transform.SetParent(lockBtnObj.transform, false);
            RectTransform labelRect = lockLabelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI lockLabel = lockLabelObj.GetComponent<TextMeshProUGUI>();
            lockLabel.text = "\U0001F512"; // 🔒 emoji
            lockLabel.fontSize = 16;
            lockLabel.alignment = TextAlignmentOptions.Center;
            lockLabel.color = Color.gray;
            lockLabel.raycastTarget = false;
            
            // Wire up references
            SerializedObject so = new SerializedObject(itemUI);
            so.FindProperty("_lockButton").objectReferenceValue = lockBtn;
            so.FindProperty("_lockIcon").objectReferenceValue = lockIcon;
            so.FindProperty("_lockOverlay").objectReferenceValue = lockOverlay;
            so.ApplyModifiedProperties();
        }
    }
}
