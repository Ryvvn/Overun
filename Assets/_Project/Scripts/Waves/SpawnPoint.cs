using UnityEngine;

namespace Overun.Waves
{
    /// <summary>
    /// Marks a position in the scene where enemies can spawn.
    /// Add multiple SpawnPoints around the arena for variety.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _spawnRadius = 0.5f;
        [SerializeField] private Color _gizmoColor = Color.red;
        [SerializeField] private bool _isActive = true;
        
        public bool IsActive => _isActive;
        
        /// <summary>
        /// Get a random position within the spawn radius.
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
            return transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = _isActive ? _gizmoColor : Color.gray;
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
            
            // Draw direction arrow
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, _spawnRadius);
        }
    }
}
