using UnityEngine;
using Overun.Combat;

namespace Overun.UI
{
    /// <summary>
    /// Spawns damage numbers when enemies take damage.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }
        
        [Header("Prefab")]
        [SerializeField] private GameObject _damageNumberPrefab;
        
        [Header("Settings")]
        [SerializeField] private float _spawnHeight = 1.5f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _criticalColor = Color.yellow;
        [SerializeField] private Color _healColor = Color.green;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            Overun.Enemies.Enemy.OnAnyEnemyDamaged += HandleEnemyDamaged;
            Overun.Enemies.EnemyResistance.OnResistTextSpawned += HandleResistTextSpawned;
        }

        private void OnDisable()
        {
            Overun.Enemies.Enemy.OnAnyEnemyDamaged -= HandleEnemyDamaged;
            Overun.Enemies.EnemyResistance.OnResistTextSpawned -= HandleResistTextSpawned;
        }

        private void HandleEnemyDamaged(Vector3 position, float amount, ElementType element)
        {
            if (element != ElementType.None)
            {
                SpawnElementalDamage(position, amount, element);
            }
            else
            {
                SpawnDamageNumber(position, amount);
            }
        }

        private void HandleResistTextSpawned(Vector3 position, string text)
        {
            SpawnResistText(position, text);
        }

        private void SpawnResistText(Vector3 position, string text)
        {
            if (_damageNumberPrefab == null) return;
            
            Vector3 spawnPos = position + Vector3.up * _spawnHeight;
            spawnPos += Random.insideUnitSphere * 0.3f;
            
            GameObject obj = Instantiate(_damageNumberPrefab, spawnPos, Quaternion.identity);
            DamageNumber dn = obj.GetComponent<DamageNumber>();
            
            if (dn != null)
            {
                dn.InitializeResistText(text);
            }
        }       
        
        /// <summary>
        /// Spawn a damage number at position.
        /// </summary>
        public void SpawnDamageNumber(Vector3 position, float damage, bool isCritical = false)
        {
            SpawnNumber(position, damage, isCritical ? _criticalColor : _normalColor, isCritical);
        }
        
        /// <summary>
        /// Spawn an elemental damage number.
        /// </summary>
        public void SpawnElementalDamage(Vector3 position, float damage, ElementType element)
        {
            if (_damageNumberPrefab == null) return;
            
            Vector3 spawnPos = position + Vector3.up * _spawnHeight;
            spawnPos += Random.insideUnitSphere * 0.3f;
            
            GameObject obj = Instantiate(_damageNumberPrefab, spawnPos, Quaternion.identity);
            DamageNumber dn = obj.GetComponent<DamageNumber>();
            
            if (dn != null)
            {
                dn.InitializeElemental(damage, element);
            }
        }
        
        /// <summary>
        /// Spawn a heal number.
        /// </summary>
        public void SpawnHealNumber(Vector3 position, float amount)
        {
            SpawnNumber(position, amount, _healColor, false);
        }
        
        private void SpawnNumber(Vector3 position, float value, Color color, bool isCritical)
        {
            if (_damageNumberPrefab == null) return;
            
            Vector3 spawnPos = position + Vector3.up * _spawnHeight;
            spawnPos += Random.insideUnitSphere * 0.3f;
            
            GameObject obj = Instantiate(_damageNumberPrefab, spawnPos, Quaternion.identity);
            DamageNumber dn = obj.GetComponent<DamageNumber>();
            
            if (dn != null)
            {
                dn.Initialize(value, color, isCritical);
            }
        }
    }
}
