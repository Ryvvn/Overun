using UnityEngine;
using UnityEngine.UI;

namespace Overun.UI
{
    /// <summary>
    /// Full-screen red vignette that flashes when the player takes damage.
    /// Attach to a full-screen UI Image on a Canvas overlay.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DamageVignetteUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _flashAlpha = 0.35f;
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private Color _vignetteColor = new Color(0.8f, 0f, 0f, 0f);
        
        private Image _vignetteImage;
        private float _currentAlpha;
        
        // Static event any system can fire to trigger the vignette
        public static event System.Action OnPlayerDamageFlash;
        
        private void Awake()
        {
            _vignetteImage = GetComponent<Image>();
            _vignetteImage.color = _vignetteColor;
            _vignetteImage.raycastTarget = false; // Don't block input
        }
        
        private void OnEnable()
        {
            OnPlayerDamageFlash += Flash;
        }
        
        private void OnDisable()
        {
            OnPlayerDamageFlash -= Flash;
        }
        
        private void Update()
        {
            if (_currentAlpha > 0f)
            {
                _currentAlpha = Mathf.MoveTowards(_currentAlpha, 0f, _fadeSpeed * Time.unscaledDeltaTime);
                _vignetteColor.a = _currentAlpha;
                _vignetteImage.color = _vignetteColor;
            }
        }
        
        /// <summary>
        /// Trigger the damage flash vignette.
        /// </summary>
        public void Flash()
        {
            _currentAlpha = _flashAlpha;
            _vignetteColor.a = _currentAlpha;
            _vignetteImage.color = _vignetteColor;
        }
        
        /// <summary>
        /// Static helper to trigger flash from any system.
        /// </summary>
        public static void TriggerFlash()
        {
            OnPlayerDamageFlash?.Invoke();
        }
    }
}
