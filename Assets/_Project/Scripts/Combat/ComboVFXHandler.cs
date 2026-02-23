using UnityEngine;

namespace Overun.Combat
{
    /// <summary>
    /// Spawns VFX when elemental combos trigger.
    /// Subscribes to ElementalComboSystem.OnComboTriggered.
    /// </summary>
    public class ComboVFXHandler : MonoBehaviour
    {
        [Header("Combo VFX Prefabs")]
        [SerializeField] private GameObject _steamExplosionVFX;
        [SerializeField] private GameObject _shatterVFX;
        [SerializeField] private GameObject _toxicCloudVFX;
        [SerializeField] private GameObject _electroPoisonVFX;
        
        [Header("Settings")]
        [SerializeField] private float _destroyDelay = 3f;
        
        private ElementalComboSystem _comboSystem;
        
        private void Awake()
        {
            _comboSystem = GetComponent<ElementalComboSystem>();
            if (_comboSystem == null)
                _comboSystem = FindObjectOfType<ElementalComboSystem>();
        }
        
        private void OnEnable()
        {
            if (_comboSystem != null)
                _comboSystem.OnComboTriggered += HandleComboTriggered;
        }
        
        private void OnDisable()
        {
            if (_comboSystem != null)
                _comboSystem.OnComboTriggered -= HandleComboTriggered;
        }
        
        private void HandleComboTriggered(ComboType combo, Vector3 position)
        {
            GameObject prefab = GetComboPrefab(combo);
            if (prefab == null) return;
            
            GameObject fx = Instantiate(prefab, position, Quaternion.identity);
            Destroy(fx, _destroyDelay);
        }
        
        private GameObject GetComboPrefab(ComboType combo)
        {
            return combo switch
            {
                ComboType.SteamExplosion => _steamExplosionVFX,
                ComboType.Shatter => _shatterVFX,
                ComboType.ToxicCloud => _toxicCloudVFX,
                ComboType.ElectroPoison => _electroPoisonVFX,
                _ => null
            };
        }
    }
}
