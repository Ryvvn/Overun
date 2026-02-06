using UnityEngine;
using UnityEngine.UI;

namespace Overun.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            // Ensure cursor is visible in menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnPlayClicked()
        {
            if (Overun.Core.GameManager.Instance != null)
            {
                Overun.Core.GameManager.Instance.StartGame();
            }
            else
            {
                // Fallback if GameManager isn't present in this empty scene yet
                // Ideally GameManager triggers scene load, but for MainMenu we might need direct call
                // OR we instantiate GameManager here. 
                // For now, let's assume direct SceneManager call as fallback.
                Debug.LogWarning("GameManager instance not found. Using SceneManager fallback.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            }
        }

        private void OnQuitClicked()
        {
            if (Overun.Core.GameManager.Instance != null)
            {
                Overun.Core.GameManager.Instance.QuitGame();
            }
            else
            {
                Debug.Log("Quitting Application...");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}
