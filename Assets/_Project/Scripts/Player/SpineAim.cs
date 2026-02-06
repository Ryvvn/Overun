using UnityEngine;

/// <summary>
/// Makes the character's upper body (spine) aim where the camera looks.
/// Attach to player and assign spine bones.
/// </summary>
public class SpineAim : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the Camera (the transform that rotates for vertical look)")]
    [SerializeField] private Transform _cameraTransform;  // Player camera
    [SerializeField] private Transform _spine;            // First spine bone
    [SerializeField] private Transform _chest;            // Optional: chest/spine2
    [SerializeField] private Transform _upperChest;       // Optional: upper chest/spine3
    
    [Header("Arm (for precise aiming)")]
    [SerializeField] private Transform _rightShoulder;    // Optional: right shoulder
    [SerializeField] private Transform _rightUpperArm;    // Optional: right upper arm
    [SerializeField] private float _armAimMultiplier = 0.5f;  // Extra arm rotation
    
    [Header("Settings")]
    [Tooltip("1.0 = spine fully follows camera pitch")]
    [SerializeField] private float _aimWeight = 1.0f;
    [Tooltip("Max degrees spine can rotate")]
    [SerializeField] private float _maxVerticalAngle = 60f;
    [SerializeField] private float _smoothSpeed = 15f;
    [SerializeField] private bool _onlyWhenHolding = true;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebug = false;
    
    //private PlayerEquipment _equipment;
    private float _currentAimAngle;
    private float _targetAimAngle;
    
    // Store initial rotations to apply from (set by animation each frame)
    private Quaternion _spineBaseRotation;
    private Quaternion _chestBaseRotation;
    private Quaternion _upperChestBaseRotation;
    private Quaternion _rightShoulderBaseRotation;
    private Quaternion _rightUpperArmBaseRotation;
    
    private void Awake()
    {
        //_equipment = GetComponent<PlayerEquipment>();
        
        // Try to find camera if not assigned
        if (_cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
                _cameraTransform = cam.transform;
        }
    }
    
    private void LateUpdate()
    {
        if (_spine == null || _cameraTransform == null) return;
        
        // Store base rotations BEFORE we modify them (set by Animator)
        CacheBaseRotations();
        
        // Check if we should aim (only when holding weapon)
        bool shouldAim = true;
        if (_onlyWhenHolding)
        {
            shouldAim = false;
        }
        
        // Calculate target angle from camera pitch
        _targetAimAngle = shouldAim ? GetCameraPitch() : 0f;
        _targetAimAngle = Mathf.Clamp(_targetAimAngle, -_maxVerticalAngle, _maxVerticalAngle);
        
        // Smooth interpolation
        _currentAimAngle = Mathf.Lerp(_currentAimAngle, _targetAimAngle, _smoothSpeed * Time.deltaTime);
        
        // Apply rotation to spine bones
        ApplySpineRotation();
    }
    
    private void CacheBaseRotations()
    {
        if (_spine != null) _spineBaseRotation = _spine.localRotation;
        if (_chest != null) _chestBaseRotation = _chest.localRotation;
        if (_upperChest != null) _upperChestBaseRotation = _upperChest.localRotation;
        if (_rightShoulder != null) _rightShoulderBaseRotation = _rightShoulder.localRotation;
        if (_rightUpperArm != null) _rightUpperArmBaseRotation = _rightUpperArm.localRotation;
    }
    
    private float GetCameraPitch()
    {
        if (_cameraTransform == null) return 0f;
        
        // Get camera's local X rotation (pitch)
        float pitch = _cameraTransform.localEulerAngles.x;
        
        // Convert from 0-360 to -180 to 180 range
        if (pitch > 180f)
            pitch -= 360f;
        
        return pitch;
    }
    
    private void ApplySpineRotation()
    {
        // Calculate total rotation needed
        float totalRotation = _currentAimAngle * _aimWeight;
        
        // Count how many bones we have
        int boneCount = 0;
        if (_spine != null) boneCount++;
        if (_chest != null) boneCount++;
        if (_upperChest != null) boneCount++;
        
        if (boneCount == 0) return;
        
        // Each bone gets a portion of the total rotation
        float rotationPerBone = totalRotation / boneCount;
        
        // Apply rotation from base rotation (not cumulative)
        if (_spine != null)
        {
            _spine.localRotation = _spineBaseRotation * Quaternion.Euler(rotationPerBone, 0, 0);
        }
        
        if (_chest != null)
        {
            _chest.localRotation = _chestBaseRotation * Quaternion.Euler(rotationPerBone, 0, 0);
        }
        
        if (_upperChest != null)
        {
            _upperChest.localRotation = _upperChestBaseRotation * Quaternion.Euler(rotationPerBone, 0, 0);
        }
        
        // Apply extra rotation to arm for precise aiming
        // Negative multiplier to correct the direction 
        float armRotation = -_currentAimAngle * _armAimMultiplier;
        
        if (_rightShoulder != null)
        {
            _rightShoulder.localRotation = _rightShoulderBaseRotation * Quaternion.Euler(armRotation * 0.3f, 0, 0);
        }
        
        if (_rightUpperArm != null)
        {
            _rightUpperArm.localRotation = _rightUpperArmBaseRotation * Quaternion.Euler(armRotation * 0.7f, 0, 0);
        }
        
        if (_showDebug)
        {
            Debug.Log($"[SpineAim] Camera: {_targetAimAngle:F1}° → Spine: {rotationPerBone:F1}°/bone, Arm: {armRotation:F1}°");
        }
    }
    
    /// <summary>
    /// Call this to find spine bones automatically from Humanoid rig.
    /// </summary>
    public void AutoFindBones()
    {
        var animator = GetComponent<Animator>();
        if (animator == null || !animator.isHuman) return;
        
        _spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        _chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        _upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
        _rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        _rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        
        Debug.Log($"[SpineAim] Found bones - Spine: {_spine?.name}, Chest: {_chest?.name}, UpperChest: {_upperChest?.name}, RightShoulder: {_rightShoulder?.name}, RightUpperArm: {_rightUpperArm?.name}");
    }
}
