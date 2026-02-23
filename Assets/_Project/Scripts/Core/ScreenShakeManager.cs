using UnityEngine;
using Cinemachine;

namespace Overun.Core
{
    public class ScreenShakeManager : MonoBehaviour
    {
        public static ScreenShakeManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _defaultIntensity = 0.15f;
        [SerializeField] private float _defaultDuration = 0.2f;
        [SerializeField] private float _frequency = 25f;
        [SerializeField] private float _maxIntensity = 10f;

        private CinemachineBasicMultiChannelPerlin _perlin;
        
        // Timer-based shake (no coroutine — immune to rapid-fire interruption)
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            var vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                _perlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            if (_perlin == null)
            {
                Debug.LogWarning("[ScreenShakeManager] No CinemachineBasicMultiChannelPerlin found!");
                return;
            }

            // IMPORTANT: Reset to zero so camera doesn't shake at game start
            _perlin.m_AmplitudeGain = 0f;
            _perlin.m_FrequencyGain = _frequency;
        }

        private void Update()
        {
            if (_perlin == null) return;

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.unscaledDeltaTime;
                
                // Decay amplitude linearly
                float t = Mathf.Clamp01(_shakeTimer / _shakeDuration);
                _perlin.m_AmplitudeGain = _shakeIntensity * t;
            }
            else if (_perlin.m_AmplitudeGain > 0f)
            {
                _perlin.m_AmplitudeGain = 0f;
            }
        }

        public void Shake(float intensity, float duration)
        {
            if (_perlin == null) return;

            Debug.Log($"[ScreenShakeManager] Shake: intensity={intensity}, duration={duration}");

            // Calculate what's left of the current shake
            float remainingTime = Mathf.Max(0f, _shakeTimer);
            float remainingIntensity = remainingTime > 0f 
                ? _shakeIntensity * Mathf.Clamp01(remainingTime / _shakeDuration) 
                : 0f;
            
            // Only upgrade — never let a weak shake kill a strong one
            float newIntensity = Mathf.Min(Mathf.Max(intensity, remainingIntensity), _maxIntensity);
            float newDuration = Mathf.Max(duration, remainingTime);
            
            _shakeIntensity = newIntensity;
            _shakeDuration = newDuration;
            _shakeTimer = newDuration;
        }

        public void Shake() => Shake(_defaultIntensity, _defaultDuration);

        public void ShakeScaled(float damageAmount, float intensityPerDamage = 0.005f, float duration = 0.15f)
        {
            float intensity = Mathf.Clamp(damageAmount * intensityPerDamage, 0.02f, _maxIntensity);
            Shake(intensity, duration);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}