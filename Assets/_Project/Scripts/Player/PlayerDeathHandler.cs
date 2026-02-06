using UnityEngine;
using UnityEngine.InputSystem;
using Overun.Core;

namespace Overun.Player
{
    /// <summary>
    /// Handles player death behavior: disables controls, triggers visual feedback,
    /// and notifies GameManager.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private CharacterController _characterController;
        
        [Header("Death Settings")]
        [SerializeField] private bool _disableCollider = true;
        [SerializeField] private bool _dropOnDeath = true;
        [SerializeField] private float _deathDropSpeed = 2f;
        
        [Header("Camera")]
        [SerializeField] private bool _shakeOnDeath = true;
        [SerializeField] private float _shakeIntensity = 0.3f;
        [SerializeField] private float _shakeDuration = 0.2f;
        
        private PlayerHealth _playerHealth;
        private bool _isDead = false;
        private Vector3 _deathPosition;
        
        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            
            // Auto-find components
            if (_playerController == null)
                _playerController = GetComponent<PlayerController>();
            
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
            
            if (_characterController == null)
                _characterController = GetComponent<CharacterController>();
        }
        
        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath += HandleDeath;
            }
        }
        
        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= HandleDeath;
            }
        }
        
        private void Update()
        {
            // Optional: Simple death drop animation
            if (_isDead && _dropOnDeath && _characterController != null)
            {
                // Gradually lower the player (simulate falling/collapsing)
                if (transform.position.y > _deathPosition.y - 0.5f)
                {
                    _characterController.enabled = true;
                    _characterController.Move(Vector3.down * _deathDropSpeed * Time.unscaledDeltaTime);
                }
            }
        }
        
        private void HandleDeath()
        {
            if (_isDead) return;
            _isDead = true;
            
            _deathPosition = transform.position;
            
            Debug.Log("[PlayerDeathHandler] Player died - disabling controls");
            
            // Disable player controller (stops movement processing)
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }
            
            // Disable input system
            if (_playerInput != null)
            {
                _playerInput.DeactivateInput();
            }
            
            // Optional: Disable character controller collision
            if (_disableCollider && _characterController != null)
            {
                // We keep it enabled for the drop animation but could disable
            }
            
            // Camera shake effect
            if (_shakeOnDeath)
            {
                StartCoroutine(CameraShake());
            }
            
            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDeath();
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] GameManager not found!");
            }
            
            // Unlock cursor so player can click UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        private System.Collections.IEnumerator CameraShake()
        {
            Transform cam = Camera.main?.transform;
            if (cam == null) yield break;
            
            Vector3 originalPos = cam.localPosition;
            float elapsed = 0f;
            
            while (elapsed < _shakeDuration)
            {
                float x = Random.Range(-_shakeIntensity, _shakeIntensity);
                float y = Random.Range(-_shakeIntensity, _shakeIntensity);
                
                cam.localPosition = originalPos + new Vector3(x, y, 0f);
                
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            cam.localPosition = originalPos;
        }
        
        /// <summary>
        /// Called when restarting to reset death state.
        /// Normally the scene reloads, but this is here for future respawn systems.
        /// </summary>
        public void ResetDeathState()
        {
            _isDead = false;
            
            if (_playerController != null)
                _playerController.enabled = true;
            
            if (_playerInput != null)
                _playerInput.ActivateInput();
        }
    }
}
