#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Overun.Shop;
using Overun.UI;

namespace Overun.Editor.Tools
{
    public class ShopUIBuilder
    {
        [MenuItem("Overun/UI/Create Shop UI")]
        public static void CreateShopUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("ShopUI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add ShopUI Component
            ShopUI shopUI = canvasObj.AddComponent<ShopUI>();

            // Create Panel
            GameObject panelObj = new GameObject("ShopPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.95f);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "WEAPON SHOP";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.yellow;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.9f);
            titleRect.anchorMax = new Vector2(0.7f, 1f);

            // Item Container
            GameObject itemsContainerObj = new GameObject("ItemsContainer");
            itemsContainerObj.transform.SetParent(panelObj.transform, false);
            HorizontalLayoutGroup hLayout = itemsContainerObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 30;
            
            RectTransform containerRect = itemsContainerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.05f, 0.2f);
            containerRect.anchorMax = new Vector2(0.95f, 0.8f);

            // Item Template (Hidden)
            GameObject itemTemplate = CreateComplexItemTemplate(itemsContainerObj.transform);
            itemTemplate.name = "ItemTemplate";
            itemTemplate.SetActive(false);

            // Buttons
            GameObject rerollBtnObj = CreateButton("RerollButton", "Reroll", panelObj.transform, new Color(1f, 0.5f, 0f));
            RectTransform rerollRect = rerollBtnObj.GetComponent<RectTransform>();
            rerollRect.anchorMin = new Vector2(0.4f, 0.05f);
            rerollRect.anchorMax = new Vector2(0.4f, 0.05f);
            rerollRect.anchoredPosition = new Vector2(0, 50);

            GameObject continueBtnObj = CreateButton("ContinueButton", "NEXT WAVE", panelObj.transform, Color.green);
            RectTransform continueRect = continueBtnObj.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.6f, 0.05f);
            continueRect.anchorMax = new Vector2(0.6f, 0.05f);
            continueRect.anchoredPosition = new Vector2(0, 50);

            // Stats Panel
            GameObject statsObj = new GameObject("StatsPanel");
            statsObj.transform.SetParent(panelObj.transform, false);
            PlayerStatsUI statsUI = statsObj.AddComponent<PlayerStatsUI>();
            
            VerticalLayoutGroup vStats = statsObj.AddComponent<VerticalLayoutGroup>();
            vStats.spacing = 5;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.02f, 0.1f);
            statsRect.anchorMax = new Vector2(0.2f, 0.5f);
            
            // Create stat texts
            TextMeshProUGUI dmgText = CreateText("DmgText", "DMG: x1.0", statsObj.transform, 20, Color.white).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI spdText = CreateText("SpdText", "SPD: x1.0", statsObj.transform, 20, Color.white).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hpText = CreateText("HpText", "HP: x1.0", statsObj.transform, 20, Color.white).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI critText = CreateText("CritText", "CRIT: 5%", statsObj.transform, 20, Color.white).GetComponent<TextMeshProUGUI>();

            // Setup Stats References via Reflection to avoid public field requirement if private
            SerializedObject statSo = new SerializedObject(statsUI);
            statSo.FindProperty("_damageText").objectReferenceValue = dmgText;
            statSo.FindProperty("_speedText").objectReferenceValue = spdText;
            statSo.FindProperty("_healthText").objectReferenceValue = hpText;
            statSo.FindProperty("_critText").objectReferenceValue = critText;
            statSo.ApplyModifiedProperties();

            // Setup ShopUI References
            SerializedObject so = new SerializedObject(shopUI);
            so.FindProperty("_shopPanel").objectReferenceValue = panelObj;
            so.FindProperty("_itemsContainer").objectReferenceValue = itemsContainerObj.transform;
            so.FindProperty("_itemTemplate").objectReferenceValue = itemTemplate;
            so.FindProperty("_rerollButton").objectReferenceValue = rerollBtnObj.GetComponent<Button>();
            so.FindProperty("_rerollCostText").objectReferenceValue = rerollBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtnObj.GetComponent<Button>();
            so.FindProperty("_statsUI").objectReferenceValue = statsUI;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Shop UI");
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("Shop UI Created!");
        }

        private static GameObject CreateComplexItemTemplate(Transform parent)
        {
            GameObject itemObj = new GameObject("ItemTemplate");
            itemObj.transform.SetParent(parent, false);
            
            // Background
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f);
            
            // Border (Slightly larger rect or overlay)
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(itemObj.transform, false);
            Image border = borderObj.AddComponent<Image>();
            border.color = Color.white;
            border.type = Image.Type.Sliced;
            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);
            
            // ShopItemUI Component
            ShopItemUI itemUI = itemObj.AddComponent<ShopItemUI>();
            
            VerticalLayoutGroup vLayout = itemObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(10, 10, 10, 10);
            vLayout.spacing = 10;
            vLayout.childControlHeight = false;
            vLayout.childControlWidth = true;

            // Name
            TextMeshProUGUI nameText = CreateText("NameText", "Weapon Name", itemObj.transform, 24, Color.white).GetComponent<TextMeshProUGUI>();
            
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.gray;
            if(iconObj.GetComponent<LayoutElement>() != null)
            {
            iconObj.GetComponent<LayoutElement>().minHeight = 100;
            }
            else
            {
                iconObj.AddComponent<LayoutElement>().minHeight = 100;
            }

            
            return itemObj;
        }

        private static GameObject CreateText(string name, string content, Transform parent, float fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAlignmentOptions.Center;
            return textObj;
        }

        private static GameObject CreateButton(string name, string label, Transform parent, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 50);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            return btnObj;
        }
    }
}
#endif
