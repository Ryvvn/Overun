using UnityEngine;
using System.Collections.Generic;
using Overun.Weapons;
using Overun.Enemies;

namespace Overun.Player
{
    /// <summary>
    /// Orchestrates floating weapon visuals around the player.
    /// Subscribes to WeaponInventory events and manages orbital positioning.
    /// </summary>
    public class FloatingWeaponController : MonoBehaviour
    {
        [Header("Orbit Settings")]
        [SerializeField] private float _orbitRadius = 1.8f;
        [SerializeField] private float _orbitHeight = 0.5f;
        [SerializeField] private float _orbitRotationSpeed = 15f; // degrees per second for slow orbit
        
        [Header("Enemy Detection")]
        [SerializeField] private LayerMask _enemyLayers;
        [SerializeField] private float _enemyDetectionInterval = 0.25f;
        
        [Header("References")]
        [SerializeField] private WeaponInventory _inventory;
        [SerializeField] private GameObject _floatingWeaponPrefab;
        
        // Runtime
        private readonly List<FloatingWeaponVisual> _activeVisuals = new List<FloatingWeaponVisual>(6);
        private readonly List<Enemy> _nearbyEnemies = new List<Enemy>(32);
        private float _orbitAngleOffset;
        private float _enemyDetectionTimer;
        
        private void Awake()
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<WeaponInventory>();
            }
            
