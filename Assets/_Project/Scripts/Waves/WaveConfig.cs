using UnityEngine;
using System;

namespace Overun.Waves
{
    /// <summary>
    /// ScriptableObject defining wave configuration parameters.
    /// Designers can create multiple configs for different difficulty modes.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Overun/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Wave Count")]
        [Tooltip("Total waves to complete for victory (default: 20)")]
        [SerializeField] private int _maxWaves = 20;
        
        [Header("Enemy Count")]
        [Tooltip("Starting number of enemies in wave 1")]
        [SerializeField] private int _baseEnemyCount = 5;
        
        [Tooltip("Percentage increase per wave (0.1 = 10%, 0.15 = 15%)")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _enemyScalingPerWave = 0.12f;
        
        [Tooltip("Maximum enemies per wave (performance cap)")]
        [SerializeField] private int _maxEnemiesPerWave = 50;
        
        [Header("Timing")]
        [Tooltip("Seconds between waves")]
        [SerializeField] private float _timeBetweenWaves = 5f;
        
        [Tooltip("Delay between spawning each enemy in a wave")]
        [SerializeField] private float _spawnInterval = 0.5f;
        
        [Tooltip("Time before first enemy spawns in a wave")]
        [SerializeField] private float _waveStartDelay = 1f;

        [Tooltip("Wave number when runner enemies start appearing")]
        [SerializeField] private int _runnerStartWave = 4;
        
        [Tooltip("Wave number when tanker enemies start appearing")]
        [SerializeField] private int _tankerStartWave = 7;

        [Tooltip("Wave number when ranged enemies start appearing")]
        [SerializeField] private int _rangedStartWave = 7;

        [Header("Boss Waves")]
        [Tooltip("Boss spawns every X waves (e.g., 5 = waves 5, 10, 15, 20)")]
        [SerializeField] private int _bossWaveInterval = 5;
        
        [Tooltip("Additional enemies on boss waves")]
        [SerializeField] private int _bossWaveExtraEnemies = 3;
        
        [Header("Elite Enemies")]
        [Tooltip("Wave number when elite enemies start appearing")]
        [SerializeField] private int _eliteStartWave = 5;
        
        [Tooltip("Chance for an enemy to be elite (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _eliteChance = 0.05f;
        
        // Properties
        public int MaxWaves => _maxWaves;
        public int BaseEnemyCount => _baseEnemyCount;
        public float EnemyScalingPerWave => _enemyScalingPerWave;
        public int MaxEnemiesPerWave => _maxEnemiesPerWave;
        public float TimeBetweenWaves => _timeBetweenWaves;
        public float SpawnInterval => _spawnInterval;
        public float WaveStartDelay => _waveStartDelay;
        public int BossWaveInterval => _bossWaveInterval;
        public int BossWaveExtraEnemies => _bossWaveExtraEnemies;
        public int EliteStartWave => _eliteStartWave;
        public float EliteChance => _eliteChance;
        public int RunnerStartWave => _runnerStartWave;
        public int TankerStartWave => _tankerStartWave;
        public int RangedStartWave => _rangedStartWave;
        
        /// <summary>
        /// Calculate enemy count for a specific wave number.
        /// </summary>
        public int GetEnemyCountForWave(int waveNumber)
        {
            if (waveNumber < 1) return _baseEnemyCount;
            
            // Base + scaling: enemies = base * (1 + scaling)^(wave-1)
            float scaledCount = _baseEnemyCount * Mathf.Pow(1f + _enemyScalingPerWave, waveNumber - 1);
            int enemyCount = Mathf.RoundToInt(scaledCount);
            
            // Add extra enemies on boss waves
            if (IsBossWave(waveNumber))
            {
                enemyCount += _bossWaveExtraEnemies;
            }
            
            return Mathf.Min(enemyCount, _maxEnemiesPerWave);
        }
        
        public bool IsBossWave(int waveNumber)
        {
            return _bossWaveInterval > 0 && waveNumber % _bossWaveInterval == 0;
        }
        
        /// <summary>
        /// Check if elite enemies can spawn on this wave.
        /// </summary>
        public bool CanSpawnElites(int waveNumber)
        {
            return waveNumber >= _eliteStartWave;
        }
        
        /// <summary>
        /// Check if an enemy should be elite based on wave and RNG.
        /// </summary>
        public bool ShouldSpawnElite(int waveNumber)
        {
            if (!CanSpawnElites(waveNumber)) return false;
            return UnityEngine.Random.value < _eliteChance;
        }
    }
}

