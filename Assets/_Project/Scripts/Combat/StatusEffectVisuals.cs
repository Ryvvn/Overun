using UnityEngine;
using System.Collections.Generic;

namespace Overun.Combat
{
    /// <summary>
    /// visual feedback for status effects (Tint + VFX).
    /// </summary>
    [RequireComponent(typeof(StatusEffectManager))]
    public class StatusEffectVisuals : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _vfxContainer;
        
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject _fireVFX;
        [SerializeField] private GameObject _iceVFX;
        [SerializeField] private GameObject _lightningVFX;
        [SerializeField] private GameObject _poisonVFX;
        
        private StatusEffectManager _manager;
        private Color _originalColor;
        private Dictionary<ElementType, GameObject> _activeVFX = new Dictionary<ElementType, GameObject>();
        
        private void Awake()
        {
            _manager = GetComponent<StatusEffectManager>();
            
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();
                
            if (_renderer != null)
                _originalColor = _renderer.material.color;
        }
        
        private void OnEnable()
        {
            if (_manager != null)
            {
                _manager.OnEffectApplied += OnEffectApplied;
                _manager.OnEffectRemoved += OnEffectRemoved;
            }
        }
        
        private void OnDisable()
        {
            if (_manager != null)
            {
                _manager.OnEffectApplied -= OnEffectApplied;
                _manager.OnEffectRemoved -= OnEffectRemoved;
            }
        }
        
        private void OnEffectApplied(ElementType element)
        {
            UpdateVisuals();
        }
        
        private void OnEffectRemoved(ElementType element)
        {
            UpdateVisuals();
            
            // Cleanup VFX
            if (_activeVFX.TryGetValue(element, out GameObject vfxInstance))
            {
                if (vfxInstance != null)
                {
                    // Use particle system stop or destroy
                    if (vfxInstance.TryGetComponent<ParticleSystem>(out var particles))
                    {
                        particles.Stop();
                        Destroy(vfxInstance, 1f); // Check destroy delay
                    }
                    else
                    {
                        Destroy(vfxInstance);
                    }
                }
                _activeVFX.Remove(element);
            }
        }
        
        private void UpdateVisuals()
        {
            if (_renderer == null) return;
            
            // Prioritize colors: Fire > Ice > Poison > Lightning > None
            Color targetColor = _originalColor;
            ElementType dominantElement = ElementType.None;
            
            if (_manager.HasEffect(ElementType.Fire))
            {
                targetColor = ElementUtils.GetElementColor(ElementType.Fire);
                dominantElement = ElementType.Fire;
            }
            else if (_manager.HasEffect(ElementType.Ice))
            {
                targetColor = ElementUtils.GetElementColor(ElementType.Ice);
                dominantElement = ElementType.Ice;
            }
            else if (_manager.HasEffect(ElementType.Poison))
            {
                targetColor = ElementUtils.GetElementColor(ElementType.Poison);
                dominantElement = ElementType.Poison;
            }
             else if (_manager.HasEffect(ElementType.Lightning))
            {
                targetColor = ElementUtils.GetElementColor(ElementType.Lightning); 
                 dominantElement = ElementType.Lightning;
            }
            
            _renderer.material.color = targetColor;
            
            // Handle VFX spawning
            EnsureVFX(dominantElement);
        }
        
        private void EnsureVFX(ElementType element)
        {
            // If already active, do nothing
            if (_activeVFX.ContainsKey(element)) return;
            
            GameObject prefab = GetVFXPrefab(element);
            if (prefab != null)
            {
                Transform parent = _vfxContainer != null ? _vfxContainer : transform;
                GameObject instance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
                _activeVFX[element] = instance;
            }
        }
        
        private GameObject GetVFXPrefab(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => _fireVFX,
                ElementType.Ice => _iceVFX,
                ElementType.Lightning => _lightningVFX,
                ElementType.Poison => _poisonVFX,
                _ => null
            };
        }
    }
}
