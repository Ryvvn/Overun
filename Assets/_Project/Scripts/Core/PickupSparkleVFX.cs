using UnityEngine;

namespace Overun.Core
{
    /// <summary>
    /// Generic sparkle/glow VFX for any pickup (gold, weapons, items).
    /// Emits continuous sparkle particles while active and a burst on collection.
    /// Attach to any pickup prefab — no assembly dependency on Currency or Weapons.
    /// </summary>
    public class PickupSparkleVFX : MonoBehaviour
    {
        [Header("Idle Sparkle")]
        [SerializeField] private Color _sparkleColor = new Color(1f, 0.85f, 0.2f, 1f); // Gold
        [SerializeField] private float _emissionRate = 5f;
        [SerializeField] private float _sparkleSize = 0.1f;
        
        [Header("Collection Burst")]
        [SerializeField] private int _burstCount = 15;
        [SerializeField] private float _burstSpeed = 2f;
        
        private ParticleSystem _particleSystem;
        
        private void Awake()
        {
            CreateSparkleSystem();
        }
        
        /// <summary>
        /// Call this when the pickup is collected to play burst and schedule cleanup.
        /// </summary>
        public void PlayCollectionBurst()
        {
            if (_particleSystem == null) return;
            
            // Stop continuous emission
            var emission = _particleSystem.emission;
            emission.rateOverTime = 0f;
            
            // Burst
            _particleSystem.Emit(_burstCount);
            
            // Destroy after particles fade
            Destroy(gameObject, _particleSystem.main.startLifetime.constant + 0.1f);
        }
        
        private void CreateSparkleSystem()
        {
            _particleSystem = gameObject.AddComponent<ParticleSystem>();
            
            var main = _particleSystem.main;
            main.startLifetime = 0.8f;
            main.startSpeed = _burstSpeed * 0.3f; // Gentle float for idle
            main.startSize = _sparkleSize;
            main.startColor = _sparkleColor;
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f; // Float upward slightly
            main.loop = true;
            
            var emission = _particleSystem.emission;
            emission.rateOverTime = _emissionRate;
            
            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            
            // Size over lifetime — shrink
            var size = _particleSystem.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            
            // Color over lifetime — fade out
            var color = _particleSystem.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(_sparkleColor, 0f), new GradientColorKey(_sparkleColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            color.color = gradient;
        }
    }
}
