using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IDamageable
{
    private Inputs _playerInputs;
    private CharacterController _characterController;
    private EdgeHanging _edgeHanging;

    #region Health
    [SerializeField]
    private float _maxHealth;

    private float _health;
    #endregion

    #region Movement variables
    [Header("Movement")]
    [SerializeField]
    private float _speed = 1f;

    [SerializeField, Range(0, 1)]
    private float _walkSpeedMultiplier;
    public bool walkPressed { get; private set; }
    private Vector3 _movement;
    private float _targetAngle;
    #endregion

    #region Jump variables
    public bool jumpPressed { get; private set; }
    private bool _isJumping = false;
    private float _initialJumpVel = 0f;

    [Header("Jump")]
    [SerializeField, Range(.25f, 3)]
    private float _jumpTime = .75f;

    [SerializeField, Range(0, 10)]
    private float _jumpHeight = 4f;
    #endregion

    #region Rotation variables
    [Header("Rotation")]
    [SerializeField, Range(.1f, 2)]
    private float _turnSmoothTime = .1f;

    private Transform _camera;
    private float _turnSmoothVelocity;
    #endregion 

    #region Gravity variables
    private float _gravity = -9.8f;
    private float _groundedGravity = -.05f;
    private bool _isFalling = false;
    #endregion

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _edgeHanging = GetComponent<EdgeHanging>();
        _health = _maxHealth;

        _playerInputs = new Inputs();
        _playerInputs.Player.Jump.started += onJump;
        _playerInputs.Player.Jump.canceled += onJump;

        _playerInputs.Player.GrabAndWalk.started += _ => walkPressed = true;
        _playerInputs.Player.GrabAndWalk.canceled += _ => walkPressed = false;

        SetupJump();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Update()
    {
        if (!_edgeHanging.isHanging)
        {
            Gravity();
            Rotation();
            HandleJump();
        }

        Movement();
    }

    #region Enabling inputs
    void OnEnable()
    {
        _playerInputs.Player.Enable();
    }

    void OnDisable()
    {
        _playerInputs.Player.Disable();
    }
    #endregion

    #region Methods
    private void Gravity()
    {
        _isFalling = _movement.y <= 0f || !jumpPressed;
        float fallMultiplier = 2f;

        if (_characterController.isGrounded)
        {
            _movement.y = _groundedGravity;
        } 
        else if (_isFalling)
        {
            float previousGravity = _movement.y;
            float newGravity = _movement.y + (_gravity *  fallMultiplier * Time.deltaTime);
            _movement.y = (previousGravity + newGravity) * .5f;
        }
        else
        {
            float previousGravity = _movement.y;
            float newGravity = _movement.y + (_gravity * Time.deltaTime);
            _movement.y = (previousGravity + newGravity) * .5f;
        }
    }

    private void Rotation()
    {
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref _turnSmoothVelocity, _turnSmoothTime); 

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void Movement()
    {
        Vector2 movementInput = _playerInputs.Player.Move.ReadValue<Vector2>();

        if (_edgeHanging.isHanging && !_edgeHanging.isHangingOnFur) _movement = transform.right * movementInput;
        else if (_edgeHanging.isHanging && _edgeHanging.isHangingOnFur)
        {
            _movement = transform.right * movementInput.x + transform.up * movementInput.y;
        }
        else
        {
            _movement.x = movementInput.x;
            _movement.z = movementInput.y;
        }

        Vector3 normalizedMovement = new Vector3(_movement.x, 0f, _movement.z).normalized;

        if (normalizedMovement.magnitude >= .1f)
        {
            Vector3 moveDirection = normalizedMovement;

            // If we are hanging we don't want the user to move based on camera!
            if (!_edgeHanging.isHanging && !_edgeHanging.isHangingOnFur)
            {
                _targetAngle = Mathf.Atan2(normalizedMovement.x, normalizedMovement.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;

                moveDirection = (Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward).normalized;
            }

            _movement.x = !walkPressed ? moveDirection.x * _speed : moveDirection.x * _speed * _walkSpeedMultiplier;
            _movement.z = !walkPressed ? moveDirection.z * _speed : moveDirection.z * _speed * _walkSpeedMultiplier;
        }

        _characterController.Move(_movement * Time.deltaTime);
    }

    public void Damage(float healthDamage, float stunTime = 0)
    {
        // TODO: stun

        _health -= healthDamage;
    }

    #region Jump
    private void SetupJump()
    {
        float timeToApex = _jumpTime / 2;

        _gravity = (-2 * _jumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVel = (2 * _jumpHeight) / timeToApex;
    }

    private void HandleJump()
    {
        if (!_isJumping && _characterController.isGrounded && jumpPressed)
        {
            _isJumping = true;
            _movement.y = _initialJumpVel * .5f;
        }
        else if (!jumpPressed && _isJumping && _characterController.isGrounded)
        {
            _isJumping = false;
        }
    }

    private void onJump(InputAction.CallbackContext context)
    {
        jumpPressed = context.ReadValueAsButton();
    }
    #endregion
    #endregion
}
