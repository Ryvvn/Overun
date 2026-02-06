using UnityEngine;
using System;
using System.Collections;

namespace Overun.Waves
{
    /// <summary>
    /// Manages wave progression, spawning timing, and wave state.
    /// Central hub for wave-related events and queries.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private WaveConfig _waveConfig;
        
        [Header("State")]
        [SerializeField] private int _currentWaveNumber = 0;
        [SerializeField] private bool _isWaveActive = false;
        [SerializeField] private bool _isSpawning = false;
        
        // Current wave data
        private WaveData _currentWave;
        
        // Events
        public event Action<int> OnWaveStarting;         // Wave number about to start
        public event Action<WaveData> OnWaveStarted;     // Wave fully started
        public event Action<WaveData> OnWaveCompleted;   // All enemies killed
        public event Action OnAllWavesCompleted;         // Victory!
        public event Action<int, int> OnEnemyKilled;     // killed count, total count
        public event Action OnReadyToSpawn;              // Signal to spawn next enemy
        
        // Properties
        public WaveConfig Config => _waveConfig;
        public int CurrentWaveNumber => _currentWaveNumber;
        public WaveData CurrentWave => _currentWave;
        public bool IsWaveActive => _isWaveActive;
        public bool IsSpawning => _isSpawning;
        public bool HasWon => _currentWaveNumber > (_waveConfig?.MaxWaves ?? 20);
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            if (_waveConfig == null)
            {
                Debug.LogError("[WaveManager] No WaveConfig assigned!");
                return;
            }
            
            // Subscribe to Shop events if ShopManager exists
            if (Overun.Shop.ShopManager.Instance != null)
            {
                Overun.Shop.ShopManager.Instance.OnShopClosed += OnShopClosed;
            }
            
            // Auto-start first wave or wait for external trigger
            StartNextWave();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            if (Overun.Shop.ShopManager.Instance != null)
            {
                Overun.Shop.ShopManager.Instance.OnShopClosed -= OnShopClosed;
            }
        }
        
        /// <summary>
        /// Start the next wave. Call this to begin wave 1 or advance after wave completion.
        /// </summary>
        public void StartNextWave()
        {
            if (_isWaveActive)
            {
                Debug.LogWarning("[WaveManager] Wave already active!");
                return;
            }
            
            _currentWaveNumber++;
            
            // Check for victory
            if (_isEndlessMode == false && _currentWaveNumber > _waveConfig.MaxWaves)
            {
                Debug.Log("[WaveManager] All waves completed! Victory!");
                OnAllWavesCompleted?.Invoke();
                return;
            }
            
            // Create wave data
            _currentWave = new WaveData(_currentWaveNumber, _waveConfig);
            
            Debug.Log($"[WaveManager] Starting {_currentWave}");
            OnWaveStarting?.Invoke(_currentWaveNumber);
            
            StartCoroutine(WaveSequence());
        }
        
        private IEnumerator WaveSequence()
        {
            _isWaveActive = true;
            
            // Initial delay before spawning
            yield return new WaitForSeconds(_waveConfig.WaveStartDelay);
            
            OnWaveStarted?.Invoke(_currentWave);
            
            // Start spawning enemies
            _isSpawning = true;
            StartCoroutine(SpawnSequence());
        }
        
        private IEnumerator SpawnSequence()
        {
            while (!_currentWave.AllSpawned)
            {
                // Signal that we're ready to spawn
                OnReadyToSpawn?.Invoke();
                _currentWave.OnEnemySpawned();
                
                yield return new WaitForSeconds(_waveConfig.SpawnInterval);
            }
            
            _isSpawning = false;
            Debug.Log($"[WaveManager] All enemies spawned for wave {_currentWaveNumber}");
        }
        
        /// <summary>
        /// Call this when an enemy is killed. Tracks wave progress.
        /// </summary>
        public void RegisterEnemyKill()
        {
            if (_currentWave == null) return;
            
            _currentWave.OnEnemyKilled();
            
            OnEnemyKilled?.Invoke(_currentWave.EnemiesKilled, _currentWave.EnemyCount);
            
            Debug.Log($"[WaveManager] Enemy killed. {_currentWave}");
            
            // Check wave completion
            if (_currentWave.IsComplete)
            {
                CompleteWave();
            }
        }
        
        private void OnShopClosed()
        {
            // Shop closed, move to next wave
            Debug.Log("[WaveManager] Shop closed. Starting next wave sequence.");
            StartCoroutine(StartingNextWaveSequence());
        }
        
        private IEnumerator StartingNextWaveSequence()
        {
            // Optional delay after shop closes before wave assumes control
            yield return new WaitForSeconds(1f);
            StartNextWave();
        }

        private void CompleteWave()
        {
            _isWaveActive = false;
            _isSpawning = false;
            
            Debug.Log($"[WaveManager] Wave {_currentWaveNumber} complete!");
            OnWaveCompleted?.Invoke(_currentWave);
            
            // Open Shop instead of direct auto-start
            if (Shop.ShopManager.Instance != null)
            {
                Shop.ShopManager.Instance.OpenShop();
            }
            else
            {
                Debug.LogWarning("[WaveManager] ShopManager missing! Auto-advancing directly.");
                StartCoroutine(AutoStartNextWave());
            }
        }
        
        private IEnumerator AutoStartNextWave()
        {
            yield return new WaitForSeconds(_waveConfig.TimeBetweenWaves);
            StartNextWave();
        }
        
        /// <summary>
        /// Manually trigger next wave start (for shop/UI control).
        /// </summary>
        public void ContinueToNextWave()
        {
            if (!_isWaveActive)
            {
                StartNextWave();
            }
        }
        
        /// <summary>
        /// Enter endless mode: Increases max waves or removes limit to allow continuation.
        /// </summary>
        public void EnterEndlessMode()
        {
            // Simple way: Increase max waves significantly
            // Or set a flag to ignore max waves
            // Let's modify config instance at runtime (dirty but works) 
            // OR ignore the check.
            
            // Better: Set a flag "EndlessMode"
            _isEndlessMode = true;
            
            // Resume game
            StartNextWave();
        }

        private bool _isEndlessMode = false;
        
        /// <summary>
        /// Reset to wave 0 (for restart/new run).
        /// </summary>
        public void ResetWaves()
        {
            StopAllCoroutines();
            _currentWaveNumber = 0;
            _currentWave = null;
            _isWaveActive = false;
            _isSpawning = false;
            _isEndlessMode = false;
        }
        
        /// <summary>
        /// Get formatted wave display text for UI.
        /// </summary>
        public string GetWaveDisplayText()
        {
            if (_currentWave == null)
            {
                return "Wave 0";
            }
            
            string text = $"Wave {_currentWaveNumber}";
            if (_currentWave.IsBossWave)
            {
                text += " - BOSS";
            }
            return text;
        }
        
        /// <summary>
        /// Get formatted progress text.
        /// </summary>
        public string GetProgressText()
        {
            if (_currentWave == null) return "0/0";
            return $"{_currentWave.EnemiesKilled}/{_currentWave.EnemyCount}";
        }
    }
}
