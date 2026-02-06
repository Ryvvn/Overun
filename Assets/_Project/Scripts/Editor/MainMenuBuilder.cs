#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Overun.UI;

namespace Overun.Editor.Tools
{
    public class MainMenuBuilder
    {
        [MenuItem("Overun/UI/Create Main Menu")]
        public static void CreateMainMenu()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("MainMenu_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add MainMenuUI Component
            MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

            // Create Panel (Background)
            GameObject panelObj = new GameObject("BackgroundPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-ish bg
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Create Content VBox
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform, false);
            VerticalLayoutGroup vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.spacing = 30;
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
            titleText.text = "CHAOS OVERUN";
            titleText.fontSize = 96;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.2f, 0.2f); // Red title
            titleText.fontStyle = FontStyles.Bold;

            // Play Button
            GameObject playBtnObj = CreateButton("PlayButton", "PLAY", contentObj.transform, Color.green);
            
            // Quit Button
            GameObject quitBtnObj = CreateButton("QuitButton", "QUIT", contentObj.transform, Color.red);

            // Assign References using SerializedObject
            SerializedObject so = new SerializedObject(menuUI);
            so.FindProperty("_playButton").objectReferenceValue = playBtnObj.GetComponent<Button>();
            so.FindProperty("_quitButton").objectReferenceValue = quitBtnObj.GetComponent<Button>();
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Main Menu");
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("Main Menu Created!");
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
            rect.sizeDelta = new Vector2(300, 80);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 36;
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
