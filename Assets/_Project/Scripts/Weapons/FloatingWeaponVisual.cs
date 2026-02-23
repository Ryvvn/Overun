using UnityEngine;

namespace Overun.Weapons
{
    /// <summary>
    /// Visual representation of a single floating weapon orbiting the player.
    /// Handles position lerping, sinusoidal bobbing, scale emphasis, and enemy-facing rotation.
    /// </summary>
    public class FloatingWeaponVisual : MonoBehaviour
    {
        [Header("Bobbing")]
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobSpeed = 2f;
        
        [Header("Movement")]
        [SerializeField] private float _positionLerpSpeed = 5f;
        [SerializeField] private float _rotationLerpSpeed = 8f;
        
        [Header("Selected Emphasis")]
        [SerializeField] private float _defaultScale = 0.6f;
        [SerializeField] private float _selectedScale = 0.8f;
        [SerializeField] private float _scaleLerpSpeed = 8f;
        
        // Runtime state
        private Vector3 _targetOrbitPosition;
        private float _bobPhaseOffset;
        private float _targetScaleValue;
        private bool _isSelected;
        private Transform _playerTransform;
        private WeaponInstance _weaponInstance;
        private GameObject _modelInstance;
        
        // Cached to avoid per-frame alloc
        private Vector3 _currentVelocity;
        
        /// <summary>
        /// The weapon instance this visual represents.
        /// </summary>
        public WeaponInstance WeaponInstance => _weaponInstance;
        
        /// <summary>
        /// Initialize this floating weapon visual.
        /// </summary>
        public void Initialize(WeaponInstance weaponInstance, Transform playerTransform, float phaseOffset)
        {
            _weaponInstance = weaponInstance;
            _playerTransform = playerTransform;
            _bobPhaseOffset = phaseOffset;
            _targetScaleValue = _defaultScale;
            
            // Spawn the weapon model
            SpawnModel();
            
            // Start at target position immediately
            transform.position = _playerTransform.position + _targetOrbitPosition;
            transform.localScale = Vector3.one * _defaultScale;
        }
        
        /// <summary>
        /// Set the target orbital position (local offset from player).
        /// </summary>
        public void SetTargetPosition(Vector3 localOffset)
        {
            _targetOrbitPosition = localOffset;
        }
        
        /// <summary>
        /// Set whether this weapon is the currently selected weapon.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            _targetScaleValue = selected ? _selectedScale : _defaultScale;
        }
        
        /// <summary>
        /// Face toward a target position (enemy or forward).
        /// </summary>
        public void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction.sqrMagnitude < 0.001f) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationLerpSpeed * Time.deltaTime);

        }
        
        /// <summary>
        /// Face the player's forward direction (no enemy nearby).
        /// </summary>
        public void FaceForward()
        {
            if (_playerTransform == null) return;
            Quaternion targetRotation = Quaternion.LookRotation(_playerTransform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationLerpSpeed * Time.deltaTime);
        }
        
        private void Update()
        {
            if (_playerTransform == null) return;
            
            // Calculate bobbing offset
            float bobOffset = Mathf.Sin((Time.time * _bobSpeed) + _bobPhaseOffset) * _bobAmplitude;
            
            // Target world position = player pos + orbit offset + bob
            Vector3 targetWorldPos = _playerTransform.position + _targetOrbitPosition;
            targetWorldPos.y += bobOffset;
            
            // Smooth position lerp
            transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref _currentVelocity, 1f / _positionLerpSpeed);
            
            // Smooth scale lerp
            float currentScale = transform.localScale.x;
            float newScale = Mathf.Lerp(currentScale, _targetScaleValue, _scaleLerpSpeed * Time.deltaTime);
            transform.localScale = Vector3.one * newScale;
        }
        
        private void SpawnModel()
        {
            // Clean up existing model
            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
            }
            
            GameObject modelPrefab = _weaponInstance?.Data?.WeaponModelPrefab;
            if (modelPrefab != null)
            {
                _modelInstance = Instantiate(modelPrefab, transform);
                _modelInstance.transform.localPosition = Vector3.zero;
                _modelInstance.transform.localRotation = Quaternion.Euler(_weaponInstance.Data.ModelRotationOffset);
            }
            else
            {
                // Fallback: create a simple cube placeholder
                _modelInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _modelInstance.transform.SetParent(transform);
                _modelInstance.transform.localPosition = Vector3.zero;
                _modelInstance.transform.localScale = Vector3.one * 0.3f;
                
                // Remove collider from placeholder
                var col = _modelInstance.GetComponent<Collider>();
                if (col != null) Destroy(col);
                
                // Tint by rarity color
                var renderer = _modelInstance.GetComponent<Renderer>();
                if (renderer != null && _weaponInstance?.Data != null)
                {
                    renderer.material.color = WeaponData.GetRarityColor(_weaponInstance.Data.Rarity);
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
            }
        }
    }
}
