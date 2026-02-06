using UnityEngine;

namespace Overun.Core
{
    /// <summary>
    /// Auto-disables GameObject after a set time.
    /// Useful for VFX that should disappear.
    /// </summary>
    public class AutoDisableAfterTime : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 1f;
        
        private float _startTime;
        
        private void OnEnable()
        {
            _startTime = Time.time;
        }
        
        private void Update()
        {
            if (Time.time - _startTime >= _lifetime)
            {
                // Try to return to pool first
                if (ObjectPool.Instance != null)
                {
                    ObjectPool.Instance.Despawn(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        
        public float Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }
    }
}
