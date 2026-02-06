using UnityEngine;

namespace Overun.Currency
{
    /// <summary>
    /// Collectible gold item dropped by enemies.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GoldPickup : MonoBehaviour
    {
        [Header("Values")]
        [SerializeField] private int _goldValue = 5;
        
        [Header("Visual")]
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.3f;
        [SerializeField] private float _rotateSpeed = 90f;
        
        [Header("Magnet")]
        [SerializeField] private float _magnetSpeed = 10f;
        
        private Vector3 _startPosition;
        private Transform _targetPlayer;
        private bool _isCollecting = false;
        
        private void Update()
        {
            if (_isCollecting && _targetPlayer != null)
            {
                // Fly to player
                transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, _magnetSpeed * Time.deltaTime);
                
                if (Vector3.Distance(transform.position, _targetPlayer.position) < 0.5f)
                {
                    Collect();
                }
            }
            else
            {
                // Idle animation
                float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
                transform.position = _startPosition + Vector3.up * yOffset;
                transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Called by PlayerMagnet to pull this item.
        /// </summary>
        public void PullToward(Transform target)
        {
            if (_isCollecting) return;
            
            _targetPlayer = target;
            _isCollecting = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            // Instant collect on collision
            Collect();
        }
        
        private void Collect()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddGold(_goldValue);
            }
            
            // Should play SFX/VFX here
            Destroy(gameObject);
        }
        
        public void Initialize(int value, Vector3 startPos)
        {
            _goldValue = value;
            _startPosition = startPos;
        }
    }
}
