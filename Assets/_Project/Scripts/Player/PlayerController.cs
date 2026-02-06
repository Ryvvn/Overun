using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Overun.Player
{
    /// <summary>
    /// Controls player movement and camera rotation using CharacterController.
    /// Implements 8-directional movement with WASD, mouse look, sprint, stamina, and jump.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _sprintSpeedMultiplier = 1.5f;
        [SerializeField] private float _gravity = -20f;
        
        [Header("Jump Settings")]
        [SerializeField] private float _jumpHeight = 2f;
        
        [Header("Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaDrainRate = 20f;
        [SerializeField] private float _staminaRegenRate = 15f;
        [SerializeField] private float _minStaminaToSprint = 20f;
        [SerializeField] private float _staminaRegenDelay = 0.5f;
        
        [Header("Mouse Look Settings")]
        [SerializeField] private float _mouseSensitivity = 100f;
        [SerializeField] private float _pitchMin = -60f;
        [SerializeField] private float _pitchMax = 60f;
        
        [Header("Camera")]
        [SerializeField] private Transform _cameraTarget;
        
        [Header("Ground Check")]
        [SerializeField] private float _groundCheckDistance = 0.1f;
        
        // Components
        private CharacterController _controller;
        private PlayerInputActions _inputActions;
        
        // Movement State
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Vector3 _velocity;
        private bool _isGrounded;
        private float _yaw;
        private float _pitch;
        
        // Sprint State
        private bool _sprintInputHeld;
        private bool _isSprinting;
        private float _currentStamina;
        private float _lastSprintTime;
        
        // Jump State
        private bool _jumpRequested;
        
        // Camera reference
        private Transform _cameraTransform;
        
        // Events
        public event Action<float, float> OnStaminaChanged;
        public event Action OnJump;
        
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _inputActions = new PlayerInputActions();
            
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
            
            _yaw = transform.eulerAngles.y;
            _currentStamina = _maxStamina;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }
        
        private void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _sprintInputHeld = true;
            }
            else if (context.canceled)
            {
                _sprintInputHeld = false;
            }
        }
        
        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _jumpRequested = true;
            }
        }
        
        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.performed += OnMove;
            _inputActions.Player.Move.canceled += OnMove;
            _inputActions.Player.Look.performed += OnLook;
            _inputActions.Player.Look.canceled += OnLook;
            _inputActions.Player.Sprint.performed += OnSprint;
            _inputActions.Player.Sprint.canceled += OnSprint;
            _inputActions.Player.Jump.performed += OnJumpInput;
        }
        
        private void OnDisable()
        {
            _inputActions.Player.Move.performed -= OnMove;
            _inputActions.Player.Move.canceled -= OnMove;
            _inputActions.Player.Look.performed -= OnLook;
            _inputActions.Player.Look.canceled -= OnLook;
            _inputActions.Player.Sprint.performed -= OnSprint;
            _inputActions.Player.Sprint.canceled -= OnSprint;
            _inputActions.Player.Jump.performed -= OnJumpInput;
            _inputActions.Player.Disable();
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleGroundCheck();
            HandleJump();
            HandleSprint();
            HandleStamina();
            HandleMovement();
            HandleGravity();
        }
        
        private void HandleMouseLook()
        {
            float mouseX = _lookInput.x * _mouseSensitivity * Time.deltaTime;
            float mouseY = _lookInput.y * _mouseSensitivity * Time.deltaTime;
            
            _yaw += mouseX;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            
            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
            
            if (_cameraTarget != null)
            {
                _cameraTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }
        
        private void HandleGroundCheck()
        {
            _isGrounded = _controller.isGrounded;
            
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
        
        private void HandleJump()
        {
            if (_jumpRequested && _isGrounded)
            {
                // Calculate jump velocity: v = sqrt(2 * g * h)
                // We use positive gravity value for the formula
                _velocity.y = Mathf.Sqrt(2f * Mathf.Abs(_gravity) * _jumpHeight);
                
                OnJump?.Invoke();
            }
            
            _jumpRequested = false;
        }
        
        private void HandleSprint()
        {
            bool isMoving = _moveInput.sqrMagnitude > 0.01f;
            bool canStartSprinting = _currentStamina >= _minStaminaToSprint;
            bool hasStamina = _currentStamina > 0f;
            
            if (_sprintInputHeld && isMoving && hasStamina && _isGrounded)
            {
                if (_isSprinting || canStartSprinting)
                {
                    _isSprinting = true;
                    _lastSprintTime = Time.time;
                }
            }
            else
            {
                _isSprinting = false;
            }
        }
        
        private void HandleStamina()
        {
            float previousStamina = _currentStamina;
            
            if (_isSprinting)
            {
                _currentStamina -= _staminaDrainRate * Time.deltaTime;
                _currentStamina = Mathf.Max(0f, _currentStamina);
                
                if (_currentStamina <= 0f)
                {
                    _isSprinting = false;
                }
            }
            else
            {
                if (Time.time - _lastSprintTime >= _staminaRegenDelay)
                {
                    _currentStamina += _staminaRegenRate * Time.deltaTime;
                    _currentStamina = Mathf.Min(_maxStamina, _currentStamina);
                }
            }
            
            if (!Mathf.Approximately(previousStamina, _currentStamina))
            {
                OnStaminaChanged?.Invoke(_currentStamina, _maxStamina);
            }
        }
        
        private void HandleMovement()
        {
            Vector3 moveDirection;
            
            if (_cameraTransform != null)
            {
                Vector3 forward = _cameraTransform.forward;
                Vector3 right = _cameraTransform.right;
                
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
                
                moveDirection = forward * _moveInput.y + right * _moveInput.x;
            }
            else
            {
                moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            }
            
            float currentSpeed = _isSprinting ? _moveSpeed * _sprintSpeedMultiplier : _moveSpeed;
            
            _controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        
        private void HandleGravity()
        {
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
        
        #region Public Properties
        
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }
        
        public float SprintSpeedMultiplier
        {
            get => _sprintSpeedMultiplier;
            set => _sprintSpeedMultiplier = value;
        }
        
        public float JumpHeight
        {
            get => _jumpHeight;
            set => _jumpHeight = value;
        }
        
        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set => _mouseSensitivity = value;
        }
        
        public bool IsGrounded => _isGrounded;
        public bool IsSprinting => _isSprinting;
        public bool IsJumping => !_isGrounded && _velocity.y > 0;
        public Vector2 MoveInput => _moveInput;
        public float Pitch => _pitch;
        public float Yaw => _yaw;
        
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => _maxStamina;
        public float StaminaPercent => _currentStamina / _maxStamina;
        
        #endregion
    }
}
