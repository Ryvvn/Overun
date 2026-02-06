using UnityEngine;

namespace Overun.Currency
{
    /// <summary>
    /// Attracts GoldPickups within range.
    /// Attach to Player.
    /// </summary>
    public class PlayerMagnet : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _magnetRadius = 3f;
        [SerializeField] private LayerMask _pickupLayer;
        [SerializeField] private float _checkInterval = 0.2f;

        // Current Radius property for upgrades
        public float MagnetRadius
        {
            get => _magnetRadius;
            set => _magnetRadius = value;
        }

        private float _nextCheckTime;
        private Collider[] _hitBuffer = new Collider[20];

        private void Update()
        {
            if (Time.time >= _nextCheckTime)
            {
                CheckForPickups();
                _nextCheckTime = Time.time + _checkInterval;
            }
        }

        private void CheckForPickups()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, _magnetRadius, _hitBuffer, _pickupLayer);
            
            for (int i = 0; i < hits; i++)
            {
                var pickup = _hitBuffer[i].GetComponent<GoldPickup>();
                if (pickup != null)
                {
                    pickup.PullToward(transform);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _magnetRadius);
        }
    }
}
