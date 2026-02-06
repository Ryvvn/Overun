using UnityEngine;

namespace Overun.Weapons
{
    /// <summary>
    /// Weapon pickup that spawns when enemies die.
    /// Player collects by walking over it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponData _weaponData;
        
        [Header("Visual")]
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.3f;
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private Renderer _glowRenderer;
        
        [Header("Lifetime")]
        [SerializeField] private float _lifetime = 30f;
        [SerializeField] private float _flashTime = 5f;
        
        private Vector3 _startPosition;
        private float _spawnTime;
        private bool _collected = false;
        
        public WeaponData WeaponData => _weaponData;
        
        private void Awake()
        {
            _startPosition = transform.position;
            _spawnTime = Time.time;
            
            // Ensure trigger collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        
        private void Start()
        {
            UpdateVisualColor();
        }
        
        private void Update()
        {
            if (_collected) return;
            
            // Bob up and down
            float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = _startPosition + Vector3.up * yOffset;
            
            // Rotate
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
            
            // Check lifetime
            float age = Time.time - _spawnTime;
            if (age >= _lifetime)
            {
                Destroy(gameObject);
                return;
            }
            
            // Flash when about to expire
            if (age >= _lifetime - _flashTime)
            {
                float flash = Mathf.PingPong(Time.time * 10f, 1f);
                if (_glowRenderer != null)
                {
                    _glowRenderer.enabled = flash > 0.5f;
                }
            }
        }
        
        private void UpdateVisualColor()
        {
            if (_weaponData == null) return;
            
            Color rarityColor = WeaponData.GetRarityColor(_weaponData.Rarity);
            
            if (_glowRenderer != null)
            {
                _glowRenderer.material.color = rarityColor;
                _glowRenderer.material.SetColor("_EmissionColor", rarityColor * 2f);
            }
            else
            {
                // Fallback: color main renderer
                Renderer r = GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    r.material.color = rarityColor;
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;
            
            // Try to give weapon to player
            var inventory = other.GetComponent<WeaponInventory>();
            if (inventory != null)
            {
                bool collected = inventory.TryAddWeapon(_weaponData);
                if (collected)
                {
                    OnCollected();
                }
            }
        }
        
        private void OnCollected()
        {
            _collected = true;
            
            // Play pickup effect/sound here
            Debug.Log($"[WeaponPickup] Collected {_weaponData.WeaponName}!");
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Initialize pickup with weapon data (called by spawner).
        /// </summary>
        public void Initialize(WeaponData data)
        {
            _weaponData = data;
            UpdateVisualColor();
        }
    }
}
