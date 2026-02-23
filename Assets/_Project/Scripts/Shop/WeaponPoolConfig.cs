using UnityEngine;
using System.Collections.Generic;
using Overun.Weapons;

namespace Overun.Shop
{
    /// <summary>
    /// Defines a weapon pool for the shop with rarity-weighted selection.
    /// Supports duplicate prevention and separate glitch pool logic.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponPool", menuName = "Overun/Weapon Pool Config")]
    public class WeaponPoolConfig : ScriptableObject
    {
        [Header("Weapon Pool")]
        [Tooltip("All weapons available in the shop pool")]
        [SerializeField] private List<WeaponData> _weapons = new List<WeaponData>();
        
        [Header("Rarity Weights")]
        [Tooltip("Drop chance weight for Common weapons")]
        [SerializeField] private float _commonWeight = 40f;
        [SerializeField] private float _uncommonWeight = 30f;
        [SerializeField] private float _rareWeight = 20f;
        [SerializeField] private float _epicWeight = 8f;
        [SerializeField] private float _legendaryWeight = 2f;
        
        public List<WeaponData> AllWeapons => _weapons;
        public int Count => _weapons.Count;
        
        /// <summary>
        /// Get the weight for a given rarity tier.
        /// </summary>
        public float GetRarityWeight(WeaponRarity rarity)
        {
            return rarity switch
            {
                WeaponRarity.Common => _commonWeight,
                WeaponRarity.Uncommon => _uncommonWeight,
                WeaponRarity.Rare => _rareWeight,
                WeaponRarity.Epic => _epicWeight,
                WeaponRarity.Legendary => _legendaryWeight,
                _ => _commonWeight
            };
        }
        
        /// <summary>
        /// Select a random weapon using rarity-weighted probability.
        /// Excludes weapons in the provided set to prevent duplicates.
        /// </summary>
        public WeaponData SelectWeaponWeighted(HashSet<WeaponData> excludeSet = null)
        {
            if (_weapons.Count == 0) return null;
            
            // Build filtered list and weights
            List<WeaponData> candidates = new List<WeaponData>();
            List<float> weights = new List<float>();
            float totalWeight = 0f;
            
            foreach (var weapon in _weapons)
            {
                if (excludeSet != null && excludeSet.Contains(weapon)) continue;
                
                float w = GetRarityWeight(weapon.Rarity);
                candidates.Add(weapon);
                weights.Add(w);
                totalWeight += w;
            }
            
            // Fallback: if all excluded, pick from full pool
            if (candidates.Count == 0)
            {
                candidates.AddRange(_weapons);
                totalWeight = 0f;
                weights.Clear();
                foreach (var weapon in _weapons)
                {
                    float w = GetRarityWeight(weapon.Rarity);
                    weights.Add(w);
                    totalWeight += w;
                }
            }
            
            // Weighted random selection
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return candidates[i];
                }
            }
            
            // Safety fallback
            return candidates[candidates.Count - 1];
        }
        
        /// <summary>
        /// Select a random weapon with EQUAL weight (for Glitch slot).
        /// </summary>
        public WeaponData SelectWeaponUniform()
        {
            if (_weapons.Count == 0) return null;
            return _weapons[Random.Range(0, _weapons.Count)];
        }
    }
}
