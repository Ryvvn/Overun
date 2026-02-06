#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Overun.UI;

namespace Overun.Editor.Tools
{
    public class VictoryUIBuilder
    {
        [MenuItem("Overun/UI/Create Victory UI")]
        public static void CreateVictoryUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("VictoryUI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add VictoryUI Component
            VictoryUI victoryUI = canvasObj.AddComponent<VictoryUI>();

            // Create Panel
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Create Content VBox
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform, false);
            VerticalLayoutGroup vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.spacing = 20;
            vLayout.childControlHeight = false;
            vLayout.childControlWidth = false;
            
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(contentObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "VICTORY!";
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.yellow;
            titleText.fontStyle = FontStyles.Bold;

            // Menu Button
            GameObject menuBtnObj = CreateButton("MenuButton", "Return to Menu", contentObj.transform);
            
            // Continue Button
            GameObject continueBtnObj = CreateButton("ContinueButton", "Continue (Endless)", contentObj.transform);

            // Assign Referneces using SerializedObject (since fields are private)
            SerializedObject so = new SerializedObject(victoryUI);
            so.FindProperty("_panel").objectReferenceValue = panelObj;
            so.FindProperty("_menuButton").objectReferenceValue = menuBtnObj.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtnObj.GetComponent<Button>();
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Victory UI");
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("Victory UI Created! Don't forget to save it as a Prefab.");
        }

        private static GameObject CreateButton(string name, string label, Transform parent)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = Color.white;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 60);

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
