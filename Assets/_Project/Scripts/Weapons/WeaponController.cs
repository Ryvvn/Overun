using UnityEngine;
using UnityEngine.InputSystem;
using Overun.Core;
using Overun.Player;
using Overun.Enemies;
using Overun.Combat;
using System.Collections;
using System.Collections.Generic;

namespace Overun.Weapons
{
    /// <summary>
    /// Handles weapon firing: manual for selected, auto for others.
    /// Replaces basic shooting with multi-weapon system.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponInventory _inventory;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Camera _playerCamera;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private TrailRenderer _bulletTrailPrefab;
        [SerializeField] private GameObject _hitEffectPrefab;
        
        [Header("Auto-Fire Settings")]
        [SerializeField] private float _autoFireCheckInterval = 0.2f;
        [SerializeField] private LayerMask _enemyLayers;
        
        [Header("Input")]
        private PlayerInputActions _playerInput;
        
        private InputAction _fireAction;
        private InputAction _scroll1Action;
        private InputAction _weaponSelectAction;
        
        private bool _isFiring = false;
        private float _lastAutoFireCheck;
        private List<Enemy> _nearbyEnemies = new List<Enemy>();

        private int _shotCount = 0;
        private float _currentSpreadAngle;
        
        private void Awake()
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<WeaponInventory>();
            }
            
            if (_playerCamera == null)
            {
                _playerCamera = Camera.main;
            }
            
              if (_playerInput == null)
            {
                _playerInput = new PlayerInputActions();
            }
        }
        
        private void OnEnable()
        {
            _playerInput.Player.Enable();
            _playerInput.Player.Fire.performed += OnFireStarted;
            _playerInput.Player.Fire.canceled += OnFireCanceled;    
            _playerInput.Player.Scroll.performed += OnScroll;

           
        }
        
        private void OnDisable()
        {
            _playerInput.Player.Fire.performed -= OnFireStarted;
            _playerInput.Player.Fire.canceled -= OnFireCanceled;
            _playerInput.Player.Scroll.performed -= OnScroll;
            _playerInput.Player.Disable();
        }
        
        private void Update()
        {
            // Manual fire for selected weapon
            if (_isFiring)
            {
                TryManualFire();
                _shotCount++;
            }
            
            // Auto-fire for secondary weapons
            if (Time.time - _lastAutoFireCheck > _autoFireCheckInterval)
            {
                FindNearbyEnemies();
                _lastAutoFireCheck = Time.time;
            }
            
            TryAutoFire();
            
            // Number key weapon selection
            HandleNumberKeySelection();
        }
        
        private void HandleNumberKeySelection()
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    _inventory.SelectWeapon(i - 1);
                }
            }
        }
        
        private void OnFireStarted(InputAction.CallbackContext context)
        {
            _isFiring = true;
        }
        
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            _isFiring = false;
            _shotCount = 0;
        }
        
        private void OnScroll(InputAction.CallbackContext context)
        {
            Vector2 scroll = context.ReadValue<Vector2>();
            if (scroll.y < 0)
            {
                _inventory.SelectNextWeapon();
            }
            else if (scroll.y > 0)
            {
                _inventory.SelectPreviousWeapon();
            }
        }
        
        private void OnWeaponSelect(InputAction.CallbackContext context)
        {
            // 1-6 keys via input system
            int index = Mathf.RoundToInt(context.ReadValue<float>()) - 1;
            if (index >= 0 && index < 6)
            {
                _inventory.SelectWeapon(index);
            }
        }
        
        #region Manual Fire
        
        private void TryManualFire()
        {
            WeaponInstance weapon = _inventory.SelectedWeapon;
            if (weapon == null) return;
            if (!weapon.CanFire()) return;
            
            // Get aim direction from mouse
            Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPoint;
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                targetPoint = hit.point;
                Debug.DrawLine(_firePoint.position, targetPoint, Color.blue, 1f);  
            }
            else    
            {
                targetPoint = ray.GetPoint(100f);
                Debug.DrawLine(_firePoint.position, targetPoint, Color.blue, 1f);  
            }
            
            Vector3 direction = (targetPoint - _firePoint.position).normalized;
            direction.Normalize();
            FireWeapon(weapon, direction);
        }
        
        #endregion
        
        #region Auto Fire
        
        private void FindNearbyEnemies()
        {
            _nearbyEnemies.Clear();
            
            float maxRange = GetMaxAutoFireRange();
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxRange, _enemyLayers);
            
            foreach (Collider col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    _nearbyEnemies.Add(enemy);
                }
            }
        }
        
        private float GetMaxAutoFireRange()
        {
            float maxRange = 0f;
            foreach (var weapon in _inventory.GetSecondaryWeapons())
            {
                if (weapon.Data.AutoFireRange > maxRange)
                {
                    maxRange = weapon.Data.AutoFireRange;
                }
            }
            return maxRange > 0 ? maxRange : 15f;
        }
        
        private void TryAutoFire()
        {
            if (_nearbyEnemies.Count == 0) return;
            
            List<WeaponInstance> secondaryWeapons = _inventory.GetSecondaryWeapons();
            if (secondaryWeapons.Count == 0) return;
            
            foreach (var weapon in secondaryWeapons)
            {
                if (!weapon.CanAutoFire()) continue;
                
                // Find closest enemy in range
                Enemy target = GetClosestEnemyInRange(weapon.Data.AutoFireRange);
                if (target == null) continue;
                
                Vector3 direction = (target.transform.position - _firePoint.position).normalized;
                FireWeapon(weapon, direction);
            }
        }
        
        private Enemy GetClosestEnemyInRange(float range)
        {
            Enemy closest = null;
            float closestDist = float.MaxValue;
            
            foreach (var enemy in _nearbyEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist <= range && dist < closestDist)
                {
                    closest = enemy;
                    closestDist = dist;
                }
            }
            
            return closest;
        }
        
        #endregion
        
        #region Firing Core
        
        private void FireWeapon(WeaponInstance weapon, Vector3 direction)
        {
            WeaponData data = weapon.Data;
            if(_shotCount > 0)
            {
                _currentSpreadAngle = Mathf.Clamp(_currentSpreadAngle + data.SpreadIncreasePerShot, 0, data.SpreadAngle);
            }
            else
            {
                _currentSpreadAngle = 0;
            }

            for (int i = 0; i < data.ProjectilesPerShot; i++)
            {
                Vector3 fireDirection = ApplySpread(direction, _currentSpreadAngle);
                if (data.IsHitScan)
                {
                    ShootHitScan(weapon, fireDirection);
                }
                else
                {
                    SpawnProjectile(weapon, fireDirection);
                }
            }
            
            weapon.RecordFire();
            PlayFireVFX();
        }
        
        private void ShootHitScan(WeaponInstance weapon, Vector3 direction)
        {
            float maxRange = 100f;
            RaycastHit hit;
            
            // Raycast from fire point in the spread direction
            if (Physics.Raycast(_firePoint.position, direction, out hit, maxRange, _enemyLayers))
            {
                Debug.DrawLine(_firePoint.position, hit.point, Color.green, 1f);
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(weapon.Damage, weapon.Data.Element);
                    StatusEffectManager _statusEffectManager = enemy.GetComponent<StatusEffectManager>();
                    if (_statusEffectManager != null)
                    {
                        ElementalComboSystem.Instance.CheckForCombo(_statusEffectManager,
                        weapon.Data.Element,
                        weapon.Damage);
                    }
                }
                var barrel = hit.collider.GetComponent<Environment.ExplosiveBarrel>();
                if (barrel != null)
                {
                    barrel.TakeDamage(weapon.Damage);
                }
                
                // VFX: Hit effect at impact point
                SpawnHitEffect(hit.point, hit.normal);
                
                // VFX: Bullet trail to hit point
                SpawnBulletTrail(_firePoint.position, hit.point);
            }
            else
            {
                // Draw miss ray
                Debug.DrawRay(_firePoint.position, direction * maxRange, Color.red, 1f);
                
                // VFX: Bullet trail to max range
                SpawnBulletTrail(_firePoint.position, _firePoint.position + direction * maxRange);
            }
        }
        
        private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
        {
            if (spreadAngle <= 0f) return direction;
            // Apply spread in a cone (both X and Y)
            float xSpread = Random.Range(-spreadAngle, spreadAngle);
            float ySpread = Random.Range(-spreadAngle, spreadAngle);
            
            Quaternion rotation = Quaternion.Euler(xSpread, ySpread, 0f);
            
            // Rotate the direction vector locally
            return rotation * direction;
        }
        
        private void SpawnProjectile(WeaponInstance weapon, Vector3 direction)
        {
            WeaponData data = weapon.Data;
            
            GameObject projectileObj;
            
            // Try object pool
            if (ObjectPool.Instance != null && data.ProjectilePrefab != null)
            {
                projectileObj = ObjectPool.Instance.Spawn(data.ProjectilePoolTag, _firePoint.position, Quaternion.LookRotation(direction));
            }
            else if (data.ProjectilePrefab != null)
            {
                projectileObj = Instantiate(data.ProjectilePrefab, _firePoint.position, Quaternion.LookRotation(direction));
            }
            else
            {
                Debug.LogWarning($"[WeaponController] No projectile prefab for {data.WeaponName}!");
                return;
            }
            
            // Configure projectile
            if(projectileObj)
            {
                Projectile projectile = projectileObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Speed = data.ProjectileSpeed;
                    projectile.Damage = weapon.Damage; // Uses stack-adjusted damage
                    projectile.ElementType = data.Element;
                }
            }
        }
        
        #endregion
        
        #region VFX
        
        private void PlayFireVFX()
        {
            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }
        }
        
        private void SpawnHitEffect(Vector3 point, Vector3 normal)
        {
            if (_hitEffectPrefab == null) return;
            GameObject fx = Instantiate(_hitEffectPrefab, point, Quaternion.LookRotation(normal));
            Destroy(fx, 2f);
        }
        
        private void SpawnBulletTrail(Vector3 start, Vector3 end)
        {
            if (_bulletTrailPrefab == null) return;
            TrailRenderer trail = Instantiate(_bulletTrailPrefab, start, Quaternion.identity);
            StartCoroutine(AnimateBulletTrail(trail, start, end));
        }
        
        private IEnumerator AnimateBulletTrail(TrailRenderer trail, Vector3 start, Vector3 end)
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
            
            // Wait for trail to fade then destroy
            yield return new WaitForSeconds(trail.time);
            Destroy(trail.gameObject);
        }
        
        #endregion
    }
}
