using UnityEngine;
using System;

namespace Overun.Player
{
    /// <summary>
    /// Manages player health, damage, healing, and death.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;
        
        [Header("Invincibility")]
        [SerializeField] private float _invincibilityDuration = 0.5f;
        private float _lastDamageTime;
        
        // Events
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDeath;
        public event Action OnDamaged;
        public event Action OnHealed;
        
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float HealthPercent => _currentHealth / _maxHealth;
        public bool IsDead => _currentHealth <= 0f;
        public bool IsInvincible => Time.time - _lastDamageTime < _invincibilityDuration;
        
        private void Awake()
        {
            _currentHealth = _maxHealth;
        }
        
        private void Start()
        {
            // Notify UI of initial health
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (IsInvincible) return;
            if (amount <= 0f) return;
            
            _lastDamageTime = Time.time;
            
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamaged?.Invoke();
            
            Debug.Log($"[PlayerHealth] Took {amount} damage. Health: {_currentHealth}/{_maxHealth}");
            
            if (_currentHealth <= 0f)
            {
                Die();
            }
        }
        
        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;
            
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            
            if (_currentHealth > previousHealth)
            {
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
                OnHealed?.Invoke();
                
                Debug.Log($"[PlayerHealth] Healed {amount}. Health: {_currentHealth}/{_maxHealth}");
            }
        }
        
        public void SetHealth(float amount)
        {
            _currentHealth = Mathf.Clamp(amount, 0f, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void SetMaxHealth(float newMax, bool healToFull = false)
        {
            _maxHealth = Mathf.Max(1f, newMax);
            
            if (healToFull)
            {
                _currentHealth = _maxHealth;
            }
            else
            {
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            }
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        private void Die()
        {
            Debug.Log("[PlayerHealth] Player died!");
            OnDeath?.Invoke();
        }
        
        // For testing in editor
        [ContextMenu("Take 10 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(10f);
        }
        
        [ContextMenu("Heal 10")]
        private void DebugHeal()
        {
            Heal(10f);
        }
    }
}
