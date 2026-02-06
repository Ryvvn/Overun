using UnityEngine;
using Overun.Player;

namespace Overun.Enemies
{
    /// <summary>
    /// Simple projectile fired by ranged enemies.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private float _damage;
        private float _speed;
        private float _lifetime = 5f;
        private float _spawnTime;
        
        public void Initialize(float damage, float speed)
        {
            _damage = damage;
            _speed = speed;
            _spawnTime = Time.time;
        }
        
        private void Update()
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
            
            if (Time.time - _spawnTime > _lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(_damage);
                }
                Destroy(gameObject);
            }
            else if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile"))
            {
                // Hit wall/environment
                Destroy(gameObject);
            }
        }
    }
}