            if (_inventory == null)
            {
                Debug.LogError("[FloatingWeaponController] No WeaponInventory found!");
                enabled = false;
            }
        }
        
        private void OnEnable()
        {
            if (_inventory == null) return;
            
            _inventory.OnWeaponAdded += HandleWeaponAdded;
            _inventory.OnWeaponStacked += HandleWeaponStacked;
            _inventory.OnWeaponSelected += HandleWeaponSelected;
            _inventory.OnInventoryChanged += HandleInventoryChanged;
        }
        
        private void OnDisable()
        {
            if (_inventory == null) return;
            
            _inventory.OnWeaponAdded -= HandleWeaponAdded;
            _inventory.OnWeaponStacked -= HandleWeaponStacked;
            _inventory.OnWeaponSelected -= HandleWeaponSelected;
            _inventory.OnInventoryChanged -= HandleInventoryChanged;
        }
        
        private void Update()
        {
            // Slowly rotate the orbit ring
            _orbitAngleOffset += _orbitRotationSpeed * Time.deltaTime;
            if (_orbitAngleOffset >= 360f) _orbitAngleOffset -= 360f;
            
            // Update orbital positions
            UpdateOrbitalPositions();
            
            // Periodic enemy detection
            _enemyDetectionTimer -= Time.deltaTime;
            if (_enemyDetectionTimer <= 0f)
            {
                _enemyDetectionTimer = _enemyDetectionInterval;
                FindNearbyEnemies();
            }
            
            // Update weapon facing
            UpdateWeaponFacing();
        }
        
        #region Event Handlers
        
        private void HandleWeaponAdded(WeaponInstance weapon)
        {
            SpawnFloatingVisual(weapon);
            RecalculateOrbitalPositions();
            UpdateSelectedEmphasis();
        }
        
        private void HandleWeaponStacked(WeaponInstance weapon)
        {
            // Stacking doesn't add/remove visuals, just a feedback moment
            // Could trigger a scale pulse in the future
        }
        
        private void HandleWeaponSelected(int selectedIndex)
        {
            UpdateSelectedEmphasis();
        }
        
        private void HandleInventoryChanged()
        {
            SyncVisualsToInventory();
            RecalculateOrbitalPositions();
            UpdateSelectedEmphasis();
        }
        
        #endregion
        
        #region Visual Management
        
        private void SpawnFloatingVisual(WeaponInstance weapon)
        {
            GameObject visualGO;
            
            if (_floatingWeaponPrefab != null)
            {
                visualGO = Instantiate(_floatingWeaponPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                visualGO = new GameObject($"FloatingWeapon_{weapon.Data.WeaponName}");
            }
            
            // Ensure component exists
            var visual = visualGO.GetComponent<FloatingWeaponVisual>();
            if (visual == null)
            {
                visual = visualGO.AddComponent<FloatingWeaponVisual>();
            }
            
            // Phase offset: spread evenly + slight random for organic feel
            float phaseOffset = (_activeVisuals.Count * Mathf.PI * 2f / 6f) + Random.Range(0f, 0.5f);
            visual.Initialize(weapon, transform, phaseOffset);
            _activeVisuals.Add(visual);
        }
        
        private void SyncVisualsToInventory()
        {
            var inventoryWeapons = _inventory.Weapons;
            
            // Remove visuals for weapons no longer in inventory
            for (int i = _activeVisuals.Count - 1; i >= 0; i--)
            {
                if (_activeVisuals[i] == null || !inventoryWeapons.Contains(_activeVisuals[i].WeaponInstance))
                {
                    if (_activeVisuals[i] != null)
                    {
                        Destroy(_activeVisuals[i].gameObject);
                    }
                    _activeVisuals.RemoveAt(i);
                }
            }
            
            // Add visuals for new weapons
            foreach (var weapon in inventoryWeapons)
            {
                bool found = false;
                for (int i = 0; i < _activeVisuals.Count; i++)
                {
                    if (_activeVisuals[i].WeaponInstance == weapon)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    SpawnFloatingVisual(weapon);
                }
            }
        }
        
        #endregion
        
        #region Orbital Positioning
        
        private void RecalculateOrbitalPositions()
        {
            UpdateOrbitalPositions();
        }
        
        private void UpdateOrbitalPositions()
        {
            int count = _activeVisuals.Count;
            if (count == 0) return;
            
            float angleStep = 360f / count;
            
            for (int i = 0; i < count; i++)
            {
                if (_activeVisuals[i] == null) continue;
                
                float angle = (angleStep * i + _orbitAngleOffset) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * _orbitRadius,
                    _orbitHeight,
                    Mathf.Sin(angle) * _orbitRadius
                );
                
                _activeVisuals[i].SetTargetPosition(offset);
            }
        }
        
        private void UpdateSelectedEmphasis()
        {
            int selectedIndex = _inventory.SelectedIndex;
            var inventoryWeapons = _inventory.Weapons;
            
            for (int i = 0; i < _activeVisuals.Count; i++)
            {
                if (_activeVisuals[i] == null) continue;
                
                // Find if this visual's weapon matches the selected weapon
                bool isSelected = false;
                for (int j = 0; j < inventoryWeapons.Count; j++)
                {
                    if (inventoryWeapons[j] == _activeVisuals[i].WeaponInstance && j == selectedIndex)
                    {
                        isSelected = true;
                        break;
                    }
                }
                
                _activeVisuals[i].SetSelected(isSelected);
            }
        }
        
        #endregion
        
        #region Enemy Detection & Facing
        
        private void FindNearbyEnemies()
        {
            _nearbyEnemies.Clear();
            
            float maxRange = 15f; // Default detection range
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
        
        private void UpdateWeaponFacing()
        {
            var inventoryWeapons = _inventory.Weapons;
            int selectedIndex = _inventory.SelectedIndex;
            
            for (int i = 0; i < _activeVisuals.Count; i++)
            {
                if (_activeVisuals[i] == null) continue;
                
                // Find inventory index for this visual
                int inventoryIndex = -1;
                for (int j = 0; j < inventoryWeapons.Count; j++)
                {
                    if (inventoryWeapons[j] == _activeVisuals[i].WeaponInstance)
                    {
                        inventoryIndex = j;
                        break;
                    }
                }
                
                // Selected weapon faces player forward
                if (inventoryIndex == selectedIndex)
                {
                    _activeVisuals[i].FaceForward();
                    continue;
                }
                
                // Non-selected weapons face nearest enemy (auto-fire facing)
                float autoRange = _activeVisuals[i].WeaponInstance?.Data?.AutoFireRange ?? 15f;
                Enemy closest = GetClosestEnemyInRange(autoRange);
                
                if (closest != null)
                {
                    _activeVisuals[i].FaceTarget(closest.transform.position);
                }
                else
                {
                    _activeVisuals[i].FaceForward();
                }
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
        
        private void OnDestroy()
        {
            // Clean up all floating visuals
            for (int i = _activeVisuals.Count - 1; i >= 0; i--)
            {
                if (_activeVisuals[i] != null)
                {
                    Destroy(_activeVisuals[i].gameObject);
                }
            }
            _activeVisuals.Clear();
        }
    }
}
