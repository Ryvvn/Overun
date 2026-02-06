using UnityEngine;
using Overun.Enemies;

namespace Overun.Currency
{
    /// <summary>
    /// Spawns gold when enemy dies.
    /// Can be used alongside WeaponDropper for hybrid drops.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class GoldDropper : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] private GameObject _goldPrefab;
        [Tooltip("Chance to drop gold (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _dropChance = 1.0f; // Guaranteed by default
        
        [Header("Values")]
        [SerializeField] private int _minValue = 1;
        [SerializeField] private int _maxValue = 3;
        
        private Enemy _enemy;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }
        
        private void OnEnable()
        {
            if (_enemy != null)
            {
                _enemy.OnDeath += OnDeath;
            }
        }
        
        private void OnDisable()
        {
            if (_enemy != null)
            {
                _enemy.OnDeath -= OnDeath;
            }
        }
        
        private void OnDeath()
        {
            if (Random.value > _dropChance) return;
            if (_goldPrefab == null) return;
            
            int amount = Random.Range(_minValue, _maxValue + 1);
            
            // Spawn gold at enemy position
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject goldObj = Instantiate(_goldPrefab, spawnPos, Quaternion.identity);
            
            GoldPickup pickup = goldObj.GetComponent<GoldPickup>();
            if (pickup != null)
            {
                pickup.Initialize(amount, spawnPos);
            }
        }
    }
}
