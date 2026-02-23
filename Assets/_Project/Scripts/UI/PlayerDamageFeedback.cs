using UnityEngine;
using Overun.Player;
using Overun.Weapons;
using Overun.Core;

namespace Overun.UI
{
    /// <summary>
    /// Bridge component that connects PlayerHealth damage events to the
    /// screen shake, hit feedback, and damage vignette systems.
    /// Place on the Player GameObject (or any persistent object with PlayerHealth reference).
    /// </summary>
    public class PlayerDamageFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private HitFeedbackManager _hitFeedbackManager;
        
        private void Awake()
        {
            if (_playerHealth == null)
                _playerHealth = GetComponent<PlayerHealth>();
        }
        
        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDamaged += HandlePlayerDamaged;
            }
        }
        
        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDamaged -= HandlePlayerDamaged;
            }
        }
        
        private void HandlePlayerDamaged()
        {
            // Trigger damage vignette
            DamageVignetteUI.TriggerFlash();
            
            // Trigger player damage shake
            if (_hitFeedbackManager != null)
            {
                _hitFeedbackManager.TriggerPlayerDamageShake();
            }
            else if (ScreenShakeManager.Instance != null)
            {
                // Fallback: direct shake if no feedback manager assigned
                ScreenShakeManager.Instance.Shake(0.3f, 0.25f);
            }
        }
    }
}
