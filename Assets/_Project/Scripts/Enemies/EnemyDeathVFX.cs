using UnityEngine;
using Overun.Combat;

namespace Overun.Enemies
{
    /// <summary>
    /// Spawns death burst particles when an enemy dies.
    /// Tints particles by the active elemental status effect color, or white if none.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyDeathVFX : MonoBehaviour
    {
        [Header("Death VFX")]
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _destroyDelay = 2f;
        
        [Header("Fallback (no prefab)")]
        [SerializeField] private int _burstCount = 20;
        [SerializeField] private float _burstRadius = 0.5f;
        [SerializeField] private float _burstSpeed = 3f;
        
        private Enemy _enemy;
        private StatusEffectManager _statusManager;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _statusManager = GetComponent<StatusEffectManager>();
        }
        
        private void OnEnable()
        {
            if (_enemy != null)
                _enemy.OnDeath += HandleDeath;
        }
        
        private void OnDisable()
        {
            if (_enemy != null)
                _enemy.OnDeath -= HandleDeath;
        }
        
        private void HandleDeath()
        {
            Color tint = GetElementTint();
            
            if (_deathVFXPrefab != null)
            {
                GameObject fx = Instantiate(_deathVFXPrefab, transform.position, Quaternion.identity);
                
                // Tint the particle system
                var ps = fx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = tint;
                    ps.Play();
                }
                
                Destroy(fx, _destroyDelay);
            }
            else
            {
                // Fallback: runtime-generated burst
                SpawnFallbackBurst(tint);
            }
        }
        
        private Color GetElementTint()
        {
            if (_statusManager == null) return Color.white;
            
            // Check active elements in priority order
            if (_statusManager.HasEffect(ElementType.Fire))
                return ElementUtils.GetElementColor(ElementType.Fire);
            if (_statusManager.HasEffect(ElementType.Ice))
                return ElementUtils.GetElementColor(ElementType.Ice);
            if (_statusManager.HasEffect(ElementType.Lightning))
                return ElementUtils.GetElementColor(ElementType.Lightning);
            if (_statusManager.HasEffect(ElementType.Poison))
                return ElementUtils.GetElementColor(ElementType.Poison);
                
            return Color.white;
        }
        
        private void SpawnFallbackBurst(Color color)
        {
            // Create a temporary particle burst at death position
            GameObject burstObj = new GameObject("DeathBurst");
            burstObj.transform.position = transform.position;
            
            var ps = burstObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = _burstSpeed;
            main.startSize = 0.15f;
            main.startColor = color;
            main.maxParticles = _burstCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;
            
            var emission = ps.emission;
            emission.enabled = false; // Only burst, no continuous
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = _burstRadius;
            
            // Fire burst
            ps.Emit(_burstCount);
            
            Destroy(burstObj, _destroyDelay);
        }
    }
}
