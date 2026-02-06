using UnityEngine;

namespace Overun.Combat
{
    /// <summary>
    /// Interface for anything that can take damage.
    /// Used to break cyclic dependencies between Combat and Enemies assemblies.
    /// </summary>
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        Transform transform { get; }
        
        void TakeDamage(float amount, ElementType element = ElementType.None);
    }
    
    /// <summary>
    /// Interface for entities that can have status effects.
    /// </summary>
    public interface IStatusEffectTarget
    {
        bool HasStatusEffect(int effectType);
        void ApplyStatusEffect(int effectType, float damage, float duration, float strength);
    }

    /// <summary>
    /// Interface for entities that can be stunned.
    /// </summary>
    public interface IStunnable
    {
        void Stun(float duration);
    }
}
