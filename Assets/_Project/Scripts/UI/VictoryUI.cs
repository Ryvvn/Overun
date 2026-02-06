using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Overun.UI
{
    public class VictoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private void Start()
        {
            // Auto-hide on start
            if (_panel != null) _panel.SetActive(false);

            // Bind buttons
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenuClicked);
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);

            // Subscribe to WaveManager
            if (Overun.Waves.WaveManager.Instance != null)
            {
                Overun.Waves.WaveManager.Instance.OnAllWavesCompleted += ShowVictory;
            }
        }

        private void OnDestroy()
        {
            if (Overun.Waves.WaveManager.Instance != null)
            {
                Overun.Waves.WaveManager.Instance.OnAllWavesCompleted -= ShowVictory;
            }
        }

        public void ShowVictory()
        {
            if (_panel != null) _panel.SetActive(true);
            // Show the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Do not pause if we want animations, but usually we pause.
            // Let WaveManager or GameManager handle pause, UI just shows.
            // But requirement says "Game pauses on Victory (unless Endless)".
            Time.timeScale = 0f;
        }

        private void OnMenuClicked()
        {
            Time.timeScale = 1f;
            // Load Menu (Scene 0 or by name)
            SceneManager.LoadScene(0); // Assuming 0 is Menu or Boot

            // Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnContinueClicked()
        {
            Time.timeScale = 1f;
            if (_panel != null) _panel.SetActive(false);
            
            // Notify WaveManager to continue (Endless Mode)
            Overun.Waves.WaveManager.Instance.EnterEndlessMode();

            // Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
