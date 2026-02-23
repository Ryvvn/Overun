using UnityEngine;

namespace Overun.Player
{
    /// <summary>
    /// Drives Animator parameters from PlayerController movement state.
    /// Attach to the Player GameObject alongside PlayerController and an Animator.
    /// 
    /// Expected Animator Parameters:
    ///   - Speed (float): 0 = idle, ~3 = walk, ~6 = run, ~9 = sprint
    ///   - IsGrounded (bool): true when on ground
    ///   - IsJumping (bool): true when ascending in air
    ///   - IsSprinting (bool): true when sprinting
    ///   - Jump (trigger): fires on jump start
    ///   - Die (trigger): fires on death
    ///   - Land (trigger): fires on landing
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Speed Smoothing")]
        [SerializeField] private float _speedSmoothTime = 0.1f;
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _sprintSpeed = 9f;
        
        // Cached hash IDs (no string alloc per frame)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int LandHash = Animator.StringToHash("Land");
        
        private PlayerController _controller;
        private Animator _animator;
        private PlayerHealth _health;
        
        private float _currentSpeed;
        private float _speedSmoothVelocity;
        private bool _wasGrounded;
        private bool _isDead;
        
        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
            _health = GetComponent<PlayerHealth>();
        }
        
        private void OnEnable()
        {
            if (_controller != null)
                _controller.OnJump += HandleJump;
                
            if (_health != null)
                _health.OnDeath += HandleDeath;
        }
        
        private void OnDisable()
        {
            if (_controller != null)
                _controller.OnJump -= HandleJump;
                
            if (_health != null)
                _health.OnDeath -= HandleDeath;
        }
        
        private void Update()
        {
            if (_isDead) return;
            
            UpdateSpeed();
            UpdateBools();
            DetectLanding();
        }
        
        private void UpdateSpeed()
        {
            // Calculate target speed from input magnitude and sprint state
            float inputMagnitude = _controller.MoveInput.magnitude;
            float targetSpeed;
            
            if (inputMagnitude < 0.01f)
            {
                targetSpeed = 0f;
            }
            else if (_controller.IsSprinting)
            {
                targetSpeed = _sprintSpeed * inputMagnitude;
            }
            else
            {
                // Blend between walk and run based on input magnitude
                // Full stick = run speed, half stick = walk speed
                targetSpeed = Mathf.Lerp(_walkSpeed, _runSpeed, inputMagnitude) * inputMagnitude;
            }
            
            // Smooth speed transitions
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);
            
            _animator.SetFloat(SpeedHash, _currentSpeed);
        }
        
        private void UpdateBools()
        {
            _animator.SetBool(IsGroundedHash, _controller.IsGrounded);
            _animator.SetBool(IsJumpingHash, _controller.IsJumping);
            _animator.SetBool(IsSprintingHash, _controller.IsSprinting);
        }
        
        private void DetectLanding()
        {
            // Detect transition from airborne to grounded
            if (_controller.IsGrounded && !_wasGrounded)
            {
                _animator.SetTrigger(LandHash);
            }
            _wasGrounded = _controller.IsGrounded;
        }
        
        private void HandleJump()
        {
            _animator.SetTrigger(JumpHash);
        }
        
        private void HandleDeath()
        {
            _isDead = true;
            _animator.SetTrigger(DieHash);
        }
    }
}
