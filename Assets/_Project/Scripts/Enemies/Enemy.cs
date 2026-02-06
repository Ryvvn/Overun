using UnityEngine;
using System;
using Overun.Core;
using Overun.Combat;

namespace Overun.Enemies
{
    /// <summary>
    /// Base enemy class with health, damage, and death handling.
    /// </summary>
    public class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected float _maxHealth = 50f;
        [SerializeField] protected float _currentHealth;
        [SerializeField] protected float _contactDamage = 10f;
        [SerializeField] protected float _moveSpeed = 3f;
        
        [Header("Type")]
        [SerializeField] protected EnemyType _enemyType = EnemyType.Basic;
        
        // Events
        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDamaged;
        
        // Properties
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float HealthPercent => _currentHealth / _maxHealth;
        public bool IsDead => _currentHealth <= 0f;
        public float ContactDamage => _contactDamage;
        public float MoveSpeed => _moveSpeed;
        public EnemyType Type => _enemyType;

        // Global event for UI/Systems to listen to
        public static event Action<Vector3, float, ElementType> OnAnyEnemyDamaged;
        private StatusEffectManager _statusEffectManager;
        private EnemyResistance _resistanceManager;
        
        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
            _statusEffectManager = GetComponent<StatusEffectManager>();
            _resistanceManager = GetComponent<EnemyResistance>();
        }

        public virtual void TakeDamage(float amount, ElementType element = ElementType.None)
        {
            if (IsDead) return;
            
            // Check Resistance
            if (_resistanceManager != null && element != ElementType.None)
            {
                amount = _resistanceManager.ModifyDamage(amount, element);
            }
            
            if (amount <= 0f) return;
            
            _currentHealth -= amount;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamaged?.Invoke();
            
            // Invoke global event instead of direct UI call
            OnAnyEnemyDamaged?.Invoke(transform.position, amount, element);
            
            Debug.Log($"[Enemy] Took {amount} damage. Health: {_currentHealth}/{_maxHealth}");
            
            // Take elemental damage (Status Effects)
            if (element != ElementType.None)
            {   
                float duration = 3;
                float strength = 1;
                _statusEffectManager.ApplyEffect(new ElementalDamage(element, amount, duration, strength));
            }

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            Debug.Log($"[Enemy] {gameObject.name} died!");
            
            OnDeath?.Invoke();
            
            // Destroy after short delay for effects
            Destroy(gameObject, 0.1f);
        }
        
        public void SetStats(float health, float damage, float speed)
        {
            _maxHealth = health;
            _currentHealth = health;
            _contactDamage = damage;
            _moveSpeed = speed;
        }
        
        /// <summary>
        /// Scale stats for elite/boss variants.
        /// </summary>
        public void ApplyEliteMultiplier(float healthMult, float damageMult)
        {
            _maxHealth *= healthMult;
            _currentHealth = _maxHealth;
            _contactDamage *= damageMult;
        }

        /// <summary>
        /// Apply wave difficulty scaling.
        /// </summary>
        public void ApplyDifficultyScaling(float multiplier)
        {
            _maxHealth *= multiplier;
            _currentHealth = _maxHealth;
            _contactDamage *= multiplier;
            
            // Optional: Speed scaling? Usually minimal or flat
            // _moveSpeed *= Mathf.Pow(multiplier, 0.1f); 
        }
    }
    
    public enum EnemyType
    {
        Basic,
        Elite,
        Boss
    }
}
