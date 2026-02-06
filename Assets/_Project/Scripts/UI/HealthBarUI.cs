using UnityEngine;
using UnityEngine.UI;

namespace Overun.UI
{
    /// <summary>
    /// Health bar UI that displays player health using a Unity Slider.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Overun.Player.PlayerHealth _playerHealth;
        
        [Header("Color Settings")]
        [SerializeField] private Gradient _healthGradient;
        
        [Header("Animation")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _useSmoothing = true;
        
        private float _targetValue = 1f;
        
        private void Awake()
        {
            // Setup default gradient if not configured
            if (_healthGradient == null || _healthGradient.colorKeys.Length == 0)
            {
                SetupDefaultGradient();
            }
            
            // Auto-find slider if not assigned
            if (_healthSlider == null)
            {
                _healthSlider = GetComponent<Slider>();
                if (_healthSlider == null)
                {
                    _healthSlider = GetComponentInChildren<Slider>();
                }
            }
            
            // Auto-find fill image from slider
            if (_healthSlider != null && _fillImage == null)
            {
                _fillImage = _healthSlider.fillRect?.GetComponent<Image>();
            }
            
            // Configure slider
            if (_healthSlider != null)
            {
                _healthSlider.minValue = 0f;
                _healthSlider.maxValue = 1f;
                _healthSlider.value = 1f;
                _healthSlider.interactable = false; // Display only, not interactive
            }
        }
        
        private void Start()
        {
            // Find player health if not assigned
            if (_playerHealth == null)
            {
                _playerHealth = FindObjectOfType<Overun.Player.PlayerHealth>();
            }
            
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += UpdateHealthBar;
                
                // Initialize with current health
                UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
            else
            {
                Debug.LogWarning("[HealthBarUI] PlayerHealth not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= UpdateHealthBar;
            }
        }
        
        private void Update()
        {
            if (_healthSlider == null) return;
            
            if (_useSmoothing)
            {
                _healthSlider.value = Mathf.Lerp(
                    _healthSlider.value, 
                    _targetValue, 
                    _smoothSpeed * Time.deltaTime
                );
            }
            
            // Update color based on current value
            if (_fillImage != null)
            {
                _fillImage.color = _healthGradient.Evaluate(_healthSlider.value);
            }
        }
        
        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
            _targetValue = healthPercent;
            
            if (_healthSlider != null && !_useSmoothing)
            {
                _healthSlider.value = healthPercent;
            }
            
            if (_fillImage != null)
            {
                _fillImage.color = _healthGradient.Evaluate(healthPercent);
            }
            
            Debug.Log($"[HealthBarUI] Health: {currentHealth}/{maxHealth} = {healthPercent:P0}");
        }
        
        private void SetupDefaultGradient()
        {
            _healthGradient = new Gradient();
            
            // Color keys: Red at 0%, Yellow at 50%, Green at 100%
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(0.8f, 0.2f, 0.2f), 0f);      // Red
            colorKeys[1] = new GradientColorKey(new Color(0.9f, 0.8f, 0.2f), 0.5f);    // Yellow
            colorKeys[2] = new GradientColorKey(new Color(0.2f, 0.8f, 0.3f), 1f);      // Green
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            _healthGradient.SetKeys(colorKeys, alphaKeys);
        }
        
        /// <summary>
        /// Manually set health display (for previewing in editor or non-connected use)
        /// </summary>
        public void SetHealthDisplay(float percent)
        {
            _targetValue = Mathf.Clamp01(percent);
            if (_healthSlider != null)
            {
                _healthSlider.value = _targetValue;
            }
            if (_fillImage != null)
            {
                _fillImage.color = _healthGradient.Evaluate(_targetValue);
            }
        }
    }
}
