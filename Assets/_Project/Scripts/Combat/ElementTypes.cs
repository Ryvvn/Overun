using UnityEngine;

namespace Overun.Combat
{
    /// <summary>
    /// Elemental types for weapons and effects.
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,       // Burning DoT
        Ice,        // Slow effect
        Lightning,  // Chain to nearby
        Poison      // Stacking DoT
    }
    
    /// <summary>
    /// Static utilities for elemental systems.
    /// </summary>
    public static class ElementUtils
    {
        public static Color GetElementColor(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => new Color(1f, 0.4f, 0.1f),      // Orange
                ElementType.Ice => new Color(0.4f, 0.8f, 1f),       // Cyan
                ElementType.Lightning => new Color(1f, 1f, 0.3f),   // Yellow
                ElementType.Poison => new Color(0.4f, 0.9f, 0.2f),  // Green
                _ => Color.white
            };
        }
        
        public static string GetElementName(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => "Fire",
                ElementType.Ice => "Ice",
                ElementType.Lightning => "Lightning",
                ElementType.Poison => "Poison",
                _ => "None"
            };
        }
    }
    
    /// <summary>
    /// Data for applying elemental damage.
    /// </summary>
    [System.Serializable]
    public class ElementalDamage
    {
        public ElementType element;
        public float damage;
        public float effectDuration = 3f;
        public float effectStrength = 1f; // Multiplier for effect intensity
        
        public ElementalDamage(ElementType type, float dmg, float duration = 3f, float strength = 1f)
        {
            element = type;
            damage = dmg;
            effectDuration = duration;
            effectStrength = strength;
        }
    }
}
