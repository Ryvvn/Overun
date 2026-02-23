using UnityEngine;
using System.Collections.Generic;
using Overun.Core;

namespace Overun.Combat
{
    /// <summary>
    /// Manages status effects on an entity (enemy or player).
    /// Handles Fire burn, Ice slow, Poison stacks, etc.
    /// </summary>
    public class StatusEffectManager : MonoBehaviour, IStatusEffectTarget
    {
        #region Fields
        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = false;
        
        private Dictionary<ElementType, StatusEffect> _activeEffects = new Dictionary<ElementType, StatusEffect>();
        private IDamageable _damageable;
        
        // Events
        public event System.Action<ElementType> OnEffectApplied;
        public event System.Action<ElementType> OnEffectRemoved;
        public event System.Action<float> OnDamageOverTime; // Damage amount


        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _damageable = GetComponent<IDamageable>();
        }
        
        private void Update()
        {
            ProcessEffects();
        }
        
        #endregion

        #region Effect Processing
        private void ProcessEffects()
        {
            List<ElementType> toRemove = new List<ElementType>();
            
            foreach (var kvp in _activeEffects)
            {
                StatusEffect effect = kvp.Value;
                
                // Check expiration
                if (Time.time >= effect.endTime)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                // Process DoT effects
                if (effect.tickDamage > 0 && Time.time >= effect.nextTickTime)
                {
                    ApplyTickDamage(effect);
                    effect.nextTickTime = Time.time + effect.tickInterval;
                }
            }
            
            // Remove expired effects
            foreach (var element in toRemove)
            {
                RemoveEffect(element);
            }
        }
        
        private void ApplyTickDamage(StatusEffect effect)
        {
            if (_damageable != null && !_damageable.IsDead)
            {
                _damageable.TakeDamage(effect.tickDamage);
                OnDamageOverTime?.Invoke(effect.tickDamage);
                
                if (_showDebugLog)
                {
                    Debug.Log($"[StatusEffect] {effect.element} tick for {effect.tickDamage} damage");
                }
            }
        }
        
        /// <summary>
        /// Apply a status effect from elemental damage.
        /// </summary>
        public void ApplyEffect(ElementalDamage elementalDamage)
        {
            if (elementalDamage.element == ElementType.None) return;
            
            switch (elementalDamage.element)
            {
                case ElementType.Fire:
                    ApplyBurn(elementalDamage);
                    break;
                case ElementType.Ice:
                    ApplySlow(elementalDamage);
                    break;
                case ElementType.Lightning:
                    ApplyChainLightning(elementalDamage);
                    break;
                case ElementType.Poison:
                    ApplyPoison(elementalDamage);
                    break;
            }
        }

        #endregion
        
        #region IStatusEffectTarget Implementation
        
        public bool HasStatusEffect(int effectType)
        {
            return HasEffect((ElementType)effectType);
        }
        
        public void ApplyStatusEffect(int effectType, float damage, float duration, float strength)
        {
            ElementalDamage ed = new ElementalDamage((ElementType)effectType, damage, duration, strength);
            ApplyEffect(ed);
        }
        
        #endregion
        
        #region Fire - Burn DoT
        
        private void ApplyBurn(ElementalDamage damage)
        {
            float chaosMult = Overun.Combat.ChaosModifierSystem.Instance != null 
                ? Overun.Combat.ChaosModifierSystem.Instance.GetFireDamageMultiplier() 
                : 1.0f;
                
            float dps = damage.damage * 0.3f * chaosMult; // Apply modifier
            float duration = damage.effectDuration;
            
            StatusEffect burn = new StatusEffect
            {
                element = ElementType.Fire,
                startTime = Time.time,
                endTime = Time.time + duration,
                tickDamage = dps * 0.5f,
                tickInterval = 0.5f,
                nextTickTime = Time.time + 0.5f,
                stacks = 1
            };
            
            // ... (rest is same)
            
            // Refresh or stack
            if (_activeEffects.ContainsKey(ElementType.Fire))
            {
                var existing = _activeEffects[ElementType.Fire];
                existing.endTime = Mathf.Max(existing.endTime, burn.endTime);
                existing.tickDamage = Mathf.Max(existing.tickDamage, burn.tickDamage);
            }
            else
            {
                _activeEffects[ElementType.Fire] = burn;
                OnEffectApplied?.Invoke(ElementType.Fire);
            }
        }
        
        #endregion
        
        #region Ice - Slow
        
        private void ApplySlow(ElementalDamage damage)
        {
            float chaosMult = Overun.Combat.ChaosModifierSystem.Instance != null 
                ? Overun.Combat.ChaosModifierSystem.Instance.GetIceSlowMultiplier() 
                : 1.0f;
                
            float slowPercent = Mathf.Clamp(damage.effectStrength * 0.3f * chaosMult, 0.1f, 0.9f); // Higher cap for Chaos
            float duration = damage.effectDuration;
            
            StatusEffect slow = new StatusEffect
            {
                // ... same init
                element = ElementType.Ice,
                startTime = Time.time,
                endTime = Time.time + duration,
                slowPercent = slowPercent,
                stacks = 1
            };
            
            if (_activeEffects.ContainsKey(ElementType.Ice))
            {
                var existing = _activeEffects[ElementType.Ice];
                existing.endTime = Mathf.Max(existing.endTime, slow.endTime);
                existing.slowPercent = Mathf.Max(existing.slowPercent, slow.slowPercent);
            }
            else
            {
                _activeEffects[ElementType.Ice] = slow;
                OnEffectApplied?.Invoke(ElementType.Ice);
            }
        }
        
