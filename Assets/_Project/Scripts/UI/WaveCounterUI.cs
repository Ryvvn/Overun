using UnityEngine;
using TMPro;
using Overun.Waves;

namespace Overun.UI
{
    /// <summary>
    /// Displays the current wave number and enemy count.
    /// </summary>
    public class WaveCounterUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _enemyCountText;
        
        [Header("Display Settings")]
        [SerializeField] private string _waveFormat = "WAVE {0}";
        [SerializeField] private string _bossWaveFormat = "WAVE {0} - BOSS";
        [SerializeField] private string _enemyFormat = "Enemies: {0}/{1}";
        
        [Header("Animation")]
        [SerializeField] private bool _animateOnWaveStart = true;
        [SerializeField] private float _waveStartScalePunch = 1.3f;
        [SerializeField] private float _animationDuration = 0.3f;
        
        private Vector3 _originalWaveTextScale;
        
        private void Awake()
        {
            if (_waveText != null)
            {
                _originalWaveTextScale = _waveText.transform.localScale;
            }
        }
        
        private void Start()
        {
            // Subscribe to WaveManager events
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += OnWaveStarted;
                WaveManager.Instance.OnEnemyKilled += OnEnemyKilled;
                WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
                WaveManager.Instance.OnAllWavesCompleted += OnAllWavesCompleted;
                
                // Initialize display
                UpdateWaveDisplay(0, false);
                UpdateEnemyCount(0, 0);
            }
            else
            {
                Debug.LogWarning("[WaveCounterUI] WaveManager not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted -= OnWaveStarted;
                WaveManager.Instance.OnEnemyKilled -= OnEnemyKilled;
                WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
                WaveManager.Instance.OnAllWavesCompleted -= OnAllWavesCompleted;
            }
        }
        
        private void OnWaveStarted(WaveData wave)
        {
            UpdateWaveDisplay(wave.WaveNumber, wave.IsBossWave);
            UpdateEnemyCount(0, wave.EnemyCount);
            
            if (_animateOnWaveStart)
            {
                StartCoroutine(AnimateWaveText());
            }
        }
        
        private void OnEnemyKilled(int killed, int total)
        {
            UpdateEnemyCount(killed, total);
        }
        
        private void OnWaveCompleted(WaveData wave)
        {
            // Could show "Wave Complete!" text here
        }
        
        private void OnAllWavesCompleted()
        {
            if (_waveText != null)
            {
                _waveText.text = "VICTORY!";
            }
            if (_enemyCountText != null)
            {
                _enemyCountText.text = "";
            }
        }
        
        private void UpdateWaveDisplay(int waveNumber, bool isBossWave)
        {
            if (_waveText == null) return;
            
            string format = isBossWave ? _bossWaveFormat : _waveFormat;
            _waveText.text = string.Format(format, waveNumber);
        }
        
        private void UpdateEnemyCount(int killed, int total)
        {
            if (_enemyCountText == null) return;
            
            _enemyCountText.text = string.Format(_enemyFormat, killed, total);
        }
        
        private System.Collections.IEnumerator AnimateWaveText()
        {
            if (_waveText == null) yield break;
            
            Transform textTransform = _waveText.transform;
            float elapsed = 0f;
            
            // Scale up
            while (elapsed < _animationDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (_animationDuration / 2f);
                textTransform.localScale = Vector3.Lerp(_originalWaveTextScale, 
                    _originalWaveTextScale * _waveStartScalePunch, t);
                yield return null;
            }
            
            // Scale back down
            elapsed = 0f;
            while (elapsed < _animationDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (_animationDuration / 2f);
                textTransform.localScale = Vector3.Lerp(_originalWaveTextScale * _waveStartScalePunch, 
                    _originalWaveTextScale, t);
                yield return null;
            }
            
            textTransform.localScale = _originalWaveTextScale;
        }
        
        /// <summary>
        /// Manually set display for testing.
        /// </summary>
        public void SetDisplay(int wave, int killed, int total, bool isBoss = false)
        {
            UpdateWaveDisplay(wave, isBoss);
            UpdateEnemyCount(killed, total);
        }
    }
}
