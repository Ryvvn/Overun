using UnityEngine;
using System;
using System.Collections.Generic;

namespace Overun.Core
{
    /// <summary>
    /// Tracks global player statistics and modifiers.
    /// Used by weapons and movement systems to calculate final values.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        public enum StatType
        {
            Damage,
            AttackSpeed,
            MoveSpeed,
            MaxHealth,
            CritChance
        }

        [Header("Base Stat Multipliers")]
        [SerializeField] private float _damageMult = 1f;
        [SerializeField] private float _attackSpeedMult = 1f;
        [SerializeField] private float _moveSpeedMult = 1f;
        [SerializeField] private float _maxHealthMult = 1f;
        [SerializeField] private float _critChance = 0.05f; // Flat value

        public event Action OnStatsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Getters
        public float DamageMultiplier => _damageMult;
        public float AttackSpeedMultiplier => _attackSpeedMult;
        public float MoveSpeedMultiplier => _moveSpeedMult;
        public float MaxHealthMultiplier => _maxHealthMult;
        public float CritChance => _critChance;

        /// <summary>
        /// Apply a permanent modifier to a stat.
        /// </summary>
        public void ApplyModifier(StatType stat, float value)
        {
            switch (stat)
            {
                case StatType.Damage:
                    _damageMult += value;
                    break;
                case StatType.AttackSpeed:
                    _attackSpeedMult += value;
                    break;
                case StatType.MoveSpeed:
                    _moveSpeedMult += value;
                    break;
                case StatType.MaxHealth:
                    _maxHealthMult += value;
                    break;
                case StatType.CritChance:
                    _critChance += value;
                    break;
            }
            
            Debug.Log($"[PlayerStats] Applied {value} to {stat}. New Value: {GetStatValue(stat)}");
            OnStatsChanged?.Invoke();
        }
        
        private float GetStatValue(StatType stat)
        {
            return stat switch
            {
               StatType.Damage => _damageMult,
               StatType.AttackSpeed => _attackSpeedMult,
               StatType.MoveSpeed => _moveSpeedMult,
               StatType.MaxHealth => _maxHealthMult,
               StatType.CritChance => _critChance,
               _ => 0f
            };
        }
    }
}
