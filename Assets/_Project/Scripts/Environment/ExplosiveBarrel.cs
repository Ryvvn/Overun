using UnityEngine;

namespace Overun.Environment
{
    /// <summary>
    /// Destructible barrel that explodes when shot, damaging nearby enemies.
    /// </summary>
    public class ExplosiveBarrel : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float _health = 30f;
        
        [Header("Explosion")]
        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private float _explosionDamage = 50f;
        [SerializeField] private float _explosionForce = 500f;
        
        [Header("Visual")]
        [SerializeField] private GameObject _explosionEffect;
        [SerializeField] private Color _damageFlashColor = Color.red;
        
        [Header("Layers")]
        [SerializeField] private LayerMask _damageLayers;
        
        private Renderer _renderer;
        private Color _originalColor;
        private bool _isDestroyed = false;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
        }
        
        public void TakeDamage(float amount)
        {
            if (_isDestroyed) return;
            
            _health -= amount;
            
            // Flash on damage
            if (_renderer != null)
            {
                StartCoroutine(DamageFlash());
            }
            
            Debug.Log($"[ExplosiveBarrel] Took {amount} damage. Health: {_health}");
            
            if (_health <= 0f)
            {
                Explode();
            }
        }
        
        private void Explode()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;
            
            Debug.Log($"[ExplosiveBarrel] Exploding!");
            
            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _damageLayers);
            
            foreach (Collider col in colliders)
            {
                // Damage enemies
                var enemy = col.GetComponent<Enemies.Enemy>();
                if (enemy != null)
                {
                    // Calculate damage falloff based on distance
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float damageMultiplier = 1f - (distance / _explosionRadius);
                    float damage = _explosionDamage * Mathf.Max(0.25f, damageMultiplier);
                    
                    enemy.TakeDamage(damage);
                    Debug.Log($"[ExplosiveBarrel] Dealt {damage} damage to {col.name}");
                }
                
                
                // Apply explosion force to rigidbodies
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
                }

                // Check other explosive barrels
                var barrel = col.GetComponent<ExplosiveBarrel>();
                if (barrel != null && barrel != this)
                {   
                    barrel.TakeDamage(_explosionDamage);
                }
            }
            
            // Spawn explosion effect
            if (_explosionEffect != null)
            {
                Instantiate(_explosionEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy barrel
            Destroy(gameObject);
        }
        
        private System.Collections.IEnumerator DamageFlash()
        {
            if (_renderer == null) yield break;
            
            _renderer.material.color = _damageFlashColor;
            yield return new WaitForSeconds(0.1f);
            _renderer.material.color = _originalColor;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _explosionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
