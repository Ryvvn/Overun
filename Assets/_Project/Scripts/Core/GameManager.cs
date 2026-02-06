using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Overun.Core
{
    /// <summary>
    /// Manages game state including playing, paused, and death states.
    /// Singleton pattern for global access.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        public enum GameState
        {
            Playing,
            Paused,
            Dead,
            GameOver
        }
        
        [Header("Settings")]
        [SerializeField] private bool _pauseTimeOnDeath = true;
        [SerializeField] private float _deathSlowMotionScale = 0.1f;
        [SerializeField] private float _slowMotionDuration = 0.5f;
        
        private GameState _currentState = GameState.Playing;
        private float _originalTimeScale = 1f;
        
        // Events
        public event Action<GameState> OnGameStateChanged;
        
        public GameState CurrentState => _currentState;
        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool IsDead => _currentState == GameState.Dead;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            _originalTimeScale = Time.timeScale;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            
            _currentState = newState;
            OnGameStateChanged?.Invoke(_currentState);
            
            Debug.Log($"[GameManager] State changed to: {_currentState}");
        }
        
        public void OnPlayerDeath()
        {
            if (_currentState == GameState.Dead) return;
            
            SetState(GameState.Dead);
            
            if (_pauseTimeOnDeath)
            {
                // Slow motion effect then pause
                StartCoroutine(DeathSlowMotionSequence());
            }
        }
        
        private System.Collections.IEnumerator DeathSlowMotionSequence()
        {
            // Slow motion phase
            Time.timeScale = _deathSlowMotionScale;
            Time.fixedDeltaTime = 0.02f * _deathSlowMotionScale;
            
            yield return new WaitForSecondsRealtime(_slowMotionDuration);
            
            // Pause completely
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
        }
        
        public void RestartGame()
        {
            // Reset time scale
            Time.timeScale = _originalTimeScale;
            Time.fixedDeltaTime = 0.02f;
            
            // Reload current scene
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
            
            Debug.Log("[GameManager] Restarting game...");
        }
        
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;
            
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }
        
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            
            SetState(GameState.Playing);
            Time.timeScale = _originalTimeScale;
        }
        public void LoadMainMenu()
        {
            Time.timeScale = 1f; // Ensure time is running
            Time.fixedDeltaTime = 0.02f;
            
            // Assuming Build Index 0 is MainMenu
            SceneManager.LoadScene(0); 
            SetState(GameState.Playing); // Reset state just in case
            
            // Show cursor for menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            
            // Assuming Build Index 1 is Game
            SceneManager.LoadScene(1);
            SetState(GameState.Playing);
            
            // Hide cursor for gameplay (PlayerController usually handles this too)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
