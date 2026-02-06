using UnityEngine;
using Overun.Core;
using Overun.Enemies;
using System.Collections.Generic;

namespace Overun.Waves
{
    /// <summary>
    /// Spawns enemies when WaveManager signals.
    /// Picks random spawn points and instantiates enemy prefabs.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject _basicEnemyPrefab;
        [SerializeField] private GameObject _runnerEnemyPrefab;
        [SerializeField] private GameObject _tankEnemyPrefab;
        [SerializeField] private GameObject _rangedEnemyPrefab;
        [SerializeField] private GameObject _eliteEnemyPrefab;
        [SerializeField] private GameObject _bossEnemyPrefab;
        
        [Header("Spawn Points")]
        [SerializeField] private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        [SerializeField] private bool _autoFindSpawnPoints = true;
        
        [Header("Settings")]
        [SerializeField] private Transform _enemyContainer;
        
        private List<GameObject> _activeEnemies = new List<GameObject>();
        
        public List<GameObject> ActiveEnemies => _activeEnemies;
        public int ActiveEnemyCount => _activeEnemies.Count;
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Create container if not assigned
            if (_enemyContainer == null)
            {
                _enemyContainer = new GameObject("Enemies").transform;
            }
            
            // Subscribe to WaveManager events
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnReadyToSpawn += SpawnNextEnemy;
                WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] WaveManager not found! Spawning will not occur.");
            }
        }
        
        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnReadyToSpawn -= SpawnNextEnemy;
                WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
            }
        }

        private void Start()
        {
            if (_autoFindSpawnPoints && _spawnPoints.Count == 0)
            {
                _spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
            }

            // Optional: Auto-start first wave if WaveManager is ready
            if (WaveManager.Instance != null && WaveManager.Instance.CurrentWaveNumber == 0)
            {
                WaveManager.Instance.StartNextWave();
            }
        }
        #endregion
        
        #region Spawning Logic
        /// <summary>
        /// Spawn the next enemy in the wave sequence.
        /// </summary>
        public void SpawnNextEnemy()
        {
            if (WaveManager.Instance == null || WaveManager.Instance.IsWaveActive == false)
            {
                return;
            }
            
            WaveData currentWave = WaveManager.Instance.CurrentWave;
            WaveConfig config = WaveManager.Instance.Config;
            
            if (currentWave == null || config == null)
            {
                return;
            }

            List<GameObject> allowedPrefabs = new List<GameObject>();
            // Check for Boss Wave (simple check, ideally Boss spawns specifically)
            // Implementation note: Boss Spawning usually handled separately, but if we need a random boss replacer:
            // Small chance to replace normal spawn OR handled by separate logic
            if (config.IsBossWave(currentWave.WaveNumber) && Random.value < 0.2f) 
            {
                // For now, keep boss spawning separate or very specific.
                allowedPrefabs.Add(_bossEnemyPrefab);
            }
            
            // 1. Determine allowed types
            allowedPrefabs.Add(_basicEnemyPrefab); // Always allowed
            
            if (currentWave.WaveNumber >= config.RunnerStartWave && _runnerEnemyPrefab != null)
                allowedPrefabs.Add(_runnerEnemyPrefab);
                
            if (currentWave.WaveNumber >= config.TankerStartWave && _tankEnemyPrefab != null)
                allowedPrefabs.Add(_tankEnemyPrefab);
                
            if (currentWave.WaveNumber >= config.RangedStartWave && _rangedEnemyPrefab != null)
                allowedPrefabs.Add(_rangedEnemyPrefab);

            // 2. Pick random type (Simple uniform random for now)
            if (allowedPrefabs.Count == 0) return;
            GameObject selectedPrefab = allowedPrefabs[Random.Range(0, allowedPrefabs.Count)];
            
            // 3. Pick Spawn Point
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;
            
            if (_spawnPoints.Count > 0)
            {   
                SpawnPoint sp = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
                spawnPos = sp.transform.position;
                spawnRot = sp.transform.rotation;
            }
            
            // 4. Instantiate
            GameObject enemyInstance = Instantiate(selectedPrefab, spawnPos, spawnRot, _enemyContainer);
            _activeEnemies.Add(enemyInstance);

            // 5. Subscribe to death event
            var enemyComp = enemyInstance.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                enemyComp.OnDeath += () => OnEnemyDied(enemyInstance);
            }
            
            // 6. Apply Elite Status
            if (config.ShouldSpawnElite(currentWave.WaveNumber))
            {
                ApplyEliteModifiers(enemyInstance);
            }
        }
        
        private void ApplyEliteModifiers(GameObject enemy)
        {
            // Visuals
            enemy.transform.localScale *= 1.5f;
            
            var renderers = enemy.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.material.color = Color.red;
            }
            
            // Stats
            var enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                enemyComp.ApplyEliteMultiplier(2f, 2f);
            }
        }
        
        private void OnEnemyDied(GameObject enemy)
        {
            _activeEnemies.Remove(enemy);
            
            // Notify WaveManager
            WaveManager.Instance?.RegisterEnemyKill();
        }
        
        private void OnWaveCompleted(WaveData wave)
        {
            // Cleanup any stragglers
            CleanupEnemies();
        }
        
        public void CleanupEnemies()
        {
            foreach (GameObject enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _activeEnemies.Clear();
        }
        
        public void AddSpawnPoint(SpawnPoint point)
        {
            if (!_spawnPoints.Contains(point))
            {
                _spawnPoints.Add(point);
            }
        }
    }
    
    #endregion
}
