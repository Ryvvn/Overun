#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Overun.Shop;

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
            panelImage.color = new Color(0, 0, 0, 0.9f);
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
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(0, 80);

            // Item Container
            GameObject itemsContainerObj = new GameObject("ItemsContainer");
            itemsContainerObj.transform.SetParent(panelObj.transform, false);
            HorizontalLayoutGroup hLayout = itemsContainerObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 20;
            hLayout.childControlHeight = false;
            hLayout.childControlWidth = false;
            
            RectTransform containerRect = itemsContainerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0.2f);
            containerRect.anchorMax = new Vector2(1, 0.8f);
            containerRect.sizeDelta = Vector2.zero;

            // Item Template (Hidden)
            GameObject itemTemplate = CreateItemTemplate(itemsContainerObj.transform);
            itemTemplate.name = "ItemTemplate";
            itemTemplate.SetActive(false);

            // Reroll Button
            GameObject rerollBtnObj = CreateButton("RerollButton", "Reroll (5g)", panelObj.transform, new Color(1f, 0.5f, 0f));
            RectTransform rerollRect = rerollBtnObj.GetComponent<RectTransform>();
            rerollRect.anchorMin = new Vector2(0.5f, 0.1f);
            rerollRect.anchorMax = new Vector2(0.5f, 0.1f);
            rerollRect.anchoredPosition = new Vector2(-150, 50);

            // Continue Button (Next Wave)
            GameObject continueBtnObj = CreateButton("ContinueButton", "NEXT WAVE", panelObj.transform, Color.green);
            RectTransform continueRect = continueBtnObj.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.5f, 0.1f);
            continueRect.anchorMax = new Vector2(0.5f, 0.1f);
            continueRect.anchoredPosition = new Vector2(150, 50);

            // Update ShopUI references
            SerializedObject so = new SerializedObject(shopUI);
            so.FindProperty("_shopPanel").objectReferenceValue = panelObj;
            so.FindProperty("_itemsContainer").objectReferenceValue = itemsContainerObj.transform;
            so.FindProperty("_itemTemplate").objectReferenceValue = itemTemplate;
            so.FindProperty("_rerollButton").objectReferenceValue = rerollBtnObj.GetComponent<Button>();
            so.FindProperty("_rerollCostText").objectReferenceValue = rerollBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtnObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Shop UI");
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("Shop UI Created!");
        }

        private static GameObject CreateItemTemplate(Transform parent)
        {
            GameObject itemObj = new GameObject("ItemTemplate");
            itemObj.transform.SetParent(parent, false);
            
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f);
            
            RectTransform rect = itemObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 300);
            
            VerticalLayoutGroup vLayout = itemObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(10, 10, 10, 10);
            vLayout.spacing = 10;
            vLayout.childControlHeight = false;
            vLayout.childControlWidth = true;

            // Name
            CreateText("NameText", "Weapon Name", itemObj.transform, 24, Color.white);
            
            // Icon (Placeholder)
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

            // Price / Button
            GameObject btnObj = CreateButton("BuyButton", "100g", itemObj.transform, Color.cyan);
            if(btnObj.GetComponent<LayoutElement>() != null)
            {
                btnObj.GetComponent<LayoutElement>().minHeight = 50;
            }
            else
            {
                btnObj.AddComponent<LayoutElement>().minHeight = 50;
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
            rect.sizeDelta = new Vector2(200, 60);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 24;
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
