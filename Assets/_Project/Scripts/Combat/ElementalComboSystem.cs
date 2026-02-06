using UnityEngine;
using System.Collections.Generic;
using Overun.Core;

namespace Overun.Combat
{
    /// <summary>
    /// Handles elemental combo reactions when two elements interact.
    /// </summary>
    public class ElementalComboSystem : MonoBehaviour
    {
        public static ElementalComboSystem Instance { get; private set; }
        
        [Header("Combo Settings")]
        [SerializeField] private float _comboDamageMultiplier = 2f;
        [SerializeField] private float _comboRadius = 4f;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = true;
        
        // Events
        public event System.Action<ComboType, Vector3> OnComboTriggered;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Check for combo when applying new element to target with existing effect.
        /// </summary>
        public void CheckForCombo(StatusEffectManager target, ElementType newElement, float baseDamage)
        {
            if (target == null) return;
            
            Vector3 position = target.transform.position;
            
            // Fire + Ice = Steam Explosion
            if ((target.HasEffect(ElementType.Fire) && newElement == ElementType.Ice) ||
                (target.HasEffect(ElementType.Ice) && newElement == ElementType.Fire))
            {
                TriggerCombo(ComboType.SteamExplosion, position, baseDamage);
            }
            
            // Fire + Poison = Toxic Cloud
            if ((target.HasEffect(ElementType.Fire) && newElement == ElementType.Poison) ||
                (target.HasEffect(ElementType.Poison) && newElement == ElementType.Fire))
            {
                TriggerCombo(ComboType.ToxicCloud, position, baseDamage);
            }
            
            // Ice + Lightning = Shatter
            if ((target.HasEffect(ElementType.Ice) && newElement == ElementType.Lightning) ||
                (target.HasEffect(ElementType.Lightning) && newElement == ElementType.Ice))
            {
                TriggerCombo(ComboType.Shatter, position, baseDamage);
            }
            
            // Lightning + Poison = Electro-Poison
            if ((target.HasEffect(ElementType.Lightning) && newElement == ElementType.Poison) ||
                (target.HasEffect(ElementType.Poison) && newElement == ElementType.Lightning))
            {
                TriggerCombo(ComboType.ElectroPoison, position, baseDamage);
            }
        }
        
        private void TriggerCombo(ComboType combo, Vector3 position, float baseDamage)
        {
            float comboDamage = baseDamage * _comboDamageMultiplier;
            
            if (_showDebugLog)
                Debug.Log($"[Combo] {combo} triggered! Damage: {comboDamage}");
            
            OnComboTriggered?.Invoke(combo, position);
            
            switch (combo)
            {
                case ComboType.SteamExplosion:
                    // AOE damage + knockback
                    DealAreaDamage(position, comboDamage, _comboRadius);
                    break;
                    
                case ComboType.ToxicCloud:
                    // Lingering poison AOE
                    SpawnToxicCloud(position, baseDamage * 0.5f);
                    break;
                    
                case ComboType.Shatter:
                    // High single-target damage + stun
                    DealAreaDamage(position, comboDamage * 1.5f, _comboRadius * 0.5f);
                    break;
                    
                case ComboType.ElectroPoison:
                    // Chain poison to nearby
                    ChainPoisonEffect(position, baseDamage);
                    break;
            }
        }
        
        private void DealAreaDamage(Vector3 center, float damage, float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            
            foreach (Collider col in colliders)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    // Damage falloff based on distance
                    float dist = Vector3.Distance(center, damageable.transform.position);
                    float falloff = 1f - (dist / radius);
                    damageable.TakeDamage(damage * Mathf.Max(0.3f, falloff));
                }
            }
        }
        
        private void SpawnToxicCloud(Vector3 position, float damagePerTick)
        {
            // Would spawn a lingering area effect
            // For MVP, just deal instant AOE poison
            Collider[] colliders = Physics.OverlapSphere(position, _comboRadius);
            
            foreach (Collider col in colliders)
            {
                StatusEffectManager effects = col.GetComponent<StatusEffectManager>();
                if (effects != null)
                {
                    ElementalDamage poison = new ElementalDamage(ElementType.Poison, damagePerTick * 3f, 5f);
                    effects.ApplyEffect(poison);
                }
            }
        }
        
        private void ChainPoisonEffect(Vector3 position, float baseDamage)
        {
            Collider[] colliders = Physics.OverlapSphere(position, _comboRadius * 1.5f);
            
            foreach (Collider col in colliders)
            {
                StatusEffectManager effects = col.GetComponent<StatusEffectManager>();
                if (effects != null)
                {
                    ElementalDamage poison = new ElementalDamage(ElementType.Poison, baseDamage * 0.5f, 4f);
                    effects.ApplyEffect(poison);
                }
            }
        }
    }
    
    public enum ComboType
    {
        SteamExplosion,     // Fire + Ice
        ToxicCloud,         // Fire + Poison
        Shatter,            // Ice + Lightning
        ElectroPoison       // Lightning + Poison
    }
}
