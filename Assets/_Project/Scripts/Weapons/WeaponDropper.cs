using UnityEngine;
using System.Collections.Generic;
using Overun.Enemies;

namespace Overun.Weapons
{
    /// <summary>
    /// Spawns weapon pickups when enemies die.
    /// Attach to enemy, or use static method from EnemySpawner.
    /// </summary>
    public class WeaponDropper : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] private float _dropChance = 0.3f; // 30% chance
        [SerializeField] private GameObject _pickupPrefab;
        
        [Header("Weapon Pool")]
        [SerializeField] private List<WeaponDropEntry> _weaponPool = new List<WeaponDropEntry>();
        
        [Header("Height")]
        [SerializeField] private float _dropHeight = 0.5f;
        
        private Enemy _enemy;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }
        
        private void OnEnable()
        {
            if (_enemy != null)
            {
                _enemy.OnDeath += TrySpawnDrop;
            }
        }
        
        private void OnDisable()
        {
            if (_enemy != null)
            {
                _enemy.OnDeath -= TrySpawnDrop;
            }
        }
        
        private void TrySpawnDrop()
        {
            if (Random.value > _dropChance) return;
            
            WeaponData weapon = SelectRandomWeapon();
            if (weapon == null) return;
            
            SpawnPickup(weapon, transform.position);
        }
        
        private WeaponData SelectRandomWeapon()
        {
            if (_weaponPool.Count == 0) return null;
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var entry in _weaponPool)
            {
                totalWeight += entry.weight;
            }
            
            // Pick random based on weight
            float random = Random.Range(0f, totalWeight);
            float current = 0f;
            
            foreach (var entry in _weaponPool)
            {
                current += entry.weight;
                if (random <= current)
                {
                    return entry.weapon;
                }
            }
            
            return _weaponPool[0].weapon;
        }
        
        public void SpawnPickup(WeaponData data, Vector3 position)
        {
            if (_pickupPrefab == null)
            {
                Debug.LogWarning("[WeaponDropper] No pickup prefab assigned!");
                return;
            }
            
            Vector3 spawnPos = position + Vector3.up * _dropHeight;
            GameObject pickup = Instantiate(_pickupPrefab, spawnPos, Quaternion.identity);
            
            WeaponPickup pickupComponent = pickup.GetComponent<WeaponPickup>();
            if (pickupComponent != null)
            {
                pickupComponent.Initialize(data);
            }
            
            Debug.Log($"[WeaponDropper] Spawned {data.WeaponName} pickup!");
        }
    }
    
    [System.Serializable]
    public class WeaponDropEntry
    {
        public WeaponData weapon;
        [Range(0f, 100f)]
        public float weight = 1f;
    }
}
