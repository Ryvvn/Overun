using UnityEngine;
using System.Collections;
using Overun.Core;
using Overun.Enemies;
using Overun.Combat;

namespace Overun.Weapons
{
    /// <summary>
    /// Subscribes to combat events and triggers screen shake + hit-stop feedback.
    /// Bridges Enemy/PlayerHealth damage events to ScreenShakeManager.
    /// </summary>
    public class HitFeedbackManager : MonoBehaviour
    {
        [Header("Hit Shake (Enemy Damaged)")]
        [SerializeField] private float _hitShakeIntensity = 0.05f;
        [SerializeField] private float _hitShakeDuration = 0.08f;
        [SerializeField] private float _hitIntensityPerDamage = 0.003f;
        
        [Header("Kill Shake")]
        [SerializeField] private float _killShakeIntensity = 0.2f;
        [SerializeField] private float _killShakeDuration = 0.15f;
        
        [Header("Player Damage Shake")]
        [SerializeField] private float _playerDamageShakeIntensity = 0.3f;
        [SerializeField] private float _playerDamageShakeDuration = 0.25f;
        
        [Header("Hit-Stop (Kill Freeze)")]
        [SerializeField] private bool _enableHitStop = true;
        [SerializeField] private float _hitStopDuration = 0.04f; // 40ms
        [SerializeField] private float _hitStopTimeScale = 0.05f;
        
        // Track active hit-stop to avoid stacking
        private bool _isInHitStop;
        
        private void OnEnable()
        {
            Enemy.OnAnyEnemyDamaged += HandleEnemyDamaged;
        }
        
        private void OnDisable()
        {
            Enemy.OnAnyEnemyDamaged -= HandleEnemyDamaged;
        }
        
        /// <summary>
        /// Call this from PlayerHealth or subscribe to PlayerHealth.OnDamaged.
        /// Since PlayerHealth is in Overun.Player (can't easily subscribe from here),
        /// this is exposed as a public method to be called by a bridge component.
        /// </summary>
        public void TriggerPlayerDamageShake()
        {
            if (ScreenShakeManager.Instance != null)
            {
                ScreenShakeManager.Instance.Shake(_playerDamageShakeIntensity, _playerDamageShakeDuration);
            }
        }
        
        private void HandleEnemyDamaged(Vector3 position, float damage, ElementType element)
        {
            // Scale shake by damage
            if (ScreenShakeManager.Instance != null)
            {
                float intensity = Mathf.Max(_hitShakeIntensity, damage * _hitIntensityPerDamage);
                ScreenShakeManager.Instance.Shake(intensity, _hitShakeDuration);
            }
        }
        
        /// <summary>
        /// Call this when an enemy dies to trigger kill shake + hit-stop.
        /// Should be subscribed to individual Enemy.OnDeath events.
        /// </summary>
        public void TriggerKillFeedback()
        {
            // Kill shake
            if (ScreenShakeManager.Instance != null)
            {
                ScreenShakeManager.Instance.Shake(_killShakeIntensity, _killShakeDuration);
            }
            
            // Hit-stop
            if (_enableHitStop && !_isInHitStop)
            {
                StartCoroutine(HitStopCoroutine());
            }
        }
        
        private IEnumerator HitStopCoroutine()
        {
            _isInHitStop = true;
            
            float previousTimeScale = Time.timeScale;
            Time.timeScale = _hitStopTimeScale;
            
            // Wait in unscaled time
            yield return new WaitForSecondsRealtime(_hitStopDuration);
            
            Time.timeScale = previousTimeScale;
            _isInHitStop = false;
        }
    }
}
