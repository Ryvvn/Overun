using UnityEngine;
using System.Collections.Generic;
using Overun.Combat;
using System;

namespace Overun.Enemies
{
    public enum ResistanceType
    {
        Normal,
        Weak,       // 1.5x damage
        Resistant,  // 0.5x damage
        Immune      // 0.0x damage
    }

    [System.Serializable]
    public struct ElementResistanceEntry
    {
        public ElementType Element;
        public ResistanceType Resistance;
    }

    public class EnemyResistance : MonoBehaviour
    {
        [SerializeField] private List<ElementResistanceEntry> _resistances = new List<ElementResistanceEntry>();
        
        private Dictionary<ElementType, ResistanceType> _resistanceMap = new Dictionary<ElementType, ResistanceType>();
        
        public static event Action<Vector3, string> OnResistTextSpawned;

        private void Awake()
        {
            // Build dictionary for fast lookup
            foreach (var entry in _resistances)
            {
                if (!_resistanceMap.ContainsKey(entry.Element))
                {
                    _resistanceMap.Add(entry.Element, entry.Resistance);
                }
            }
        }
        
        public float ModifyDamage(float damage, ElementType element)
        {
            if (element == ElementType.None) return damage;
            if (!_resistanceMap.TryGetValue(element, out ResistanceType type)) return damage;
            
            switch (type)
            {
                case ResistanceType.Weak:
                    // Spawn "Weak!" text if needed
                    return damage * 1.5f;
                    
                case ResistanceType.Resistant:
                    // Spawn "Resist" text
                    SpawnResistText("Resist");
                    return damage * 0.5f;
                    
                case ResistanceType.Immune:
                    // Spawn "Immune" text
                    SpawnResistText("Immune");
                    return 0f;
                    
                default:
                    return damage;
            }
        }
        
        public bool IsImmune(ElementType element)
        {
            if (element == ElementType.None) return false;
            if (_resistanceMap.TryGetValue(element, out ResistanceType type))
            {
                return type == ResistanceType.Immune;
            }
            return false;
        }

        private void SpawnResistText(string text)
        {
             // TODO: Add support for text popups in DamageNumberSpawner
             OnResistTextSpawned?.Invoke(transform.position, text);
             // For now we rely on 0 damage showing or console
             Debug.Log($"[EnemyResistance] {text}!");
        }
    }
}
