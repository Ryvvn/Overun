using UnityEngine;
using UnityEngine.UI;

namespace Overun.UI
{
    /// <summary>
    /// Stamina bar UI that displays player stamina using a Unity Slider.
    /// </summary>
    public class StaminaBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Overun.Player.PlayerController _playerController;
        
        [Header("Color Settings")]
        [SerializeField] private Color _staminaColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private Color _depletedColor = new Color(0.5f, 0.3f, 0.3f);
        [SerializeField] private float _lowStaminaThreshold = 0.2f;
        
        [Header("Animation")]
        [SerializeField] private float _smoothSpeed = 8f;
        [SerializeField] private bool _useSmoothing = true;
        
        [Header("Visibility")]
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private float _fadeSpeed = 3f;
        
        private float _targetValue = 1f;
        private CanvasGroup _canvasGroup;
        private float _targetAlpha = 1f;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null && _hideWhenFull)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Auto-find slider if not assigned
            if (_staminaSlider == null)
            {
                _staminaSlider = GetComponent<Slider>();
                if (_staminaSlider == null)
                {
                    _staminaSlider = GetComponentInChildren<Slider>();
                }
            }
            
            // Auto-find fill image from slider
            if (_staminaSlider != null && _fillImage == null)
            {
                _fillImage = _staminaSlider.fillRect?.GetComponent<Image>();
            }
            
            // Configure slider
            if (_staminaSlider != null)
            {
                _staminaSlider.minValue = 0f;
                _staminaSlider.maxValue = 1f;
                _staminaSlider.value = 1f;
                _staminaSlider.interactable = false; // Display only, not interactive
            }
            
            // Set initial color
            if (_fillImage != null)
            {
                _fillImage.color = _staminaColor;
            }
        }
        
        private void Start()
        {
            if (_playerController == null)
            {
                _playerController = FindObjectOfType<Overun.Player.PlayerController>();
            }
            
            if (_playerController != null)
            {
                _playerController.OnStaminaChanged += UpdateStaminaBar;
                
                // Initialize
                UpdateStaminaBar(_playerController.CurrentStamina, _playerController.MaxStamina);
            }
            else
            {
                Debug.LogWarning("[StaminaBarUI] PlayerController not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (_playerController != null)
            {
                _playerController.OnStaminaChanged -= UpdateStaminaBar;
            }
        }
        
        private void Update()
        {
            if (_staminaSlider == null) return;
            
            if (_useSmoothing)
            {
                _staminaSlider.value = Mathf.Lerp(
                    _staminaSlider.value,
                    _targetValue,
                    _smoothSpeed * Time.deltaTime
                );
            }
            
            // Handle visibility fade
            if (_hideWhenFull && _canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(
                    _canvasGroup.alpha,
                    _targetAlpha,
                    _fadeSpeed * Time.deltaTime
                );
            }
        }
        
        private void UpdateStaminaBar(float currentStamina, float maxStamina)
        {
            float staminaPercent = Mathf.Clamp01(currentStamina / maxStamina);
            _targetValue = staminaPercent;
            
            if (_staminaSlider != null && !_useSmoothing)
            {
                _staminaSlider.value = staminaPercent;
            }
            
            // Color based on stamina level
            if (_fillImage != null)
            {
                _fillImage.color = staminaPercent <= _lowStaminaThreshold 
                    ? _depletedColor 
                    : _staminaColor;
            }
            
            // Show/hide based on stamina
            if (_hideWhenFull)
            {
                _targetAlpha = staminaPercent < 0.99f ? 1f : 0f;
            }
            
            Debug.Log($"[StaminaBarUI] Stamina: {currentStamina}/{maxStamina} = {staminaPercent:P0}");
        }
        
        public void SetStaminaDisplay(float percent)
        {
            _targetValue = Mathf.Clamp01(percent);
            if (_staminaSlider != null)
            {
                _staminaSlider.value = _targetValue;
            }
        }
    }
}
