using UnityEngine;
using UnityEngine.InputSystem;
using Overun.Core;

namespace Overun.Player
{
    /// <summary>
    /// Handles weapon firing with support for both hitscan (raycast) and projectile modes.
    /// - Hitscan: Instant hit via raycast (best for rapid-fire weapons)
    /// - Projectile: Physical projectile spawn (best for special/slow weapons)
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        public enum FireMode
        {
            Hitscan,    // Raycast - instant hit
            Projectile  // Physical projectile
        }
        
        [Header("References")]
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private Camera _playerCamera;
        
        [Header("Firing Mode")]
        [SerializeField] private FireMode _fireMode = FireMode.Hitscan;
        
        [Header("Common Settings")]
        [SerializeField] private float _fireRate = 5f; // Shots per second
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _range = 100f;
        
        [Header("Hitscan Settings")]
        [SerializeField] private LayerMask _hitLayers = ~0; // What can be hit
        [SerializeField] private GameObject _hitEffectPrefab; // Spawn on hit
        [SerializeField] private TrailRenderer _bulletTrailPrefab; // Optional tracer
        
        [Header("Projectile Settings")]
        [SerializeField] private string _projectilePoolTag = "Projectile";
        
        [Header("Audio/Visual")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _fireSound;
        [SerializeField] private ParticleSystem _muzzleFlash;
        
        // Input
        private PlayerInputActions _inputActions;
        private bool _isFiring;
        
        // Fire rate
        private float _lastFireTime;
        private float FireInterval => 1f / _fireRate;
        
        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            
            // If no muzzle point, use this transform
            if (_muzzlePoint == null)
            {
                _muzzlePoint = transform;
            }
            
            // Get camera if not assigned
            if (_playerCamera == null)
            {
                _playerCamera = Camera.main;
            }
        }
        
        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Fire.performed += OnFirePerformed;
            _inputActions.Player.Fire.canceled += OnFireCanceled;
        }
        
        private void OnDisable()
        {
            _inputActions.Player.Fire.performed -= OnFirePerformed;
            _inputActions.Player.Fire.canceled -= OnFireCanceled;
            _inputActions.Player.Disable();
        }
        
        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            _isFiring = true;
        }
        
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            _isFiring = false;
        }
        
        private void Update()
        {
            if (_isFiring)
            {
                TryFire();
            }
        }
        
        private void TryFire()
        {
            // Check fire rate
            if (Time.time - _lastFireTime < FireInterval)
            {
                return;
            }
            
            Fire();
            _lastFireTime = Time.time;
        }
        
        private void Fire()
        {
            switch (_fireMode)
            {
                case FireMode.Hitscan:
                    FireHitscan();
                    break;
                case FireMode.Projectile:
                    FireProjectile();
                    break;
            }
            
            // Play effects
            PlayFireEffects();
        }
        
        #region Hitscan
        
        private void FireHitscan()
        {
            // Get aim direction from camera center
            Vector3 aimDirection = GetAimDirection();
            Vector3 origin = _muzzlePoint.position;
            
            // Raycast
            if (Physics.Raycast(origin, aimDirection, out RaycastHit hit, _range, _hitLayers))
            {
                // Apply damage
                // var damageable = hit.collider.GetComponent<IDamageable>();
                // damageable?.TakeDamage(_damage);
                
                // Spawn hit effect
                if (_hitEffectPrefab != null)
                {
                    Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }
                
                // Optional: Spawn bullet trail
                SpawnBulletTrail(origin, hit.point);
                
                Debug.Log($"[WeaponController] Hitscan hit: {hit.collider.name} at {hit.point}");
            }
            else
            {
                // Miss - trail to max range
                Vector3 endPoint = origin + aimDirection * _range;
                SpawnBulletTrail(origin, endPoint);
            }
        }
        
        private void SpawnBulletTrail(Vector3 start, Vector3 end)
        {
            if (_bulletTrailPrefab == null) return;
            
            // Create trail that travels from start to end
            TrailRenderer trail = Instantiate(_bulletTrailPrefab, start, Quaternion.identity);
            StartCoroutine(AnimateBulletTrail(trail, start, end));
        }
        
        private System.Collections.IEnumerator AnimateBulletTrail(TrailRenderer trail, Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            float duration = distance / 200f; // Trail speed
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                trail.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
            
            // Wait for trail to fade
            yield return new WaitForSeconds(trail.time);
            Destroy(trail.gameObject);
        }
        
        #endregion
        
        #region Projectile
        
        private void FireProjectile()
        {
            if (ObjectPool.Instance == null)
            {
                Debug.LogWarning("[WeaponController] ObjectPool not found! Cannot spawn projectile.");
                return;
            }
            
            Vector3 aimDirection = GetAimDirection();
            Quaternion rotation = Quaternion.LookRotation(aimDirection);
            
            ObjectPool.Instance.Spawn(_projectilePoolTag, _muzzlePoint.position, rotation);
        }
        
        #endregion
        
        private Vector3 GetAimDirection()
        {
            if (_playerCamera != null)
            {
                // Aim toward center of screen (crosshair)
                Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                
                // Raycast to find actual aim point
                if (Physics.Raycast(ray, out RaycastHit hit, _range, _hitLayers))
                {
                    return (hit.point - _muzzlePoint.position).normalized;
                }
                else
                {
                    Vector3 targetPoint = ray.GetPoint(_range);
                    return (targetPoint - _muzzlePoint.position).normalized;
                }
            }
            
            return _muzzlePoint.forward;
        }
        
        private void PlayFireEffects()
        {
            // Muzzle flash
            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }
            
            // Sound
            if (_audioSource != null && _fireSound != null)
            {
                _audioSource.PlayOneShot(_fireSound);
            }
        }
        
        #region Public Properties
        
        public FireMode CurrentFireMode
        {
            get => _fireMode;
            set => _fireMode = value;
        }
        
        public float FireRate
        {
            get => _fireRate;
            set => _fireRate = Mathf.Max(0.1f, value);
        }
        
        public float Damage
        {
            get => _damage;
            set => _damage = value;
        }
        
        public bool IsFiring => _isFiring;
        
        #endregion
    }
}