        #endregion
        
        #region Lightning - Chain
        
        private void ApplyChainLightning(ElementalDamage damage)
        {
            int extraChains = Overun.Combat.ChaosModifierSystem.Instance != null 
                ? Overun.Combat.ChaosModifierSystem.Instance.GetLightningChainBonus() 
                : 0;
            
            float chainRange = 5f;
            float chainDamageReduction = 0.7f;
            int maxChains = 3 + extraChains; // Add bonus chains
            
            // ... (rest of logic remains same, just uses updated maxChains)
            Collider[] nearby = Physics.OverlapSphere(transform.position, chainRange);
            int chainCount = 0;
            
            foreach (Collider col in nearby)
            {
                if (chainCount >= maxChains) break;
                if (col.gameObject == gameObject) continue;
                
                IDamageable nearbyTarget = col.GetComponent<IDamageable>();
                if (nearbyTarget != null && !nearbyTarget.IsDead)
                {
                    float chainDamage = damage.damage * Mathf.Pow(chainDamageReduction, chainCount + 1);
                    nearbyTarget.TakeDamage(chainDamage);
                    if(col.GetComponent<IStunnable>() != null)
                    {
                        col.GetComponent<IStunnable>().Stun(0.3f);
                    }
                   
                    chainCount++;
                }
            }
            
            // Brief stun effect
            StatusEffect stun = new StatusEffect
            {
                element = ElementType.Lightning,
                startTime = Time.time,
                endTime = Time.time + 0.3f,
                isStunned = true
            };
            
            _activeEffects[ElementType.Lightning] = stun;
            OnEffectApplied?.Invoke(ElementType.Lightning);
        }
        
        #endregion
        
        #region Poison - Stacking DoT
        
        private void ApplyPoison(ElementalDamage damage)
        {
            float durationMult = Overun.Combat.ChaosModifierSystem.Instance != null 
                ? Overun.Combat.ChaosModifierSystem.Instance.GetPoisonDurationMultiplier() 
                : 1.0f;
                
            float baseDPS = damage.damage * 0.2f;
            float duration = damage.effectDuration * durationMult; // Longer poison
            int maxStacks = 5;
            
            if (_activeEffects.ContainsKey(ElementType.Poison))
            {
                var existing = _activeEffects[ElementType.Poison];
                existing.stacks = Mathf.Min(existing.stacks + 1, maxStacks);
                existing.tickDamage = baseDPS * existing.stacks;
                existing.endTime = Time.time + duration;
            }
            else
            {
                StatusEffect poison = new StatusEffect
                {
                    element = ElementType.Poison,
                    startTime = Time.time,
                    endTime = Time.time + duration,
                    tickDamage = baseDPS,
                    tickInterval = 1f,
                    nextTickTime = Time.time + 1f,
                    stacks = 1
                };
                
                _activeEffects[ElementType.Poison] = poison;
                OnEffectApplied?.Invoke(ElementType.Poison);
            }
        }
        
        #endregion
        
        private void RemoveEffect(ElementType element)
        {
            if (_activeEffects.ContainsKey(element))
            {
                _activeEffects.Remove(element);
                OnEffectRemoved?.Invoke(element);
                
                if (_showDebugLog)
                    Debug.Log($"[StatusEffect] {element} effect expired");
            }
        }
        
        #region Public Helper Methods
        /// <summary>
        /// Check if entity has a specific effect.
        /// </summary>
        public bool HasEffect(ElementType element)
        {
            return _activeEffects.ContainsKey(element);
        }
        
        /// <summary>
        /// Get current slow percentage (0-1).
        /// </summary>
        public float GetSlowPercent()
        {
            if (_activeEffects.TryGetValue(ElementType.Ice, out StatusEffect ice))
            {
                return ice.slowPercent;
            }
            return 0f;
        }
        
        /// <summary>
        /// Check if currently stunned.
        /// </summary>
        public bool IsStunned()
        {
            if (_activeEffects.TryGetValue(ElementType.Lightning, out StatusEffect lightning))
            {
                return lightning.isStunned;
            }
            return false;
        }
        
        /// <summary>
        /// Clear all effects.
        /// </summary>
        public void ClearAllEffects()
        {
            _activeEffects.Clear();
        }
    }
    
    /// <summary>
    /// Runtime data for an active status effect.
    /// </summary>
    public class StatusEffect
    {
        public ElementType element;
        public float startTime;
        public float endTime;
        public float tickDamage;
        public float tickInterval;
        public float nextTickTime;
        public float slowPercent;
        public bool isStunned;
        public int stacks;
        
        public float RemainingTime => Mathf.Max(0, endTime - Time.time);
        public float Duration => endTime - startTime;
        public float Progress => (Time.time - startTime) / Duration;
    }
    #endregion
}