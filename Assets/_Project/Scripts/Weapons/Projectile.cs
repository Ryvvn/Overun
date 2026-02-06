using UnityEngine;
using Overun.Core;
using Overun.Combat;

namespace Overun.Weapons
{
    /// <summary>
    /// Projectile behavior - moves forward and destroys on collision or timeout.
    /// Implements IPoolable for object pool integration.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 40f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private ElementType _elementType = ElementType.None;
        
        private Rigidbody _rigidbody;
        private float _spawnTime;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        private void Update()
        {
            // Check lifetime
            if (Time.time - _spawnTime >= _lifetime)
            {
                ReturnToPool();
            }
        }
        
        private void FixedUpdate()
        {
            // Move forward at constant speed
            _rigidbody.velocity = transform.forward * _speed;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Ignore player layer
            if (other.CompareTag("Player")) return;
            
            // Try to damage enemy
            var enemy = other.GetComponent<Enemies.Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage, _elementType);
                Debug.Log($"[Projectile] Hit {other.name} for {_damage} damage");
                StatusEffectManager _statusEffectManager = enemy.GetComponent<StatusEffectManager>();
                ElementalComboSystem.Instance.CheckForCombo(_statusEffectManager, _elementType, _damage);
            }
            
            // Try to damage barrel
            var barrel = other.GetComponent<Environment.ExplosiveBarrel>();
            if (barrel != null)
            {
                barrel.TakeDamage(_damage);
            }
            
            // Return to pool
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Despawn(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        #region IPoolable
        
        public void OnSpawnFromPool()
        {
            _spawnTime = Time.time;
            _rigidbody.velocity = Vector3.zero;
        }
        
        public void OnDespawnToPool()
        {
            _rigidbody.velocity = Vector3.zero;
        }
        
        #endregion
        
        #region Public Properties
        
        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }
        
        public float Damage
        {
            get => _damage;
            set => _damage = value;
        }
        
        public ElementType ElementType
        {
            get => _elementType;
            set => _elementType = value;
        }
        #endregion
    }
}
