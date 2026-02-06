using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Overun.Core;
using Overun.Player;

namespace Overun.UI
{
    /// <summary>
    /// Death screen UI that appears when player dies.
    /// Shows "You Died" message with Restart and Quit options.
    /// </summary>
    public class DeathUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private TextMeshProUGUI _deathText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Text Settings")]
        [SerializeField] private string _deathMessage = "YOU DIED";
        [SerializeField] private Color _textColor = new Color(0.8f, 0.1f, 0.1f);
        
        private PlayerHealth _playerHealth;
        private bool _isShowing = false;
        
        private void Awake()
        {
            // Ensure panel is hidden initially
            if (_deathPanel != null)
            {
                _deathPanel.SetActive(false);
            }
            
            // Setup canvas group for fade
            if (_canvasGroup == null && _deathPanel != null)
            {
                _canvasGroup = _deathPanel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _deathPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Setup text
            if (_deathText != null)
            {
                _deathText.text = _deathMessage;
                _deathText.color = _textColor;
            }
        }
        
        private void Start()
        {
            // Find player health to subscribe to death event
            _playerHealth = FindObjectOfType<PlayerHealth>();
            
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath += ShowDeathScreen;
            }
            else
            {
                Debug.LogWarning("[DeathUI] PlayerHealth not found!");
            }
            
            // Setup button listeners
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }
        
        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= ShowDeathScreen;
            }
            
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }
            
            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }
        
        public void ShowDeathScreen()
        {
            if (_isShowing) return;
            _isShowing = true;
            
            Debug.Log("[DeathUI] Showing death screen");
            
            if (_deathPanel != null)
            {
                _deathPanel.SetActive(true);
                StartCoroutine(FadeIn());
            }
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;
            
            _canvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _fadeInDuration;
                _canvasGroup.alpha = _fadeCurve.Evaluate(t);
                yield return null;
            }
            
            _canvasGroup.alpha = 1f;
        }
        
        public void HideDeathScreen()
        {
            _isShowing = false;
            
            if (_deathPanel != null)
            {
                _deathPanel.SetActive(false);
            }
        }
        
        private void OnRestartClicked()
        {
            Debug.Log("[DeathUI] Restart clicked");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                // Fallback: reload scene directly
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
        
        private void OnQuitClicked()
        {
            Debug.Log("[DeathUI] Quit clicked");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
